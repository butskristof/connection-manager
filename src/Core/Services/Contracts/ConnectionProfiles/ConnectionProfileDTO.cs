using ConnectionManager.Core.Models;

namespace ConnectionManager.Core.Services.Contracts.ConnectionProfiles;

public sealed record ConnectionProfileDTO(
    Guid Id,
    string Name,
    ConnectionType ConnectionType,
    string Host,
    ushort Port,
    string Username
)
{
    public ConnectionProfileDTO(ConnectionProfile connectionProfile)
        : this(
            connectionProfile.Id,
            connectionProfile.Name,
            connectionProfile.ConnectionType,
            connectionProfile.Host,
            connectionProfile.Port,
            connectionProfile.Username
        ) { }
}
