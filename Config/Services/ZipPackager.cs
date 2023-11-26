using Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Cuplan.Config.Services;

public class ZipPackager : IPackager
{
    private readonly IList<string> _packageFiles;

    public ZipPackager()
    {
        _packageFiles = new List<string>();
    }

    public void Dispose()
    {
        foreach (string packageFile in _packageFiles)
        {
            if (!File.Exists(packageFile)) continue;

            File.Delete(packageFile);
        }
    }

    public Result<Empty, Error<string>> Package(string sourcePath, string targetFilePath)
    {
        try
        {
            FastZip fastZip = new();
            fastZip.CreateZip(targetFilePath, sourcePath, true, null);

            _packageFiles.Add(targetFilePath);
        }
        catch (Exception e)
        {
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.PackageFailure,
                $"failed to package zip: {e.Message}"));
        }

        return Result<Empty, Error<string>>.Ok(new Empty());
    }
}