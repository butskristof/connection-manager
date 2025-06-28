using ConnectionManager.Core;
using ConnectionManager.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Hardcoded connection string for local SQLite database
const string connectionString = "Data Source=app.db";
builder.Services.AddCore(connectionString);

var host = builder.Build();

// Test database connection
using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

var profiles = await dbContext.ConnectionProfiles.ToListAsync();
Console.WriteLine($"Found {profiles.Count} connection profiles:");

foreach (var profile in profiles)
{
    Console.WriteLine($"- {profile.Name} ({profile.ConnectionType})");
}
