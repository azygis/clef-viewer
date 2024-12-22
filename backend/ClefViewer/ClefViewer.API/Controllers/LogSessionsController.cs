using ClefViewer.API.Business;
using ClefViewer.API.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;

namespace ClefViewer.API.Controllers;

[ApiController]
[Route("log-sessions")]
public sealed class LogSessionsController(ILogSessionProvider logSessionProvider) : ControllerBase
{
    [HttpPost("{sessionId:guid}")]
    public SearchLogEventsResponse SearchLogsAsync([FromRoute] Guid sessionId, [FromBody] SearchLogEventsRequest request, CancellationToken cancellationToken) =>
        logSessionProvider.GetEvents(sessionId, request);
}