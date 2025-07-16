using ConnectionManager.Cli.Services.Environment;
using ConnectionManager.Cli.Services.Ssh;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectionManager.Cli.Extensions;

internal static class DependencyInjection
{
    internal static IServiceCollection AddCli(this IServiceCollection services)
    {
        services.AddScoped<ConsoleUI>();
        services.AddScoped<IEnvironmentService, EnvironmentService>();
        services.AddScoped<ISshConnector, SshConnector>();

        return services;
    }
}
