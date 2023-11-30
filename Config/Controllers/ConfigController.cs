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

    [Route("api/[controller]/get/{microservice}")]
    [HttpGet]
    public IActionResult GetConfig([FromRoute] string microservice)
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
                    _logger.LogWarning($"Failed to obtain configuration: {error.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError, error.ErrorKind);
                case ErrorKind.NotFound:
                    _logger.LogDebug($"Config for microservice '{microservice} not found.");
                    return StatusCode(StatusCodes.Status404NotFound, error.ErrorKind);
            }
        }

        return File(result.Unwrap(), "application/zip", "config.zip");
    }

    [Route("api/[controller]/refresh")]
    [HttpGet]
    public IActionResult Refresh()
    {
        Result<Empty, Error<string>> result = _configProvider.Refresh();

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();
            return StatusCode(StatusCodes.Status500InternalServerError, error.ErrorKind);
        }

        return NoContent();
    }
}