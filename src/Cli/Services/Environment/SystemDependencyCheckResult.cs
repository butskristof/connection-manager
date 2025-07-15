namespace ConnectionManager.Cli.Services.Environment;

internal sealed record SystemDependencyCheckResult(SystemDependency Dependency, bool IsAvailable);
