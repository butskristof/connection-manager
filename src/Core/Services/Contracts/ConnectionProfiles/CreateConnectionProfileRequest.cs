using ConnectionManager.Core.Common.Constants;
using ConnectionManager.Core.Models;
using FluentValidation;

namespace ConnectionManager.Core.Services.Contracts.ConnectionProfiles;

public sealed record CreateConnectionProfileRequest(
    string Name,
    ConnectionType ConnectionType,
    string Host,
    ushort Port,
    string Username
);

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

        RuleFor(r => r.Host)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Required)
            .WithMessage("Host is required");

        RuleFor(r => r.Host)
            .MaximumLength(ApplicationConstants.DefaultMaxStringLength)
            .WithErrorCode(ErrorCodes.MaxLength)
            .WithMessage(
                $"Host cannot exceed {ApplicationConstants.DefaultMaxStringLength} characters"
            );

        RuleFor(r => r.Port)
            .InclusiveBetween(ApplicationConstants.MinPort, ApplicationConstants.MaxPort)
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage(
                $"Port must be between {ApplicationConstants.MinPort} and {ApplicationConstants.MaxPort}"
            );

        RuleFor(r => r.Username)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Required)
            .WithMessage("Username is required");

        RuleFor(r => r.Username)
            .MaximumLength(ApplicationConstants.DefaultMaxStringLength)
            .WithErrorCode(ErrorCodes.MaxLength)
            .WithMessage(
                $"Username cannot exceed {ApplicationConstants.DefaultMaxStringLength} characters"
            );
    }
}
