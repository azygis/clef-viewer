using Microsoft.AspNetCore.Mvc;

namespace ClefViewer.API.Controllers;

[ApiController]
[Route("live")]
public class LiveController : ControllerBase
{
    [HttpHead]
    public IActionResult Head() => Ok();
}