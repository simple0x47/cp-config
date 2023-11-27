using Core;
using Cuplan.Config.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cuplan.Config.Controllers;

[ApiController]
public class ConfigController : ControllerBase
{
    private readonly ConfigProvider _configProvider;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(ILogger<ConfigController> logger, ConfigProvider configProvider)
    {
        _logger = logger;
        _configProvider = configProvider;
    }

    [Route("api/[controller]/{microservice}")]
    [HttpGet]
    public IActionResult Register([FromRoute] string microservice)
    {
        Result<byte[], Error<string>> result = _configProvider.Generate(microservice);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();

            switch (error.ErrorKind)
            {
                case ErrorKind.DownloadFailure:
                case ErrorKind.PackageFailure:
                case ErrorKind.TimedOut:
                case ErrorKind.UnexpectedNull:
                    return StatusCode(StatusCodes.Status500InternalServerError, error.ErrorKind);
                case ErrorKind.NotFound:
                    _logger.LogDebug($"Config for microservice '{microservice} not found.");
                    return StatusCode(StatusCodes.Status404NotFound, error.ErrorKind);
            }
        }

        return File(result.Unwrap(), "application/zip", "config.zip");
    }
}