using Core;
using Cuplan.Config.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace Cuplan.Config.IntegrationTests.Services;

public class MicroconfigConfigBuilderTest : TestBase, IDisposable
{
    private readonly IConfigBuilder _configBuilder;
    private readonly string _targetPath;

    public MicroconfigConfigBuilderTest()
    {
        _targetPath = $"{Directory.GetCurrentDirectory()}/{Guid.NewGuid().ToString()}";
        Mock<IWebHostEnvironment> mockHostEnvironment = new();
        mockHostEnvironment.SetupProperty(h => h.EnvironmentName);
        IWebHostEnvironment hostEnvironment = mockHostEnvironment.Object;
        hostEnvironment.EnvironmentName = "development";

        _configBuilder = new MicroconfigConfigBuilder(hostEnvironment, Config);
    }

    public void Dispose()
    {
        _configBuilder.Dispose();
    }

    [Fact]
    public void Build_BasicTestData_CorrectBuild()
    {
        string basicTestDataPath = $"{TestDataPath}/{GetType().Name}/basic";


        Result<Empty, Error<string>> result = _configBuilder.Build(basicTestDataPath, _targetPath);

        Assert.True(result.IsOk);
        Assert.Equal(new Empty(), result.Unwrap());
        Assert.True(File.Exists($"{_targetPath}/authentication/application.yaml"));
    }
}