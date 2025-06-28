using ConnectionManager.Core.Common.Constants;
using ConnectionManager.Core.Models;
using FluentValidation;

namespace ConnectionManager.Core.Services.Contracts.ConnectionProfiles;

public sealed record CreateConnectionProfileRequest(string Name, ConnectionType ConnectionType);

internal sealed class CreateConnectionProfileRequestValidator
    : AbstractValidator<CreateConnectionProfileRequest>
{
    public CreateConnectionProfileRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Required)
            .WithMessage("Name is required");

        RuleFor(r => r.Name)
            .MaximumLength(ApplicationConstants.DefaultMaxStringLength)
            .WithErrorCode(ErrorCodes.MaxLength)
            .WithMessage(
                $"Name cannot exceed {ApplicationConstants.DefaultMaxStringLength} characters"
            );

        RuleFor(x => x.ConnectionType)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage("ConnectionType must be a valid enum value");
    }
}
