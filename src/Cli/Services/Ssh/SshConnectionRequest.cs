namespace ConnectionManager.Cli.Services.Ssh;

public sealed record SshConnectionRequest(
    string Host,
    int Port,
    string Username,
    string? KeyPath,
    string? Password
);
