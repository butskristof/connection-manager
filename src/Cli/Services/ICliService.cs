namespace ConnectionManager.Cli.Services;

internal interface ICliService
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
