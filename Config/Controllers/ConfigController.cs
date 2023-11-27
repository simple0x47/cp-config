using Core;
using Cuplan.Config.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cuplan.Config.Controllers;

[ApiController]
public class ConfigController : ControllerBase
{
    private readonly ConfigProvider _configProvider;

    public ConfigController(ConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    [Route("api/[controller]/{microservice}")]
    [HttpGet]
    public async Task<IActionResult> Register([FromRoute] string microservice)
    {
        Result<byte[], Error<string>> result = _configProvider.Generate();

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
            }
        }

        return File(result.Unwrap(), "application/zip", "config.zip");
    }
}