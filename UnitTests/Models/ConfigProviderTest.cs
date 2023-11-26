using Core;
using Cuplan.Config.Models;
using Cuplan.Config.Services;
using Moq;
using Xunit;

namespace Cuplan.Config.UnitTests.Models;

public class ConfigProviderTest : TestBase, IDisposable
{
    private readonly ConfigProvider _configProvider;

    public ConfigProviderTest()
    {
        Mock<IDownloader> downloader = new();
        downloader.Setup(d => d.Download(It.IsAny<string>()))
            .Returns(Result<Empty, Error<string>>.Ok(new Empty()));

        Mock<IConfigBuilder> configBuilder = new();
        configBuilder.Setup(c => c.Build(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result<Empty, Error<string>>.Ok(new Empty()));

        Mock<IPackager> packager = new();
        packager.Setup(p => p.Package(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result<Empty, Error<string>>.Ok(new Empty()));

        _configProvider = new ConfigProvider(downloader.Object, configBuilder.Object, packager.Object);
    }

    public void Dispose()
    {
        _configProvider.Dispose();
    }

    [Fact]
    public void Generate_Correctly()
    {
        Result<byte[], Error<string>> result = _configProvider.Generate();

        Assert.True(result.IsOk);
        Assert.NotEmpty(result.Unwrap());
    }
}