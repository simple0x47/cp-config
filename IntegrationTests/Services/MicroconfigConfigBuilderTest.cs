using Core;
using Cuplan.Config.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Cuplan.Config.IntegrationTests.Services;

public class MicroconfigConfigBuilderTest : TestBase, IDisposable
{
    private readonly string _targetPath;

    public MicroconfigConfigBuilderTest()
    {
        _targetPath = $"{Directory.GetCurrentDirectory()}/{Guid.NewGuid().ToString()}";
    }

    public void Dispose()
    {
        if (!Directory.Exists(_targetPath)) return;

        Directory.Delete(_targetPath, true);
    }

    [Fact]
    public void Build_BasicTestData_CorrectBuild()
    {
        string basicTestDataPath = $"{TestDataPath}/{GetType().Name}/basic";

        Mock<IWebHostEnvironment> mockHostEnvironment = new ();
        mockHostEnvironment.SetupProperty(h => h.EnvironmentName);
        IWebHostEnvironment hostEnvironment = mockHostEnvironment.Object;
        hostEnvironment.EnvironmentName = "development";
        
        IConfigBuilder builder = new MicroconfigConfigBuilder(hostEnvironment, Config);
        Result<Empty, Error<string>> result = builder.Build(basicTestDataPath, _targetPath);

        Assert.True(result.IsOk);
        Assert.Equal(new Empty(), result.Unwrap());
        Assert.True(File.Exists($"{_targetPath}/authentication/application.yaml"));
    }
}