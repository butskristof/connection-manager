using ConnectionManager.Core.Models;
using ConnectionManager.Core.Services.Contracts.ConnectionProfiles;
using ConnectionManager.Core.Services.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConnectionManager.Cli.Services;

internal sealed class ConsoleUI
{
    #region construction

    private readonly ILogger<ConsoleUI> _logger;
    private readonly IConnectionProfilesService _connectionProfilesService;

    public ConsoleUI(
        ILogger<ConsoleUI> logger,
        IConnectionProfilesService connectionProfilesService
    )
    {
        _logger = logger;
        _connectionProfilesService = connectionProfilesService;
    }

    #endregion

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting console UI");

        AnsiConsole.Write(new FigletText("Connection Manager").LeftJustified().Color(Color.Blue));

        while (!cancellationToken.IsCancellationRequested)
            await ShowMainMenu(cancellationToken);
    }

    #region main menu

    private enum MainMenuAction : byte
    {
        Exit = 0,
        Show = 1,
        Add = 2,
    }

    private async Task ShowMainMenu(CancellationToken cancellationToken)
    {
        var result = await _connectionProfilesService.GetAllAsync(cancellationToken);
        if (result.IsError)
        {
            AnsiConsole.MarkupLine("[red]Error loading profiles:[/]");
            foreach (var error in result.Errors)
                AnsiConsole.MarkupLine($"[red]  - {error.Code}: {error.Description}[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }
        var profiles = result.Value;

        // TODO empty state
        var prompt = new SelectionPrompt<(MainMenuAction action, ConnectionProfileDTO? profile)>()
            .Title("[bold]Select a connection profile[/]")
            .UseConverter(tuple =>
                tuple.action switch
                {
                    MainMenuAction.Show =>
                        $"[cyan]{tuple.profile!.Name}[/] [dim]({tuple.profile!.ConnectionType})[/]",
                    MainMenuAction.Add => "[green]Add New Profile[/]",
                    MainMenuAction.Exit => "[red]Exit[/]",
                    _ => throw new ArgumentException("Unknown main menu action"),
                }
            )
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more profiles)[/]");
        prompt.AddChoices(profiles.Select(p => (MainMenuAction.Show, (ConnectionProfileDTO?)p)));
        prompt.AddChoice((MainMenuAction.Add, null));
        prompt.AddChoice((MainMenuAction.Exit, null));

        var selection = AnsiConsole.Prompt(prompt);
        switch (selection.action)
        {
            case MainMenuAction.Show:
                if (selection.profile is null)
                {
                    AnsiConsole.MarkupLine(
                        "[red]Invalid selection: could not determine selected profile.[/]"
                    );
                    return;
                }
                ShowConnectionProfileMenu(selection.profile);
                break;
            case MainMenuAction.Add:
                await CreateConnectionProfileAsync(cancellationToken);
                break;
            case MainMenuAction.Exit:
                AnsiConsole.MarkupLine("[green]Goodbye![/]");
                Environment.Exit(0);
                break;
            default:
                AnsiConsole.MarkupLine("[red]Unknown action selected. Please try again.[/]");
                return;
        }
    }

    #endregion

    #region connection profile menu

    private enum ConnectionProfileMenuAction : byte
    {
        BackToMainMenu = 0,
        Connect = 1,
        Edit = 2,
        Delete = 4,
    }

    private static readonly Dictionary<ConnectionProfileMenuAction, string> ActionLabels = new()
    {
        { ConnectionProfileMenuAction.Connect, "[green]Connect[/]" },
        { ConnectionProfileMenuAction.Edit, "[blue]Edit profile[/]" },
        { ConnectionProfileMenuAction.Delete, "[red]Delete profile[/]" },
        { ConnectionProfileMenuAction.BackToMainMenu, "Back to main menu" },
    };

    private static void ShowConnectionProfileMenu(ConnectionProfileDTO connectionProfile)
    {
        var prompt = new SelectionPrompt<ConnectionProfileMenuAction>()
            .Title($"Connection Profile: [bold]{connectionProfile.Name}[/]")
            .UseConverter(a => ActionLabels[a]);
        prompt.AddChoice(ConnectionProfileMenuAction.Connect);
        prompt.AddChoice(ConnectionProfileMenuAction.Edit);
        prompt.AddChoice(ConnectionProfileMenuAction.Delete);
        prompt.AddChoice(ConnectionProfileMenuAction.BackToMainMenu);

        var choice = AnsiConsole.Prompt(prompt);

        switch (choice)
        {
            case ConnectionProfileMenuAction.BackToMainMenu:
                return;
            case ConnectionProfileMenuAction.Connect:
            case ConnectionProfileMenuAction.Edit:
            case ConnectionProfileMenuAction.Delete:
                ShowFeatureNotImplemented();
                break;
            default:
                AnsiConsole.MarkupLine("[red]Unknown action selected. Please try again.[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
                Console.ReadKey();
                break;
        }
    }

    private async Task CreateConnectionProfileAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold blue]Create New Connection Profile[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // Get connection profile name with validation
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter connection profile [blue]name[/]:")
                .PromptStyle("cyan")
                .ValidationErrorMessage("[red]Name cannot be empty[/]")
                .Validate(input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return ValidationResult.Error("[red]Name is required[/]");

                    return ValidationResult.Success();
                })
        );

        // Get connection type selection
        var connectionType = AnsiConsole.Prompt(
            new SelectionPrompt<ConnectionType>()
                .Title("Select [blue]connection type[/]:")
                .UseConverter(type => type.ToString())
                .AddChoices(
                    Enum.GetValues<ConnectionType>().Where(t => t != ConnectionType.Unknown)
                )
        );

        await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(
                "Creating connection profile...",
                async ctx =>
                {
                    // Create the request
                    var request = new CreateConnectionProfileRequest(name, connectionType);

                    // Call the service
                    var result = await _connectionProfilesService.CreateAsync(
                        request,
                        cancellationToken
                    );

                    AnsiConsole.WriteLine();

                    if (result.IsError)
                    {
                        AnsiConsole.MarkupLine("[red]Failed to create connection profile:[/]");
                        AnsiConsole.WriteLine();

                        foreach (var error in result.Errors)
                        {
                            switch (error.Type)
                            {
                                case ErrorType.Validation:
                                    AnsiConsole.MarkupLine(
                                        $"[yellow]Validation Error:[/] {error.Description}"
                                    );
                                    if (!string.IsNullOrEmpty(error.Code))
                                        AnsiConsole.MarkupLine(
                                            $"[dim]  Error Code: {error.Code}[/]"
                                        );
                                    break;
                                case ErrorType.Conflict:
                                    AnsiConsole.MarkupLine(
                                        $"[orange1]Conflict:[/] {error.Description}"
                                    );
                                    break;
                                default:
                                    AnsiConsole.MarkupLine(
                                        $"[red]Error ({error.Code}):[/] {error.Description}"
                                    );
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var createdProfile = result.Value;
                        AnsiConsole.MarkupLine(
                            "[green]✓ Connection profile created successfully![/]"
                        );
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine($"[bold]Name:[/] {createdProfile.Name}");
                        AnsiConsole.MarkupLine($"[bold]Type:[/] {createdProfile.ConnectionType}");
                        AnsiConsole.MarkupLine($"[bold]ID:[/] [dim]{createdProfile.Id}[/]");
                    }
                }
            );

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    #endregion

    private static void ShowFeatureNotImplemented()
    {
        AnsiConsole.MarkupLine("[yellow]Feature not implemented yet[/]");
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }
}
