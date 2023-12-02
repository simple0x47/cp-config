using Core;
using Cuplan.Config.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cuplan.Config.HealthChecks;

public class DownloadHealthCheck : IHealthCheck
{
    private const string DummyComponent = "dummy";
    private readonly ConfigProvider _configProvider;

    public DownloadHealthCheck(ConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        Result<byte[], Error<string>> result = _configProvider.Generate(DummyComponent);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();
            return Task.FromResult(HealthCheckResult.Unhealthy($"{error.ErrorKind}: {error.Message}"));
        }

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}