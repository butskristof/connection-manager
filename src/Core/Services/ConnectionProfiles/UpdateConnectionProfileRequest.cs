using ConnectionManager.Core.Common.Validation;
using ConnectionManager.Core.Models;

namespace ConnectionManager.Core.Services.ConnectionProfiles;

public sealed record UpdateConnectionProfileRequest(
    Guid Id,
    string Name,
    ConnectionType ConnectionType,
    string Host,
    ushort Port,
    string Username,
    string? KeyPath,
    string? Password
) : BaseConnectionProfileRequest(Name, ConnectionType, Host, Port, Username, KeyPath, Password);

internal sealed class UpdateConnectionProfileRequestValidator
    : BaseConnectionProfileRequestValidator<UpdateConnectionProfileRequest>
{
    public UpdateConnectionProfileRequestValidator()
    {
        RuleFor(r => r.Id).NotEmptyWithErrorCode();
    }
}
