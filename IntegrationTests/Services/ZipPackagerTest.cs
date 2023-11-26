using Core;
using Cuplan.Config.Services;
using Xunit;

namespace Cuplan.Config.IntegrationTests.Services;

public class ZipPackagerTest : TestBase, IDisposable
{
    private readonly string _packageFilePath;
    private readonly IPackager _packager;
    private readonly string _sourcePath;

    public ZipPackagerTest()
    {
        _sourcePath = $"{TestDataPath}/{GetType().Name}/basic";
        _packageFilePath = $"{Directory.GetCurrentDirectory()}/{Guid.NewGuid().ToString()}.zip";
        _packager = new ZipPackager();
    }

    public void Dispose()
    {
        _packager.Dispose();
    }

    [Fact]
    public void Package_ValidTargetFile_CreatesZipCorrectly()
    {
        Result<Empty, Error<string>> result = _packager.Package(_sourcePath, _packageFilePath);

        Assert.True(result.IsOk);
        Assert.True(File.Exists(_packageFilePath));
    }
}