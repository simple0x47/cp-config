using Core;
using Cuplan.Config.Services;

namespace Cuplan.Config.Models;

public class ConfigProvider : IDisposable
{
    private const string ConfigBuildPath = "__config__build__";

    private readonly TimeSpan _acquireReaderLockTimeout;
    private readonly IConfiguration _config;
    private readonly IConfigBuilder _configBuilder;
    private readonly IDownloader _downloader;
    private readonly string _downloadPath;
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
        _downloadPath = _config.GetValue<string>("ConfigProvider:DownloadPath")!;
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
        Result<DownloadResult, Error<string>> downloadResult = _downloader.Download(_downloadPath);

        if (!downloadResult.IsOk) return Result<byte[], Error<string>>.Err(downloadResult.UnwrapErr());

        string packageFilePath = $"package-{component}.{_packager.PackageExtension}";
        string componentPath = $"{ConfigBuildPath}/{component}";

        if (downloadResult.Unwrap() == DownloadResult.NoChanges)
        {
            if (!File.Exists(packageFilePath))
            {
                Result<Empty, Error<string>> result =
                    PackageComponentPath(component, componentPath, packageFilePath);

                if (!result.IsOk) return Result<byte[], Error<string>>.Err(result.UnwrapErr());
            }

            return Result<byte[], Error<string>>.Ok(File.ReadAllBytes(packageFilePath));
        }

        return DownloadBuildAndPackage(component, componentPath, packageFilePath);
    }

    private Result<Empty, Error<string>> PackageComponentPath(string component, string componentPath,
        string packageFilePath)
    {
        if (!Directory.Exists(componentPath))
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.NotFound,
                $"component '{component}' could not be found"));

        Result<Empty, Error<string>> packagerResult = _packager.Package(componentPath, packageFilePath);

        if (!packagerResult.IsOk) return Result<Empty, Error<string>>.Err(packagerResult.UnwrapErr());

        return Result<Empty, Error<string>>.Ok(new Empty());
    }

    private Result<byte[], Error<string>> DownloadBuildAndPackage(string component, string componentPath,
        string packageFilePath)
    {
        ReaderWriterLock readerWriterLock = _downloader.GetReaderWriterLock();

        _downloader.GetReaderWriterLock().AcquireReaderLock(_acquireReaderLockTimeout);

        try
        {
            Result<Empty, Error<string>> configBuilderResult =
                _configBuilder.Build(_downloadPath, ConfigBuildPath);

            if (!configBuilderResult.IsOk)
                return Result<byte[], Error<string>>.Err(configBuilderResult.UnwrapErr());
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

        Result<Empty, Error<string>> packageResult = PackageComponentPath(component, componentPath, packageFilePath);

        if (!packageResult.IsOk) return Result<byte[], Error<string>>.Err(packageResult.UnwrapErr());

        return Result<byte[], Error<string>>.Ok(File.ReadAllBytes(packageFilePath));
    }
}