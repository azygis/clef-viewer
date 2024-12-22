using ClefViewer.API.Business;
using Microsoft.AspNetCore.Mvc;

namespace ClefViewer.API.Controllers;

[ApiController]
[Route("log-files")]
public sealed class LogFilesController(ILogFileReader logFileReader, ILogSessionProvider logSessionProvider) : ControllerBase
{
    [HttpPost]
    public async Task<Guid> ProcessFilesAsync(string[] files, CancellationToken cancellationToken)
    {
        var logFiles = await logFileReader.ReadLogFilesAsync(files, cancellationToken);
        return logSessionProvider.AddSession(logFiles);
    }
}