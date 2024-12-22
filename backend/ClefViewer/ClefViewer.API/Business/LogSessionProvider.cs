using ClefViewer.API.Data;
using ClefViewer.API.Exceptions;

namespace ClefViewer.API.Business;

public interface ILogSessionProvider
{
    Guid AddSession(LogFiles logFiles);
    LogFiles GetSession(Guid sessionId);
    SearchLogEventsResponse GetEvents(Guid sessionId, SearchLogEventsRequest request);
}

public class LogSessionProvider : ILogSessionProvider
{
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
        var (pageNumber, pageSize) = request;
        var entries = GetSession(sessionId).Files.SelectMany(x => x.Entries);
        return new SearchLogEventsResponse(entries.Skip(pageNumber * pageSize).Take(pageSize).ToArray(), entries.Count());
    }
}