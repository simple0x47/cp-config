using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Cuplan.Config.Controllers;

[ApiController]
public class VersionController : ControllerBase
{
    private readonly ILogger<VersionController> _logger;

    public VersionController(ILogger<VersionController> logger)
    {
        _logger = logger;
    }

    [Route("api/[controller]")]
    [HttpGet]
    public IActionResult GetVersion()
    {
        Version? version = Assembly.GetExecutingAssembly().GetName().Version;

        if (version is null) return StatusCode(StatusCodes.Status500InternalServerError, "failed to get version");

        return Ok(version.ToString());
    }
}