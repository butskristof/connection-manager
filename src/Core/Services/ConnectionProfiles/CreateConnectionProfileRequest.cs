using ConnectionManager.Core.Models;

namespace ConnectionManager.Core.Services.ConnectionProfiles;

public sealed record CreateConnectionProfileRequest(
    string Name,
    ConnectionType ConnectionType,
    string Host,
    ushort Port,
    string Username,
    string? KeyPath,
    string? Password
) : BaseConnectionProfileRequest(Name, ConnectionType, Host, Port, Username, KeyPath, Password);

internal sealed class CreateConnectionProfileRequestValidator
    : BaseConnectionProfileRequestValidator<CreateConnectionProfileRequest> { }
