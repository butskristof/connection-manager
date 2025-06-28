using ConnectionManager.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConnectionManager.Cli.Services;

internal sealed class CliService : ICliService
{
    #region Construction

    private readonly ILogger<CliService> _logger;
    private readonly IConnectionProfilesService _connectionProfilesService;

    public CliService(
        ILogger<CliService> logger,
        IConnectionProfilesService connectionProfilesService
    )
    {
        _logger = logger;
        _connectionProfilesService = connectionProfilesService;
    }

    #endregion

    #region Implementation

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting CLI application");

        AnsiConsole.Write(new FigletText("Connection Manager").LeftJustified().Color(Color.Blue));

        while (!cancellationToken.IsCancellationRequested)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .AddChoices("List Connection Profiles", "Exit")
            );

            switch (choice)
            {
                case "List Connection Profiles":
                    await DisplayConnectionProfilesAsync(cancellationToken);
                    break;
                case "Exit":
                    _logger.LogDebug("User requested exit");
                    AnsiConsole.MarkupLine("[green]Goodbye![/]");
                    return;
            }
        }
    }

    #endregion

    #region Private Methods

    private async Task DisplayConnectionProfilesAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Displaying connection profiles");

        var result = await _connectionProfilesService.GetAllAsync(cancellationToken);

        if (result.IsError)
        {
            foreach (var error in result.Errors)
            {
                AnsiConsole.MarkupLine($"[red]Error: {error.Description}[/]");
            }
            return;
        }

        var profiles = result.Value;

        if (profiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No connection profiles found.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table();
        table.AddColumn("[blue]Name[/]");
        table.AddColumn("[blue]Connection Type[/]");
        table.AddColumn("[blue]ID[/]");

        foreach (var profile in profiles)
        {
            table.AddRow(profile.Name, profile.ConnectionType.ToString(), profile.Id.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    #endregion
}
