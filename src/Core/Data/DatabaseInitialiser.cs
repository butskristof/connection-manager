using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace ConnectionManager.Core.Data;

public interface IDatabaseInitialiser
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

internal sealed class DatabaseInitialiser : IDatabaseInitialiser
{
    private readonly ILogger<DatabaseInitialiser> _logger;
    private readonly AppDbContext _dbContext;

    public DatabaseInitialiser(ILogger<DatabaseInitialiser> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing application database context...");

            await EnsureDatabaseAsync(cancellationToken);
            await RunMigrationsAsync(cancellationToken);

            _logger.LogInformation("Database context initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database initialization");
            throw;
        }
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        var dbCreator = _dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                _logger.LogInformation("Database does not exist. Creating database...");
                await dbCreator.CreateAsync(cancellationToken);
                _logger.LogInformation("Database created successfully");
            }
            else
            {
                _logger.LogInformation("Database already exists");
            }
        });
    }

    private async Task RunMigrationsAsync(CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var migrations = (
                await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)
            ).ToList();

            if (migrations.Count != 0)
            {
                _logger.LogInformation(
                    "Applying {Count} pending migrations: {Migrations}",
                    migrations.Count,
                    string.Join(", ", migrations)
                );

                await _dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Successfully applied database migrations");
            }
            else
            {
                _logger.LogInformation("No pending migrations found - database is up to date");
            }
        });
    }
}
