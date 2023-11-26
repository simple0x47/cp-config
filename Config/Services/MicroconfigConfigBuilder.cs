using System.Diagnostics;
using Core;

namespace Cuplan.Config.Services;

public class MicroconfigConfigBuilder : IConfigBuilder
{
    private const string MicroconfigExecutable = "microconfig";

    private readonly TimeSpan _buildTimeout;

    private readonly string _currentEnvironment;

    public MicroconfigConfigBuilder(IWebHostEnvironment env, IConfiguration config)
    {
        _currentEnvironment = env.EnvironmentName.ToLowerInvariant();
        _buildTimeout = TimeSpan.FromSeconds(double.Parse(config["MicroconfigConfigBuilder:BuildTimeout"]));
    }

    public Result<Empty, Error<string>> Build(string buildPath, string targetPath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = MicroconfigExecutable,
            Arguments = $"-r {buildPath} -d {targetPath} -e {_currentEnvironment}"
        };

        using (Process process = new())
        {
            process.StartInfo = startInfo;
            process.Start();
            bool exited = process.WaitForExit(_buildTimeout);

            if (!exited)
                return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.TimedOut,
                    "building has timed out"));
        }

        return Result<Empty, Error<string>>.Ok(new Empty());
    }
}