using ConnectionManager.Core.Constants;
using ConnectionManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectionManager.Core.Data;

// TODO make internal after testing
public sealed class AppDbContext : DbContext
{
    #region construction

    public AppDbContext(DbContextOptions options)
        : base(options) { }

    #endregion

    #region entities

    public DbSet<ConnectionProfile> ConnectionProfiles { get; set; }

    #endregion

    #region configuration

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // the base method is empty, but retain the call to minimise impact if
        // it should be used in a future version
        base.ConfigureConventions(configurationBuilder);

        // set text fields to have a reduced maximum length by default
        // this cuts down on a lot of varchar(max) columns, and can still be set to a higher
        // maximum length on a per-column basis
        configurationBuilder
            .Properties<string>()
            .HaveMaxLength(ApplicationConstants.DefaultMaxStringLength);

        configurationBuilder.Properties<decimal>().HavePrecision(18, 6);

        // TODO DateTimeOffset?
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // the base method is empty, but retain the call to minimise impact if
        // it should be used in a future version
        base.OnModelCreating(modelBuilder);

        // TODO case insensitive collation?

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    #endregion
}
