using Core;
using Cuplan.Config.Models;
using Cuplan.Config.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cuplan.Config.IntegrationTests.Services;

[Collection("Downloader")]
public class GitDownloaderTest : TestBase, IDisposable
{
    private readonly IDownloader _downloader;
    private readonly string _downloadPath;

    public GitDownloaderTest()
    {
        Mock<ILogger<GitDownloader>> logger = new();

        _downloader = new GitDownloader(logger.Object, Config, SecretsManager);
        _downloadPath = $"{Directory.GetCurrentDirectory()}/{Guid.NewGuid().ToString()}";
    }

    public void Dispose()
    {
        _downloader.Dispose();
    }

    [Fact]
    public void Download_ValidPath_Repository()
    {
        Result<DownloadResult, Error<string>> result = _downloader.Download(_downloadPath);

        AssertDownloadSuccess(result);
    }

    [Fact]
    public void Download_AlreadyExistingPath_UpdatesRepository()
    {
        _downloader.Download(_downloadPath);

        Result<DownloadResult, Error<string>> result = _downloader.Download(_downloadPath);

        AssertDownloadSuccess(result);
    }

    private void AssertDownloadSuccess(Result<DownloadResult, Error<string>> result)
    {
        Assert.True(result.IsOk);
        Assert.True(Directory.Exists(_downloadPath));
        Assert.True(Directory.Exists($"{_downloadPath}/.git"));
    }
}