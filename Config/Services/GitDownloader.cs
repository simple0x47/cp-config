using Core;
using Core.Secrets;
using Cuplan.Config.Models;
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
    private readonly ILogger<GitDownloader> _logger;
    private readonly Signature _merger;
    private readonly PullOptions _pullOptions;
    private readonly ISecretsManager _secretsManager;

    public GitDownloader(ILogger<GitDownloader> logger, IConfiguration config, ISecretsManager secretsManager)
    {
        _logger = logger;
        _config = config;
        _secretsManager = secretsManager;

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

    public ReaderWriterLock GetReaderWriterLock()
    {
        return _lock;
    }

    public Result<DownloadResult, Error<string>> Download(string path)
    {
        try
        {
            Result<DownloadResult, Error<string>> result;
            // Update already existing repository or clone it if it doesn't exist
            if (Directory.Exists(path) && Directory.Exists($"{path}/.git"))
                result = PullChanges(path);
            else
                result = CloneRepository(path);

            return result;
        }
        catch (Exception e)
        {
            return Result<DownloadResult, Error<string>>.Err(new Error<string>(ErrorKind.DownloadFailure,
                $"failed to download configuration: {e.Message}"));
        }
    }

    private Result<DownloadResult, Error<string>> PullChanges(string path)
    {
        _lock.AcquireWriterLock(_acquireWriterLockTimeout);

        try
        {
            using Repository repository = new(path);
            MergeResult result = Commands.Pull(repository, _merger, _pullOptions);

            if (result.Status != MergeStatus.UpToDate)
                return Result<DownloadResult, Error<string>>.Ok(DownloadResult.Updated);

            return Result<DownloadResult, Error<string>>.Ok(DownloadResult.NoChanges);
        }
        catch (Exception e)
        {
            return Result<DownloadResult, Error<string>>.Err(new Error<string>(ErrorKind.DownloadFailure,
                $"pulling changes from repository has thrown an exception '{e.GetType().Name}': {e.Message}: {e.StackTrace}"));
        }
        finally
        {
            _lock.ReleaseWriterLock();
        }
    }

    private Result<DownloadResult, Error<string>> CloneRepository(string path)
    {
        _lock.AcquireWriterLock(_acquireWriterLockTimeout);

        try
        {
            Repository.Clone(_config["GitDownloader:Repository"], path, _cloneOptions);

            _downloadedPaths.Add(path);

            return Result<DownloadResult, Error<string>>.Ok(DownloadResult.Created);
        }
        catch (Exception e)
        {
            return Result<DownloadResult, Error<string>>.Err(new Error<string>(ErrorKind.DownloadFailure,
                $"cloning repository has thrown an exception '{e.GetType().Name}': {e.Message}: {e.StackTrace}"));
        }
        finally
        {
            _lock.ReleaseWriterLock();
        }
    }
}