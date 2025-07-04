namespace ConnectionManager.Cli.Services.Ssh;

internal sealed record SshConnectionRequest(
    string Host,
    int Port,
    string Username,
    string? KeyPath,
    string? Password
);
