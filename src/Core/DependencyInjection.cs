using ConnectionManager.Core.Common.Validation;
using ConnectionManager.Core.Data;
using ConnectionManager.Core.Services.ConnectionProfiles;
using FluentValidation;
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

        services.AddValidatorsFromAssemblyContaining(
            typeof(DependencyInjection),
            includeInternalTypes: true
        );

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
        // internal services
        services.AddScoped<IValidationService, ValidationService>();

        // public services
        services.AddScoped<IConnectionProfilesService, ConnectionProfilesService>();

        return services;
    }
}
