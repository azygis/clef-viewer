using System.Collections.Frozen;
using ClefViewer.API.Data;
using ClefViewer.API.Exceptions;
using Serilog.Events;
using Serilog.Expressions;

namespace ClefViewer.API.Business;

public interface ILogSessionProvider
{
    Guid AddSession(LogFiles logFiles);
    LogFiles GetSession(Guid sessionId);
    SearchLogEventsResponse GetEvents(Guid sessionId, SearchLogEventsRequest request);
}

public delegate bool EventFilter(LogEvent logEvent);

public class LogSessionProvider : ILogSessionProvider
{
    private static readonly FrozenSet<char> ExpressionOperators = "@()+=*<>%-".ToCharArray().ToFrozenSet();

    private readonly Dictionary<Guid, LogFiles> _sessions = new();

    public Guid AddSession(LogFiles logFiles)
    {
        var sessionId = Guid.NewGuid();
        _sessions.Add(sessionId, logFiles);
        return sessionId;
    }

    public LogFiles GetSession(Guid sessionId) =>
        _sessions.TryGetValue(sessionId, out var logFiles) ? logFiles : throw new EntityNotFoundException("Session", sessionId);

    public SearchLogEventsResponse GetEvents(Guid sessionId, SearchLogEventsRequest request)
    {
        var (pageNumber, pageSize, sortOrder, expression) = request;
        var entries = GetSession(sessionId).Files.SelectMany(x => x.Entries);
        if (!string.IsNullOrWhiteSpace(expression))
        {
            EventFilter? filter = null;
            if (!expression.Any(x => ExpressionOperators.Contains(x)))
            {
                filter = MessageLike();
            }
            else if (SerilogExpression.TryCompile(expression, out var expressionResult, out var errorMessage))
            {
                filter = evt => ExpressionResult.IsTrue(expressionResult(evt));
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
            if (SerilogExpression.TryCompile(filterSearch, out var expressionResult, out _))
            {
                return evt => ExpressionResult.IsTrue(expressionResult(evt));
            }

            return null;
        }
    }
}