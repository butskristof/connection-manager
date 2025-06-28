using ConnectionManager.Core.Data;
using ConnectionManager.Core.Services.Implementations;
using ConnectionManager.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectionManager.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddPersistence(connectionString).AddServices();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<AppDbContext>(builder => builder.UseSqlite(connectionString));

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<IValidatorFactory, ValidatorFactory>()
            .AddScoped<IConnectionProfilesService, ConnectionProfilesService>();

        return services;
    }
}
