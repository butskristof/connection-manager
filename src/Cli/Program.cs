using ConnectionManager.Core;
using ConnectionManager.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Hardcoded connection string for local SQLite database
const string connectionString = "Data Source=app.db";
builder.Services.AddCore(connectionString);

var host = builder.Build();

// Test database connection
using var scope = host.Services.CreateScope();
var connectionProfilesService =
    scope.ServiceProvider.GetRequiredService<IConnectionProfilesService>();

var profiles = await connectionProfilesService.GetAllAsync();
Console.WriteLine($"Found {profiles.Count} connection profiles:");

foreach (var profile in profiles)
{
    Console.WriteLine($"- {profile.Name} ({profile.ConnectionType})");
}
