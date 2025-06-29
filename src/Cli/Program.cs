using ConnectionManager.Cli.Extensions;
using ConnectionManager.Cli.Services;
using ConnectionManager.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new KeyNotFoundException("Connection string 'AppDbContext' not found in configuration.");

builder.Services.AddCore(connectionString).AddCli();

var host = builder.Build();
using var scope = host.Services.CreateScope();
var consoleUI = scope.ServiceProvider.GetRequiredService<ConsoleUI>();

await consoleUI.RunAsync();
