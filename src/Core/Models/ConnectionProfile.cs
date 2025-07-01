namespace ConnectionManager.Core.Models;

public sealed class ConnectionProfile
{
    public Guid Id { get; init; }

    public required string Name { get; set; }
    public required ConnectionType ConnectionType { get; set; }

    public required string Host { get; set; }
    public required ushort Port { get; set; }
    public required string Username { get; set; }
}
