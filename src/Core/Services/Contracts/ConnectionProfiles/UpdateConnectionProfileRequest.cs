using ConnectionManager.Core.Common.Constants;
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
    public UpdateConnectionProfileRequestValidator()
    {
        RuleFor(r => r.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Required)
            .WithMessage("Id is required");

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

        RuleFor(r => r.ConnectionType)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage("ConnectionType must be a valid enum value");
    }
}
