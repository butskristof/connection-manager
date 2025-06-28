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

var profilesResult = await connectionProfilesService.GetAllAsync();

if (profilesResult.IsError)
{
    Console.WriteLine("Failed to retrieve connection profiles:");
    foreach (var error in profilesResult.Errors)
    {
        Console.WriteLine($"- {error.Code}: {error.Description}");
    }
}
else
{
    var profiles = profilesResult.Value;
    Console.WriteLine($"Found {profiles.Count} connection profiles:");

    foreach (var profile in profiles)
    {
        Console.WriteLine($"- {profile.Name} ({profile.ConnectionType})");
    }
}
