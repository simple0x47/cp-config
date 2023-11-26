using Core;
using Cuplan.Config.Services;
using Xunit;

namespace Cuplan.Config.IntegrationTests.Services;

public class GitDownloaderTest : TestBase, IDisposable
{
    private readonly IDownloader _downloader;
    private readonly string _downloadPath;

    public GitDownloaderTest()
    {
        _downloader = new GitDownloader(Config, SecretsManager);
        _downloadPath = $"{Directory.GetCurrentDirectory()}/{Guid.NewGuid().ToString()}";
    }

    public void Dispose()
    {
        _downloader.Dispose();
    }

    [Fact]
    public void Download_ValidPath_Repository()
    {
        Result<Empty, Error<string>> result = _downloader.Download(_downloadPath);

        AssertDownloadSuccess(result);
    }

    [Fact]
    public void Download_AlreadyExistingPath_UpdatesRepository()
    {
        _downloader.Download(_downloadPath);

        Result<Empty, Error<string>> result = _downloader.Download(_downloadPath);

        AssertDownloadSuccess(result);
    }

    private void AssertDownloadSuccess(Result<Empty, Error<string>> result)
    {
        Assert.True(result.IsOk);
        Assert.True(Directory.Exists(_downloadPath));
        Assert.True(Directory.Exists($"{_downloadPath}/.git"));
    }
}