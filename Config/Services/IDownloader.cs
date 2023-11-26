using Core;

namespace Cuplan.Config.Services;

public interface IDownloader : IDisposable
{
    /// <summary>
    ///     Downloads the configuration from somewhere into the specified path.
    ///     Avoid downloading into previously downloaded paths.
    ///     If the path already exists, the content is updated to the latest version.
    /// </summary>
    /// <param name="path">Path where the configuration is downloaded into.</param>
    /// <returns><see cref="Empty" /> or an error.</returns>
    public Result<Empty, Error<string>> Download(string path);
}