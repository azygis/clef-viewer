using System.Collections.Frozen;
using ClefViewer.API.Data;
using ClefViewer.API.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Serilog.Events;
using Serilog.Expressions;

namespace ClefViewer.API.Business;

public interface ILogSessionProvider
{
    Guid AddSession(LogFiles logFiles);
    IEnumerable<LogSessionDTO> GetSessions();
    LogFiles GetSession(Guid sessionId);
    void SetTrackChanges(Guid sessionId, bool track);
    SearchLogEventsResponse GetEvents(Guid sessionId, SearchLogEventsRequest request);
    void DeleteSession(Guid sessionId);
}

public delegate bool EventFilter(LogEvent logEvent);

public class LogSessionProvider(NameResolver nameResolver, IHubContext<LogHub> hubContext) : ILogSessionProvider
{
    private static readonly FrozenSet<char> ExpressionOperators = "@()+=*<>%-".ToCharArray().ToFrozenSet();

    private readonly Dictionary<Guid, LogFiles> _sessions = new();
    private readonly Dictionary<Guid, List<FileSystemWatcher>> _watchers = new();

    public Guid AddSession(LogFiles logFiles)
    {
        var sessionId = Guid.NewGuid();
        _sessions.Add(sessionId, logFiles);
        _watchers.Add(sessionId, ConfigureWatchers());
        return sessionId;

        List<FileSystemWatcher> ConfigureWatchers()
        {
            var watchers = new List<FileSystemWatcher>();
            foreach (var fileGroup in logFiles.Files.GroupBy(x => x.DirectoryPath))
            {
                var watcher = new FileSystemWatcher(fileGroup.Key) { NotifyFilter = NotifyFilters.LastWrite, EnableRaisingEvents = true };
                watcher.Changed += (_, args) => hubContext.Clients.All.SendAsync(LogHub.FilesChanged, sessionId, args.FullPath);
                foreach (var fileName in fileGroup.Select(x => Path.GetFileName(x.Path)))
                {
                    watcher.Filters.Add(fileName);
                }
                watchers.Add(watcher);
            }
            return watchers;
        }
    }

    public IEnumerable<LogSessionDTO> GetSessions() =>
        _sessions.Select(x => new LogSessionDTO(x.Key, x.Value.Files.Sum(y => y.Entries.Count), x.Value.Paths));

    public LogFiles GetSession(Guid sessionId) =>
        _sessions.TryGetValue(sessionId, out var logFiles) ? logFiles : throw new EntityNotFoundException("Session", sessionId);

    public void SetTrackChanges(Guid sessionId, bool track)
    {
        if (_watchers.TryGetValue(sessionId, out var watchers))
        {
            watchers.ForEach(x => x.EnableRaisingEvents = track);
        }
    }

    public SearchLogEventsResponse GetEvents(Guid sessionId, SearchLogEventsRequest request)
    {
        var (pageNumber, pageSize, sortOrder, expression, filters) = request;
        var entries = GetSession(sessionId).Files.SelectMany(x => x.Entries);
        if (filters?.Length > 0)
        {
            var filterExpression = string.Join(" and ", filters.Select(x => $"{x.Property} = '{SerilogExpression.EscapeStringContent(x.Value)}'"));
            if (string.IsNullOrWhiteSpace(expression))
            {
                expression = filterExpression;
            }
            else
            {
                expression += $" and {filterExpression}";
            }
        }
        if (!string.IsNullOrWhiteSpace(expression))
        {
            EventFilter? filter;
            if (!expression.Contains(' ') && !expression.Any(x => ExpressionOperators.Contains(x)))
            {
                filter = MessageLike();
            }
            else if (SerilogExpression.TryCompile(expression, null, nameResolver, out var compiledExpression, out _))
            {
                filter = IsTrue(compiledExpression);
            }
            else
            {
                filter = MessageLike();
            }

            if (filter is not null)
            {
                entries = entries.Where(x => filter(x.Event));
            }
        }
        switch (sortOrder)
        {
            case "desc":
                entries = entries.OrderByDescending(x => x.Timestamp);
                break;
            case "asc":
                entries = entries.OrderBy(x => x.Timestamp);
                break;
        }
        return new SearchLogEventsResponse(entries.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray(), new LogEventCounts(entries));

        EventFilter? MessageLike()
        {
            var filterSearch = $"@m like '%{SerilogExpression.EscapeLikeExpressionContent(expression)}%' ci";
            return SerilogExpression.TryCompile(filterSearch, out var compiledExpression, out _) ? IsTrue(compiledExpression) : null;
        }

        EventFilter IsTrue(CompiledExpression compiledExpression) =>
            evt => ExpressionResult.IsTrue(compiledExpression(evt));
    }

    public void DeleteSession(Guid sessionId)
    {
        var session = GetSession(sessionId);
        session.Files.ForEach(x => x.Entries.Clear());
        session.Files.Clear();
        _sessions.Remove(sessionId);
        var watchers = _watchers[sessionId];
        watchers.ForEach(x =>
        {
            x.EnableRaisingEvents = false;
            x.Dispose();
        });
        watchers.Clear();
        _watchers.Remove(sessionId);
    }
}