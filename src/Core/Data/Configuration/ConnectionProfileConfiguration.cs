using ConnectionManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConnectionManager.Core.Data.Configuration;

internal sealed class ConnectionProfileConfiguration : IEntityTypeConfiguration<ConnectionProfile>
{
    public void Configure(EntityTypeBuilder<ConnectionProfile> builder)
    {
        builder.ToTable("ConnectionProfiles");

        builder.Property(cp => cp.Name).IsRequired();
        builder.HasIndex(cp => cp.Name).IsUnique();

        builder.Property(cp => cp.ConnectionType).IsRequired();
    }
}
