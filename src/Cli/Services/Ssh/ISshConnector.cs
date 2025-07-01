namespace ConnectionManager.Cli.Services.Ssh;

public interface ISshConnector
{
    void Connect(SshConnectionRequest request);
}