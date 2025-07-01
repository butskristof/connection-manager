using ConnectionManager.Cli.Services.Ssh;
using ConnectionManager.Core.Models;
using ConnectionManager.Core.Services.Contracts.ConnectionProfiles;
using ConnectionManager.Core.Services.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConnectionManager.Cli;

internal sealed class ConsoleUI
{
    #region construction

    private readonly ILogger<ConsoleUI> _logger;
    private readonly IConnectionProfilesService _connectionProfilesService;
    private readonly ISshConnector _sshConnector;

    public ConsoleUI(
        ILogger<ConsoleUI> logger,
        IConnectionProfilesService connectionProfilesService,
        ISshConnector sshConnector
    )
    {
        _logger = logger;
        _connectionProfilesService = connectionProfilesService;
        _sshConnector = sshConnector;
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
                await ShowConnectionProfileMenu(selection.profile, cancellationToken);
                break;
            case MainMenuAction.Add:
                await CreateConnectionProfileAsync(cancellationToken);
                break;
            case MainMenuAction.Exit:
                ExitApplication();
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
        Delete = 3,
    }

    private static readonly Dictionary<ConnectionProfileMenuAction, string> ActionLabels = new()
    {
        { ConnectionProfileMenuAction.Connect, "[green]Connect[/]" },
        { ConnectionProfileMenuAction.Edit, "[blue]Edit profile[/]" },
        { ConnectionProfileMenuAction.Delete, "[red]Delete profile[/]" },
        { ConnectionProfileMenuAction.BackToMainMenu, "Back to main menu" },
    };

