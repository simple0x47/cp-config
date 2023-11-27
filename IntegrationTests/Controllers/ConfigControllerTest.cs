using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Cuplan.Config.IntegrationTests.Controllers;

public class ConfigControllerTest : TestBase, IDisposable
{
    private const string ConfigApi = "api/Config";

    private readonly string _targetDirectory;

    private Stream? _contentStream;

    public ConfigControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output) :
        base(factory, output)
    {
        _targetDirectory = $"{Directory.GetCurrentDirectory()}/{Guid.NewGuid().ToString()}";
    }

    public void Dispose()
    {
        _contentStream?.Dispose();

        if (Directory.Exists(_targetDirectory)) Directory.Delete(_targetDirectory, true);
    }

    [Fact]
    public async Task Config_RespondsWithZipFile()
    {
        string dummyConfigFilePath = $"{_targetDirectory}/application.yaml";
        string expectedConfigFile = $"{TestDataPath}/{GetType().Name}/application.yaml";

        HttpResponseMessage response = await Client.GetAsync($"{ConfigApi}/dummy");
        _contentStream = await response.Content.ReadAsStreamAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        FastZip fastZip = new();
        fastZip.ExtractZip(_contentStream, _targetDirectory, FastZip.Overwrite.Always, null, null, null, false, false);

        Assert.True(Directory.Exists(_targetDirectory));
        Assert.NotEmpty(Directory.GetFileSystemEntries(_targetDirectory));
        Assert.True(File.Exists(dummyConfigFilePath));
        Assert.Equal(await File.ReadAllTextAsync(expectedConfigFile), await File.ReadAllTextAsync(dummyConfigFilePath));
    }
}