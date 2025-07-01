using ConnectionManager.Cli.Services.Ssh;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectionManager.Cli.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCli(this IServiceCollection services)
    {
        services.AddScoped<ConsoleUI>();
        services.AddScoped<ISshConnector, SshConnector>();

        return services;
    }
}
