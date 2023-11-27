using Core;
using Cuplan.Config.Services;

namespace Cuplan.Config.Models;

public class ConfigProvider : IDisposable
{
    private readonly TimeSpan _acquireReaderLockTimeout;
    private readonly IConfiguration _config;
    private readonly IConfigBuilder _configBuilder;
    private readonly IDownloader _downloader;
    private readonly IPackager _packager;

    public ConfigProvider(IDownloader downloader, IConfigBuilder configBuilder, IPackager packager,
        IConfiguration config)
    {
        _downloader = downloader;
        _configBuilder = configBuilder;
        _packager = packager;
        _config = config;
        _acquireReaderLockTimeout =
            TimeSpan.FromSeconds(double.Parse(_config["ConfigProvider:AcquireReaderLockTimeout"]));
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
    /// <param name="component">Component which will be packed.</param>
    /// <returns>A byte array containing the package's bytes or an error.</returns>
    public Result<byte[], Error<string>> Generate(string component)
    {
        string? downloadPath = _config.GetValue<string>("ConfigProvider:DownloadPath");

        if (downloadPath is null)
            return Result<byte[], Error<string>>.Err(new Error<string>(ErrorKind.UnexpectedNull,
                "config provider's download path is null"));

        Result<Empty, Error<string>> downloadResult = _downloader.Download(downloadPath, false);

        if (!downloadResult.IsOk) return Result<byte[], Error<string>>.Err(downloadResult.UnwrapErr());

        ReaderWriterLock readerWriterLock = _downloader.GetReaderWriterLock();

        _downloader.GetReaderWriterLock().AcquireReaderLock(_acquireReaderLockTimeout);
        string targetPath = $"t-{Guid.NewGuid().ToString()}";

        try
        {
            Result<Empty, Error<string>> configBuilderResult = _configBuilder.Build(downloadPath, targetPath);

            if (!configBuilderResult.IsOk) return Result<byte[], Error<string>>.Err(configBuilderResult.UnwrapErr());
        }
        catch (Exception e)
        {
            return Result<byte[], Error<string>>.Err(new Error<string>(ErrorKind.UnknownError,
                $"an unexpected exception occurred: {e.Message}"));
        }
        finally
        {
            readerWriterLock.ReleaseReaderLock();
        }

        string componentPath = $"{targetPath}/{component}";

        if (!Directory.Exists(componentPath))
            return Result<byte[], Error<string>>.Err(new Error<string>(ErrorKind.NotFound,
                $"component '{component}' could not be found"));

        string packageFile = $"p-{Guid.NewGuid().ToString()}.{_packager.PackageExtension}";
        Result<Empty, Error<string>> packagerResult = _packager.Package(componentPath, packageFile);

        if (!packagerResult.IsOk) return Result<byte[], Error<string>>.Err(packagerResult.UnwrapErr());

        return Result<byte[], Error<string>>.Ok(File.ReadAllBytes(packageFile));
    }
}