using ClefViewer.API.Data;
using ClefViewer.API.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ClefViewer.API.Business;

public sealed record LogSession(LogFiles LogFiles, List<FileSystemWatcher> Watchers);

public interface ILogSessionProvider
{
    Guid AddSession(LogFiles logFiles);
    IEnumerable<LogSessionDTO> GetSessions();
    LogSession GetSession(Guid sessionId);
    SearchLogEventsResponse GetEvents(Guid sessionId, SearchLogEventsRequest request);
    void DeleteSession(Guid sessionId);
}

public sealed class LogSessionProvider(IHubContext<LogHub> hubContext, ILogSessionFilterFactory filterFactory) : ILogSessionProvider, IDisposable
{
    private readonly Dictionary<Guid, LogSession> _sessions = new();

    public Guid AddSession(LogFiles logFiles)
    {
        var sessionId = Guid.NewGuid();
        _sessions.Add(sessionId, new LogSession(logFiles, ConfigureWatchers()));
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
        _sessions.Select(x => new LogSessionDTO(x.Key, x.Value.LogFiles.Files.Sum(y => y.Entries.Count), x.Value.LogFiles.Paths));

    public LogSession GetSession(Guid sessionId) =>
        _sessions.TryGetValue(sessionId, out var session) ? session : throw new EntityNotFoundException("Session", sessionId);

    public SearchLogEventsResponse GetEvents(Guid sessionId, SearchLogEventsRequest request)
    {
        var (pageNumber, pageSize, sortOrder, expression, filters) = request;
        var entries = GetSession(sessionId).LogFiles.Files.SelectMany(x => x.Entries);
        if (filterFactory.TryCreateFilter(expression, filters, out var filter))
        {
            entries = entries.Where(x => filter(x.Event));
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
        // ReSharper disable PossibleMultipleEnumeration because we intentionally enumerate it twice, once for page page events, once for counts
        return new SearchLogEventsResponse(entries.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray(), new LogEventCounts(entries));
        // ReSharper restore PossibleMultipleEnumeration

    }

    public void DeleteSession(Guid sessionId)
    {
        var (logFiles, watchers) = GetSession(sessionId);
        logFiles.Files.ForEach(x => x.Entries.Clear());
        logFiles.Files.Clear();
        watchers.ForEach(x =>
        {
            x.EnableRaisingEvents = false;
            x.Dispose();
        });
        watchers.Clear();
        _sessions.Remove(sessionId);
    }

    public void Dispose() =>
        _sessions.Keys.ToList().ForEach(DeleteSession);
}