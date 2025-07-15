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
            FileName = GetCommand(request),
            Arguments = BuildArguments(request),
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            startInfo.Environment["SSHPASS"] = request.Password;
        }

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

    private static string GetCommand(SshConnectionRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Password) ? "sshpass" : "ssh";
    }

    private static string BuildArguments(SshConnectionRequest request)
    {
        var args = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            args.Add("-e");
            args.Add("ssh");
        }

        args.Add($"{request.Username}@{request.Host}");
        args.Add($"-p {request.Port}");

        if (
            string.IsNullOrWhiteSpace(request.Password)
            && !string.IsNullOrWhiteSpace(request.KeyPath)
        )
        {
            args.Add($"-i \"{request.KeyPath}\"");
        }

        return string.Join(" ", args);
    }
}
