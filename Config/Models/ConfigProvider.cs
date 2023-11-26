using Core;
using Cuplan.Config.Services;

namespace Cuplan.Config.Models;

public class ConfigProvider : IDisposable
{
    private readonly IConfigBuilder _configBuilder;
    private readonly IDownloader _downloader;
    private readonly IPackager _packager;

    public ConfigProvider(IDownloader downloader, IConfigBuilder configBuilder, IPackager packager)
    {
        _downloader = downloader;
        _configBuilder = configBuilder;
        _packager = packager;
    }

    public void Dispose()
    {
        _downloader.Dispose();
        _configBuilder.Dispose();
        _packager.Dispose();
    }

    /// <summary>
    ///     Generates a packed file containing the requested configuration.
    /// </summary>
    /// <returns>A string indicating the package's file path or an error.</returns>
    public Result<string, Error<string>> Generate()
    {
        string downloadPath = $"d-{Guid.NewGuid().ToString()}";
        Result<Empty, Error<string>> downloadResult = _downloader.Download(downloadPath);

        if (!downloadResult.IsOk) return Result<string, Error<string>>.Err(downloadResult.UnwrapErr());

        string targetPath = $"t-{Guid.NewGuid().ToString()}";
        Result<Empty, Error<string>> configBuilderResult = _configBuilder.Build(downloadPath, targetPath);

        if (!configBuilderResult.IsOk) return Result<string, Error<string>>.Err(configBuilderResult.UnwrapErr());

        string packageFile = $"p-{Guid.NewGuid().ToString()}.{_packager.PackageExtension}";
        Result<Empty, Error<string>> packagerResult = _packager.Package(targetPath, packageFile);

        if (!packagerResult.IsOk) return Result<string, Error<string>>.Err(packagerResult.UnwrapErr());

        return Result<string, Error<string>>.Ok(packageFile);
    }
}