using ConnectionManager.Core.Models;
using FluentValidation;

namespace ConnectionManager.Core.Services.Contracts.ConnectionProfiles;

public sealed record CreateConnectionProfileRequest(string Name, ConnectionType ConnectionType);

internal sealed class CreateConnectionProfileRequestValidator
    : AbstractValidator<CreateConnectionProfileRequest>
{
    public CreateConnectionProfileRequestValidator() { }
}
