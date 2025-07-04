using ConnectionManager.Cli.Services.Ssh;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectionManager.Cli.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddCli(this IServiceCollection services)
    {
        services.AddScoped<ConsoleUI>();
        services.AddScoped<ISshConnector, SshConnector>();

        return services;
    }
}
