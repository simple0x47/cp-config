using Core;

namespace Cuplan.Config.Services;

public interface IConfigBuilder : IDisposable
{
    /// <summary>
    ///     Builds the configuration directory.
    /// </summary>
    /// <param name="buildPath">Build path where the configuration files to be built are located.</param>
    /// <param name="targetPath">Target path where the build resulting files will be located.</param>
    /// <returns>A string containing the build path or an error.</returns>
    public Result<Empty, Error<string>> Build(string buildPath, string targetPath);
}