using ConnectionManager.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectionManager.Cli.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCli(this IServiceCollection services)
    {
        services.AddScoped<ICliService, CliService>();

        return services;
    }
}
