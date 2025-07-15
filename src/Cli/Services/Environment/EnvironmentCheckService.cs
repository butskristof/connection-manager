using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ConnectionManager.Cli.Services.Environment;

internal sealed class EnvironmentCheckService : IEnvironmentCheckService
{
    #region construction

    private readonly ILogger<EnvironmentCheckService> _logger;

    public EnvironmentCheckService(ILogger<EnvironmentCheckService> logger)
    {
        _logger = logger;
    }

    #endregion

    private static readonly SystemDependency[] Dependencies =
    [
        new("ssh", SystemDependencyType.Required, "OpenSSH client - Required for SSH connections"),
        new(
            "sshpass",
            SystemDependencyType.Optional,
            "Password authentication helper - Needed for SSH connections using passwords"
        ),
    ];

    public async Task<ICollection<SystemDependencyCheckResult>> CheckSystemDependenciesAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Checking system dependencies");

        var results = new List<SystemDependencyCheckResult>();

        foreach (var dependency in Dependencies)
        {
            var isAvailable = await CheckDependencyAsync(dependency.Name, cancellationToken);
            results.Add(new SystemDependencyCheckResult(dependency, isAvailable));

            _logger.LogDebug(
                "Dependency check: {DependencyName} = {IsAvailable}",
                dependency.Name,
                isAvailable
            );
        }

        return results;
    }

    private async Task<bool> CheckDependencyAsync(
        string toolName,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = toolName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogWarning(
                    "Failed to start 'which' command for dependency check: {ToolName}",
                    toolName
                );
                return false;
            }

            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking dependency: {ToolName}", toolName);
            return false;
        }
    }
}
