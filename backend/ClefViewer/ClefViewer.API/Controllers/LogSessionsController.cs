using ClefViewer.API.Business;
using ClefViewer.API.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;

namespace ClefViewer.API.Controllers;

[ApiController]
[Route("log-sessions/{sessionId:guid}")]
public sealed class LogSessionsController(ILogSessionProvider logSessionProvider) : ControllerBase
{
    [HttpPost]
    public SearchLogEventsResponse SearchLogs([FromRoute] Guid sessionId, [FromBody] SearchLogEventsRequest request) =>
        logSessionProvider.GetEvents(sessionId, request);

    [HttpPatch("track")]
    public void SetChangeTracking(Guid sessionId, [FromQuery] bool track) =>
        logSessionProvider.SetTrackChanges(sessionId, track);

    [HttpPost("reload")]
    public async Task ReloadAsync(Guid sessionId, [FromQuery(Name = "filePath[]")] string[] filePath, [FromServices] ILogFileReader logFileReader, CancellationToken cancellationToken)
    {
        var logFiles = logSessionProvider.GetSession(sessionId).Files.Where(x => filePath.Contains(x.Path));
        foreach (var logFile in logFiles)
        {
            await logFileReader.AddLogEntriesAsync(logFile, cancellationToken);
        }
    }
}