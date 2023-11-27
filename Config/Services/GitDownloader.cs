using Core;
using Core.Secrets;
using Cuplan.Config.Utils;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Cuplan.Config.Services;

public class GitDownloader : IDownloader
{
    private readonly TimeSpan _acquireWriterLockTimeout;
    private readonly CloneOptions _cloneOptions;
    private readonly IConfiguration _config;
    private readonly IList<string> _downloadedPaths;
    private readonly ReaderWriterLock _lock;
    private readonly Signature _merger;
    private readonly PullOptions _pullOptions;

    public GitDownloader(IConfiguration config, ISecretsManager secretsManager)
    {
        _config = config;

        _downloadedPaths = new List<string>();

        CredentialsHandler handler = (_, _, _) => new SecureUsernamePasswordCredentials
        {
            Username = secretsManager.Get(_config["GitDownloader:UsernameSecret"]),
            Password = SecretStringProvider.GetSecureStringFromString(
                secretsManager.Get(_config["GitDownloader:PasswordSecret"]))
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

        _acquireWriterLockTimeout =
            TimeSpan.FromSeconds(double.Parse(_config["GitDownloader:AcquireWriterLockTimeout"]));
        _lock = new ReaderWriterLock();
    }

    public void Dispose()
    {
        foreach (string downloadedPath in _downloadedPaths)
        {
            if (!Directory.Exists(downloadedPath)) continue;

            Directory.Delete(downloadedPath, true);
        }
    }

    public Result<Empty, Error<string>> Download(string path, bool update)
    {
        try
        {
            // Update already existing repository or clone it if it doesn't exist
            if (Directory.Exists(path) && Directory.Exists($"{path}/.git"))
            {
                if (!update) return Result<Empty, Error<string>>.Ok(new Empty());

                Result<Empty, Error<string>> result = PullChanges(path);

                if (!result.IsOk) return Result<Empty, Error<string>>.Err(result.UnwrapErr());
            }
            else
            {
                Result<Empty, Error<string>> result = CloneRepository(path);

                if (!result.IsOk) return Result<Empty, Error<string>>.Err(result.UnwrapErr());
            }

            return Result<Empty, Error<string>>.Ok(new Empty());
        }
        catch (Exception e)
        {
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.DownloadFailure,
                $"failed to download configuration: {e.Message}"));
        }
    }

    public ReaderWriterLock GetReaderWriterLock()
    {
        return _lock;
    }

    private Result<Empty, Error<string>> PullChanges(string path)
    {
        _lock.AcquireWriterLock(_acquireWriterLockTimeout);

        try
        {
            using Repository repository = new(path);
            Commands.Pull(repository, _merger, _pullOptions);

            return Result<Empty, Error<string>>.Ok(new Empty());
        }
        catch (Exception e)
        {
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.DownloadFailure,
                $"pulling changes from repository has thrown an exception: {e.Message}"));
        }
        finally
        {
            _lock.ReleaseWriterLock();
        }
    }

    private Result<Empty, Error<string>> CloneRepository(string path)
    {
        _lock.AcquireWriterLock(_acquireWriterLockTimeout);

        try
        {
            Repository.Clone(_config["GitDownloader:Repository"], path, _cloneOptions);

            _downloadedPaths.Add(path);

            return Result<Empty, Error<string>>.Ok(new Empty());
        }
        catch (Exception e)
        {
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.DownloadFailure,
                $"cloning repository has thrown an exception: {e.Message}"));
        }
        finally
        {
            _lock.ReleaseWriterLock();
        }
    }
}