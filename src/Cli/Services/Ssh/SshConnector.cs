using CliWrap;

namespace ConnectionManager.Cli.Services.Ssh;

internal interface ISshConnector
{
    Task ConnectAsync(SshConnectionRequest request, CancellationToken cancellationToken = default);
}

internal sealed class SshConnector : ISshConnector
{
    public async Task ConnectAsync(
        SshConnectionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await using var stdIn = Console.OpenStandardInput();
        await using var stdOut = Console.OpenStandardOutput();
        await using var stdErr = Console.OpenStandardError();

        var cmd = stdIn | GetCommand(request) | (stdOut, stdErr);

        await cmd.ExecuteAsync(cancellationToken);
    }

    private static Command GetCommand(SshConnectionRequest request)
    {
        var useSshPass = !string.IsNullOrWhiteSpace(request.Password);

        return CliWrap
            .Cli.Wrap(useSshPass ? "sshpass" : "ssh")
            .WithArguments(args =>
            {
                if (useSshPass)
                    args.Add("-e").Add("ssh");

                args.Add($"{request.Username}@{request.Host}");
                args.Add("-p").Add(request.Port);

                // Force pseudo-tty allocation
                // https://stackoverflow.com/a/7122115
                args.Add("-tt");

                if (!useSshPass && !string.IsNullOrWhiteSpace(request.KeyPath))
                    args.Add("-i").Add(request.KeyPath);
            })
            .WithEnvironmentVariables(env =>
            {
                if (useSshPass)
                    env.Set("SSHPASS", request.Password);
            });
    }
}
