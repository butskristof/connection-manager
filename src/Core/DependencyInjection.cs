using ConnectionManager.Core.Data;
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
        services.AddPersistence(connectionString);

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
}
