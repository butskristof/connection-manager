namespace ConnectionManager.Cli.Services.Environment;

internal interface IEnvironmentCheckService
{
    Task<ICollection<SystemDependencyCheckResult>> CheckSystemDependenciesAsync(
        CancellationToken cancellationToken = default
    );
}
