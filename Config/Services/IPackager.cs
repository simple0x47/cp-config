using Core;

namespace Cuplan.Config.Services;

public interface IPackager : IDisposable
{
    public string PackageExtension { get; }

    /// <summary>
    ///     Packages the contents of a directory within a file whose path is the one specified.
    /// </summary>
    /// <param name="sourcePath">Contents of the directory being packaged.</param>
    /// <param name="targetFilePath">Resulting package file.</param>
    /// <returns><see cref="Empty" /> or an error.</returns>
    public Result<Empty, Error<string>> Package(string sourcePath, string targetFilePath);
}