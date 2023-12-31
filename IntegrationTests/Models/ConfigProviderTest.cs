using Core;
using Cuplan.Config.Models;
using Cuplan.Config.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cuplan.Config.IntegrationTests.Models;

[Collection("Downloader")]
public class ConfigProviderTest : TestBase, IDisposable
{
    private const string EnvironmentName = "development";
    private const string DummyComponent = "dummy";
    private readonly ConfigProvider _configProvider;

    public ConfigProviderTest()
    {
        Mock<ILogger<GitDownloader>> logger = new();
        IDownloader downloader = new GitDownloader(logger.Object, Config, SecretsManager);
        Mock<IWebHostEnvironment> mockHostEnvironment = new();
        mockHostEnvironment.SetupProperty(h => h.EnvironmentName);
        IWebHostEnvironment hostEnvironment = mockHostEnvironment.Object;
        hostEnvironment.EnvironmentName = EnvironmentName;
        IConfigBuilder configBuilder = new MicroconfigConfigBuilder(hostEnvironment, Config);
        IPackager packager = new ZipPackager();
        Mock<ILogger<ConfigProvider>> configProviderLogger = new();

        _configProvider = new ConfigProvider(downloader, configBuilder, packager, Config, configProviderLogger.Object);
    }

    public void Dispose()
    {
        _configProvider.Dispose();
    }

    [Fact]
    public void Generate_Correctly()
    {
        Result<byte[], Error<string>> result = _configProvider.Generate(DummyComponent);

        Assert.True(result.IsOk);
        Assert.NotEmpty(result.Unwrap());
    }
}