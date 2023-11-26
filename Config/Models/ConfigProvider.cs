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
    /// <returns>A byte array containing the package's bytes or an error.</returns>
    public Result<byte[], Error<string>> Generate()
    {
        string downloadPath = $"d-{Guid.NewGuid().ToString()}";
        Result<Empty, Error<string>> downloadResult = _downloader.Download(downloadPath);

        if (!downloadResult.IsOk) return Result<byte[], Error<string>>.Err(downloadResult.UnwrapErr());

        string targetPath = $"t-{Guid.NewGuid().ToString()}";
        Result<Empty, Error<string>> configBuilderResult = _configBuilder.Build(downloadPath, targetPath);

        if (!configBuilderResult.IsOk) return Result<byte[], Error<string>>.Err(configBuilderResult.UnwrapErr());

        string packageFile = $"p-{Guid.NewGuid().ToString()}.{_packager.PackageExtension}";
        Result<Empty, Error<string>> packagerResult = _packager.Package(targetPath, packageFile);

        if (!packagerResult.IsOk) return Result<byte[], Error<string>>.Err(packagerResult.UnwrapErr());

        return Result<byte[], Error<string>>.Ok(File.ReadAllBytes(packageFile));
    }
}