    private async Task ShowConnectionProfileMenu(
        ConnectionProfileDTO connectionProfile,
        CancellationToken cancellationToken
    )
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
                ConnectToProfile(connectionProfile);
                break;
            case ConnectionProfileMenuAction.Edit:
                await UpdateConnectionProfileAsync(connectionProfile, cancellationToken);
                break;
            case ConnectionProfileMenuAction.Delete:
                await DeleteConnectionProfileAsync(connectionProfile, cancellationToken);
                break;
            default:
                AnsiConsole.MarkupLine("[red]Unknown action selected. Please try again.[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
                Console.ReadKey();
                break;
        }
    }

    #region shared input methods

    private static string PromptForConnectionProfileName(string? defaultValue = null)
    {
        var prompt = new TextPrompt<string>("Enter connection profile [blue]name[/]:")
            .PromptStyle("cyan")
            .ValidationErrorMessage("[red]Name cannot be empty[/]")
            .Validate(input =>
            {
                if (string.IsNullOrWhiteSpace(input))
                    return ValidationResult.Error("[red]Name is required[/]");

                return ValidationResult.Success();
            });

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            prompt = prompt.DefaultValue(defaultValue).ShowDefaultValue(true);
        }

        return AnsiConsole.Prompt(prompt);
    }

    private static ConnectionType PromptForConnectionType(ConnectionType? defaultValue = null)
    {
        var prompt = new SelectionPrompt<ConnectionType>()
            .Title("Select [blue]connection type[/]:")
            .UseConverter(type => type.ToString())
            .AddChoices(Enum.GetValues<ConnectionType>().Where(t => t != ConnectionType.Unknown));

        if (defaultValue.HasValue)
        {
            prompt = prompt.WrapAround(false);
            // Set the default by finding and highlighting it
            var choices = Enum.GetValues<ConnectionType>()
                .Where(t => t != ConnectionType.Unknown)
                .ToList();
            if (choices.Contains(defaultValue.Value))
            {
                // Add default value first, then others
                prompt = new SelectionPrompt<ConnectionType>()
                    .Title("Select [blue]connection type[/]:")
                    .UseConverter(type =>
                        type == defaultValue.Value
                            ? $"[bold]{type}[/] [dim](current)[/]"
                            : type.ToString()
                    )
                    .AddChoices(choices);
            }
        }

        return AnsiConsole.Prompt(prompt);
    }

    private static string PromptForHost(string? defaultValue = null)
    {
        var prompt = new TextPrompt<string>("Enter [blue]host[/] (IP address or hostname):")
            .PromptStyle("cyan")
            .ValidationErrorMessage("[red]Host cannot be empty[/]")
            .Validate(input =>
            {
                if (string.IsNullOrWhiteSpace(input))
                    return ValidationResult.Error("[red]Host is required[/]");

                return ValidationResult.Success();
            });

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            prompt = prompt.DefaultValue(defaultValue).ShowDefaultValue(true);
        }

        return AnsiConsole.Prompt(prompt);
    }

    private static ushort PromptForPort(ushort? defaultValue = null)
    {
        var prompt = new TextPrompt<ushort>("Enter [blue]port[/]:")
            .PromptStyle("cyan")
            .ValidationErrorMessage("[red]Please enter a valid port number (1-65535)[/]")
            .Validate(port =>
            {
                if (port is < 1 or > 65535)
                    return ValidationResult.Error("[red]Port must be between 1 and 65535[/]");

                return ValidationResult.Success();
            });

        if (defaultValue.HasValue)
        {
            prompt = prompt.DefaultValue(defaultValue.Value).ShowDefaultValue(true);
        }

        return AnsiConsole.Prompt(prompt);
    }

    private static string PromptForUsername(string? defaultValue = null)
    {
        var prompt = new TextPrompt<string>("Enter [blue]username[/]:")
            .PromptStyle("cyan")
            .ValidationErrorMessage("[red]Username cannot be empty[/]")
            .Validate(input =>
            {
                if (string.IsNullOrWhiteSpace(input))
                    return ValidationResult.Error("[red]Username is required[/]");

                return ValidationResult.Success();
            });

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            prompt = prompt.DefaultValue(defaultValue).ShowDefaultValue(true);
        }

        return AnsiConsole.Prompt(prompt);
    }

    private static string? PromptForKeyPath(string? defaultValue = null)
    {
        var prompt = new TextPrompt<string>("Enter [blue]private key path[/] (optional):")
            .PromptStyle("cyan")
            .AllowEmpty();

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            prompt = prompt.DefaultValue(defaultValue).ShowDefaultValue(true);
        }

        var result = AnsiConsole.Prompt(prompt);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static string? PromptForPassword(string? defaultValue = null)
    {
        var prompt = new TextPrompt<string>("Enter [blue]password[/] (optional):")
            .PromptStyle("cyan")
            .Secret()
            .AllowEmpty();

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            prompt = prompt.DefaultValue(defaultValue).ShowDefaultValue(true);
        }

        var result = AnsiConsole.Prompt(prompt);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static void DisplayErrorOrResult<T>(
        ErrorOr<T> result,
        string successMessage,
        Func<T, string>? successDetails = null,
        string? objectName = null
    )
    {
        if (result.IsError)
        {
            AnsiConsole.MarkupLine($"[red]Failed to {objectName ?? "process"}:[/]");
            AnsiConsole.WriteLine();

            foreach (var error in result.Errors)
            {
                switch (error.Type)
                {
                    case ErrorType.Validation:
                        AnsiConsole.MarkupLine($"[yellow]Validation Error:[/] {error.Description}");
                        if (!string.IsNullOrWhiteSpace(error.Code))
                            AnsiConsole.MarkupLine($"[dim]  Error Code: {error.Code}[/]");
                        break;
                    case ErrorType.Conflict:
                        AnsiConsole.MarkupLine($"[orange1]Conflict:[/] {error.Description}");
                        break;
                    case ErrorType.NotFound:
                        AnsiConsole.MarkupLine($"[yellow]Not Found:[/] {error.Description}");
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
            AnsiConsole.MarkupLine($"[green]✓ {successMessage}[/]");
            if (successDetails != null)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(successDetails(result.Value));
            }
        }
    }

    #endregion

    private async Task CreateConnectionProfileAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold blue]Create New Connection Profile[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // Get connection profile name with validation
        var name = PromptForConnectionProfileName();

        // Get connection type selection
        var connectionType = PromptForConnectionType();

        // Get host information
        var host = PromptForHost();

        // Get port information
        var port = PromptForPort();

        // Get username information
        var username = PromptForUsername();

        // Get optional key path
        var keyPath = PromptForKeyPath();

        // Get optional password
        var password = PromptForPassword();

        await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(
                "Creating connection profile...",
                async _ =>
                {
                    // Create the request
                    var request = new CreateConnectionProfileRequest(
                        name,
                        connectionType,
                        host,
                        port,
                        username,
                        keyPath,
                        password
                    );

                    // Call the service
                    var result = await _connectionProfilesService.CreateAsync(
                        request,
                        cancellationToken
                    );

                    AnsiConsole.WriteLine();

                    DisplayErrorOrResult(
                        result,
                        "Connection profile created successfully!",
                        profile =>
                            $"[bold]Name:[/] {profile.Name}\n"
                            + $"[bold]Type:[/] {profile.ConnectionType}\n"
                            + $"[bold]Host:[/] {profile.Host}\n"
                            + $"[bold]Port:[/] {profile.Port}\n"
                            + $"[bold]Username:[/] {profile.Username}\n"
                            + (
                                profile.KeyPath != null
                                    ? $"[bold]Key Path:[/] {profile.KeyPath}\n"
                                    : ""
                            )
                            + (
                                profile.Password != null
                                    ? $"[bold]Password:[/] {profile.Password}\n"
                                    : ""
                            )
                            + $"[bold]ID:[/] [dim]{profile.Id}[/]",
                        "create connection profile"
                    );
                }
            );

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    private async Task UpdateConnectionProfileAsync(
        ConnectionProfileDTO connectionProfile,
        CancellationToken cancellationToken
    )
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold blue]Edit Connection Profile[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // Show current profile details
        AnsiConsole.MarkupLine($"[bold]Editing profile:[/] [cyan]{connectionProfile.Name}[/]");
        AnsiConsole.MarkupLine(
            $"[dim]Current values are shown as defaults. Press Enter to keep current value.[/]"
        );
        AnsiConsole.WriteLine();

        // Get connection profile name with current value as default
        var name = PromptForConnectionProfileName(connectionProfile.Name);

        // Get connection type selection with current value as default
        var connectionType = PromptForConnectionType(connectionProfile.ConnectionType);

        // Get host information with current value as default
        var host = PromptForHost(connectionProfile.Host);

        // Get port information with current value as default
        var port = PromptForPort(connectionProfile.Port);

        // Get username information with current value as default
        var username = PromptForUsername(connectionProfile.Username);

        // Get optional key path with current value as default
        var keyPath = PromptForKeyPath(connectionProfile.KeyPath);

        // Get optional password with current value as default
        var password = PromptForPassword(connectionProfile.Password);

        await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(
                "Updating connection profile...",
                async _ =>
                {
                    // Create the request
                    var request = new UpdateConnectionProfileRequest(
                        connectionProfile.Id,
                        name,
                        connectionType,
                        host,
                        port,
                        username,
                        keyPath,
                        password
                    );

                    // Call the service
                    var result = await _connectionProfilesService.UpdateAsync(
                        request,
                        cancellationToken
                    );

                    AnsiConsole.WriteLine();

                    DisplayErrorOrResult(
                        result,
                        "Connection profile updated successfully!",
                        profile =>
                            $"[bold]Name:[/] {profile.Name}\n"
                            + $"[bold]Type:[/] {profile.ConnectionType}\n"
                            + $"[bold]Host:[/] {profile.Host}\n"
                            + $"[bold]Port:[/] {profile.Port}\n"
                            + $"[bold]Username:[/] {profile.Username}\n"
                            + (
                                profile.KeyPath != null
                                    ? $"[bold]Key Path:[/] {profile.KeyPath}\n"
                                    : ""
                            )
                            + (
                                profile.Password != null
                                    ? $"[bold]Password:[/] {profile.Password}\n"
                                    : ""
                            )
                            + $"[bold]ID:[/] [dim]{profile.Id}[/]",
                        "update connection profile"
                    );
                }
            );

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    private async Task DeleteConnectionProfileAsync(
        ConnectionProfileDTO connectionProfile,
        CancellationToken cancellationToken
    )
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold red]Delete Connection Profile[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // Show profile details
        AnsiConsole.MarkupLine("[bold]Profile to delete:[/]");
        AnsiConsole.MarkupLine($"  [cyan]Name:[/] {connectionProfile.Name}");
        AnsiConsole.MarkupLine($"  [cyan]Type:[/] {connectionProfile.ConnectionType}");
        AnsiConsole.MarkupLine($"  [dim]ID: {connectionProfile.Id}[/]");
        AnsiConsole.WriteLine();

        // Confirmation prompt
        var confirmed = AnsiConsole.Confirm(
            "[red]Are you sure you want to delete this connection profile?[/]",
            false
        );

        if (!confirmed)
        {
            AnsiConsole.MarkupLine("[yellow]Deletion cancelled.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(
                "Deleting connection profile...",
                async _ =>
                {
                    // Call the service
                    var result = await _connectionProfilesService.DeleteAsync(
                        connectionProfile.Id,
                        cancellationToken
                    );

                    AnsiConsole.WriteLine();

                    DisplayErrorOrResult(
                        result,
                        $"Connection profile '{connectionProfile.Name}' deleted successfully!",
                        objectName: "delete connection profile"
                    );
                }
            );

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    #endregion

    private void ConnectToProfile(ConnectionProfileDTO connectionProfile)
    {
        switch (connectionProfile.ConnectionType)
        {
            case ConnectionType.SSH:
            {
                var request = new SshConnectionRequest(
                    Host: connectionProfile.Host,
                    Port: connectionProfile.Port,
                    Username: connectionProfile.Username,
                    KeyPath: connectionProfile.KeyPath,
                    Password: connectionProfile.Password
                );

                AnsiConsole.MarkupLine($"[green]Connecting to {connectionProfile.Name}...[/]");
                _sshConnector.Connect(request);
                ExitApplication();
                break;
            }
            case ConnectionType.Unknown:
            default:
                ShowFeatureNotImplemented();
                break;
        }
    }

    private static void ExitApplication()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]Goodbye![/]");
        Environment.Exit(0);
    }

    private static void ShowFeatureNotImplemented()
    {
        AnsiConsole.MarkupLine("[yellow]Feature not implemented yet[/]");
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }
}
