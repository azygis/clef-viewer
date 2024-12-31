using ClefViewer.API.Business;
using ClefViewer.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace ClefViewer.API.Controllers;

[ApiController]
[Route("log-sessions")]
public sealed class LogSessionsController(ILogSessionProvider logSessionProvider) : ControllerBase
{
    [HttpGet]
    public IEnumerable<LogSessionDTO> GetSessions() =>
        logSessionProvider.GetSessions();

    [HttpPost("{sessionId:guid}")]
    public SearchLogEventsResponse SearchLogs([FromRoute] Guid sessionId, [FromBody] SearchLogEventsRequest request) =>
        logSessionProvider.GetEvents(sessionId, request);

    [HttpPatch("{sessionId:guid}/track")]
    public void SetChangeTracking(Guid sessionId, [FromQuery] bool track) =>
        logSessionProvider.SetTrackChanges(sessionId, track);

    [HttpPost("{sessionId:guid}/reload")]
    public async Task ReloadAsync(Guid sessionId, [FromQuery(Name = "filePath[]")] string[] filePath, [FromServices] ILogFileReader logFileReader, CancellationToken cancellationToken)
    {
        var logFiles = logSessionProvider.GetSession(sessionId).Files.Where(x => filePath.Contains(x.Path));
        foreach (var logFile in logFiles)
        {
            await logFileReader.AddLogEntriesAsync(logFile, cancellationToken);
        }
    }

    [HttpDelete("{sessionId:guid}")]
    public void DeleteSession(Guid sessionId) =>
        logSessionProvider.DeleteSession(sessionId);
}