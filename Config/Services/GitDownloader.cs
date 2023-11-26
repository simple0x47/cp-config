using Core;
using Core.Secrets;
using Cuplan.Config.Utils;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Cuplan.Config.Services;

public class GitDownloader : IDownloader
{
    private readonly CloneOptions _cloneOptions;
    private readonly IConfiguration _config;

    private readonly IList<string> _downloadedPaths;
    private readonly Signature _merger;
    private readonly PullOptions _pullOptions;
    private readonly ISecretsManager _secretsManager;

    public GitDownloader(IConfiguration config, ISecretsManager secretsManager)
    {
        _config = config;
        _secretsManager = secretsManager;

        _downloadedPaths = new List<string>();

        CredentialsHandler handler = (_, _, _) => new SecureUsernamePasswordCredentials
        {
            Username = _secretsManager.Get(_config["GitDownloader:UsernameSecret"]),
            Password = SecretStringProvider.GetSecureStringFromString(
                _secretsManager.Get(_config["GitDownloader:PasswordSecret"]))
        };

        _cloneOptions = new CloneOptions
        {
            CredentialsProvider = handler,
            BranchName = _config["GitDownloader:Branch"]
        };

        _pullOptions = new PullOptions
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = handler
            },
            MergeOptions = new MergeOptions
            {
                MergeFileFavor = MergeFileFavor.Theirs,
                FailOnConflict = false,
                FastForwardStrategy = FastForwardStrategy.Default,
                FileConflictStrategy = CheckoutFileConflictStrategy.Theirs
            }
        };

        _merger = new Signature(_config["GitDownloader:Merge:Name"], _config["GitDownloader:Merge:Email"],
            DateTimeOffset.Now);
    }

    public void Dispose()
    {
        foreach (string downloadedPath in _downloadedPaths)
        {
            if (!Directory.Exists(downloadedPath)) continue;

            Directory.Delete(downloadedPath, true);
        }
    }

    public Result<Empty, Error<string>> Download(string path)
    {
        try
        {
            // Update already existing repository or clone it if it doesn't exist
            if (Directory.Exists(path) && Directory.Exists($"{path}/.git"))
            {
                using Repository repository = new(path);
                Commands.Pull(repository, _merger, _pullOptions);
            }
            else
            {
                Repository.Clone(_config["GitDownloader:Repository"], path, _cloneOptions);

                _downloadedPaths.Add(path);
            }

            return Result<Empty, Error<string>>.Ok(new Empty());
        }
        catch (Exception e)
        {
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.DownloadFailure,
                $"failed to download configuration: {e.Message}"));
        }
    }
}