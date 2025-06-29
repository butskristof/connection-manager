using ConnectionManager.Core.Services.Contracts.ConnectionProfiles;
using ConnectionManager.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConnectionManager.Cli.Services;

internal sealed class Cli
{
    #region Construction

    private readonly ILogger<Cli> _logger;
    private readonly IConnectionProfilesService _connectionProfilesService;

    public Cli(ILogger<Cli> logger, IConnectionProfilesService connectionProfilesService)
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
            await ShowMainMenuAsync(cancellationToken);
    }

    #endregion

    #region Private Methods

    private async Task ShowMainMenuAsync(CancellationToken cancellationToken)
    {
        var result = await _connectionProfilesService.GetAllAsync(cancellationToken);

        if (result.IsError)
        {
            AnsiConsole.MarkupLine("[red]Error loading profiles:[/]");
            foreach (var error in result.Errors)
                AnsiConsole.MarkupLine($"[red]  - {error.Description}[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        var profiles = result.Value;

        var prompt = new SelectionPrompt<string>()
            .Title(
                profiles.Count > 0
                    ? "[bold]Select a connection profile or choose an action:[/]"
                    : "[bold]No profiles found. Choose an action:[/]"
            )
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]");

        foreach (var profile in profiles)
            prompt.AddChoice($"[cyan]{profile.Name}[/] [dim]({profile.ConnectionType})[/]");

        prompt.AddChoice("[green]Add New Profile[/]");
        prompt.AddChoice("[red]Exit[/]");

        var selection = AnsiConsole.Prompt(prompt);

        if (selection == "[green]Add New Profile[/]")
        {
            AnsiConsole.MarkupLine("[yellow]Feature not implemented yet[/]");
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey();
        }
        else if (selection == "[red]Exit[/]")
        {
            _logger.LogDebug("User requested exit");
            AnsiConsole.MarkupLine("[green]Goodbye![/]");
            Environment.Exit(0);
        }
        else if (!string.IsNullOrWhiteSpace(selection))
        {
            var selectedProfile = profiles.FirstOrDefault(p =>
                $"[cyan]{p.Name}[/] [dim]({p.ConnectionType})[/]" == selection
            );

            if (selectedProfile != null)
                await ShowProfileOperationsAsync(selectedProfile, cancellationToken);
        }
    }

    private static Task ShowProfileOperationsAsync(
        ConnectionProfileDTO profile,
        CancellationToken cancellationToken
    )
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(
                    $"[bold]Operations for '[cyan]{profile.Name}[/]' ({profile.ConnectionType}):[/]"
                )
                .AddChoices(
                    "[green]Open Connection[/]",
                    "[blue]Edit Profile[/]",
                    "[red]Delete Profile[/]",
                    "[dim]Back to Main Menu[/]"
                )
        );

        switch (choice)
        {
            case "[green]Open Connection[/]":
                AnsiConsole.MarkupLine("[yellow]Feature not implemented yet[/]");
                break;
            case "[blue]Edit Profile[/]":
                AnsiConsole.MarkupLine("[yellow]Feature not implemented yet[/]");
                break;
            case "[red]Delete Profile[/]":
                AnsiConsole.MarkupLine("[yellow]Feature not implemented yet[/]");
                break;
            case "[dim]Back to Main Menu[/]":
                return Task.CompletedTask;
        }

        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey();
        return Task.CompletedTask;
    }

    #endregion
}
