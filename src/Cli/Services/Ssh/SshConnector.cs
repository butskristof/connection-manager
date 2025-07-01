using System.Diagnostics;

namespace ConnectionManager.Cli.Services.Ssh;

internal interface ISshConnector
{
    void Connect(SshConnectionRequest request);
}

internal sealed class SshConnector : ISshConnector
{
    public void Connect(SshConnectionRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ssh",
            Arguments = BuildArguments(request),
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        try
        {
            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SSH session failed: {ex.Message}");
        }
    }

    private static string BuildArguments(SshConnectionRequest request)
    {
        var args = new List<string>();

        args.Add($"{request.Username}@{request.Host}");
        args.Add($"-p {request.Port}");

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            // TODO add sshpass
        }
        else if (!string.IsNullOrWhiteSpace(request.KeyPath))
        {
            args.Add($"-i \"{request.KeyPath}\"");
        }

        return string.Join(" ", args);
    }
}
