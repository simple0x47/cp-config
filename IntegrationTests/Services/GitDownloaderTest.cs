using Core;
using Cuplan.Config.Services;
using Xunit;

namespace Cuplan.Config.IntegrationTests.Services;

public class GitDownloaderTest : TestBase, IDisposable
{
    private readonly IDownloader _downloader;

    public GitDownloaderTest()
    {
        _downloader = new GitDownloader(Config, SecretsManager);
    }

    public void Dispose()
    {
        _downloader.Dispose();
    }

    [Fact]
    public void Download_ValidPath_Repository()
    {
        string downloadPath = GenerateDownloadPath();
        Result<Empty, Error<string>> result = _downloader.Download(downloadPath);

        AssertDownloadSuccess(result, downloadPath);
    }

    [Fact]
    public void Download_AlreadyExistingPath_UpdatesRepository()
    {
        string downloadPath = GenerateDownloadPath();
        _downloader.Download(downloadPath);

        Result<Empty, Error<string>> result = _downloader.Download(downloadPath);

        AssertDownloadSuccess(result, downloadPath);
    }

    private string GenerateDownloadPath()
    {
        return $"{Directory.GetCurrentDirectory()}/{Guid.NewGuid().ToString()}";
    }

    private void AssertDownloadSuccess(Result<Empty, Error<string>> result, string downloadPath)
    {
        Assert.True(result.IsOk);
        Assert.Equal(new Empty(), result.Unwrap());
        Assert.True(Directory.Exists(downloadPath));
        Assert.True(Directory.Exists($"{downloadPath}/.git"));
    }
}