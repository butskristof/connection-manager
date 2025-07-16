using CliWrap;
using Microsoft.Extensions.Logging;

namespace ConnectionManager.Cli.Services.Environment;

internal interface IEnvironmentService
{
    Task<IDictionary<string, bool>> CheckSystemDependencies(
        CancellationToken cancellationToken = default
    );
}

internal sealed class EnvironmentService : IEnvironmentService
{
    #region construction

    private readonly ILogger<EnvironmentService> _logger;

    public EnvironmentService(ILogger<EnvironmentService> logger)
    {
        _logger = logger;
    }

    #endregion

    public async Task<IDictionary<string, bool>> CheckSystemDependencies(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Checking environment for system dependencies");

        var dependencies = SystemDependencies.All;
        _logger.LogInformation(
            "Collected {Count} system dependencies to verify",
            dependencies.Count
        );

        var results = new Dictionary<string, bool>();
        foreach (var systemDependency in dependencies)
        {
            var available = await CheckSystemDependency(systemDependency, cancellationToken);
            _logger.LogInformation(
                "System dependency '{Name}' is {Status}",
                systemDependency.Name,
                available ? "available" : "not available"
            );
            results.Add(systemDependency.Name, available);
        }

        return results;
    }

    private static async Task<bool> CheckSystemDependency(
        SystemDependency systemDependency,
        CancellationToken cancellationToken = default
    )
    {
        // only Unix-like using `which` for now
        // TODO add support for Windows using `where`
        var result = await CliWrap
            .Cli.Wrap("which")
            .WithArguments([systemDependency.Name])
            // don't throw on error, just return failure
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(cancellationToken);
        return result.IsSuccess;
    }
}
