using System.Diagnostics;

namespace ConnectionManager.Cli.Services.Ssh;

internal sealed class SshConnector : ISshConnector
{
    public void Connect(SshConnectionRequest request)
    {
        var arguments = BuildSshCommand(request);

        var startInfo = new ProcessStartInfo
        {
            FileName = "ssh",
            Arguments = arguments,
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

    private static string BuildSshCommand(SshConnectionRequest request)
    {
        var args = new List<string>();

        // Add port if not default
        if (request.Port != 22)
        {
            args.Add("-p");
            args.Add(request.Port.ToString());
        }

        // Add SSH key if specified
        if (!string.IsNullOrWhiteSpace(request.KeyPath))
        {
            args.Add("-i");
            args.Add($"\"{request.KeyPath}\"");
        }

        // Add user@host
        args.Add($"{request.Username}@{request.Host}");

        return string.Join(" ", args);
    }
}
