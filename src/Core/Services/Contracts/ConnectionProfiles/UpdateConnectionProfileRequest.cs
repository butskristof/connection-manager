using ConnectionManager.Core.Models;
using FluentValidation;

namespace ConnectionManager.Core.Services.Contracts.ConnectionProfiles;

public sealed record UpdateConnectionProfileRequest(
    Guid Id,
    string Name,
    ConnectionType ConnectionType
);

internal sealed class UpdateConnectionProfileRequestValidator
    : AbstractValidator<UpdateConnectionProfileRequest>
{
    public UpdateConnectionProfileRequestValidator() { }
}
