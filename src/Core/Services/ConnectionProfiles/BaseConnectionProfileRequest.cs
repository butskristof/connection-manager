using ConnectionManager.Core.Common.Constants;
using ConnectionManager.Core.Common.Validation;
using ConnectionManager.Core.Models;
using FluentValidation;

namespace ConnectionManager.Core.Services.ConnectionProfiles;

public abstract record BaseConnectionProfileRequest(
    string Name,
    ConnectionType ConnectionType,
    string Host,
    ushort Port,
    string Username,
    string? KeyPath,
    string? Password
);

internal abstract class BaseConnectionProfileRequestValidator<T> : AbstractValidator<T>
    where T : BaseConnectionProfileRequest
{
    protected BaseConnectionProfileRequestValidator()
    {
        RuleFor(r => r.Name).ValidString(true);

        RuleFor(x => x.ConnectionType)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage("{PropertyName} must be a valid enum value")
            .NotEqual(ConnectionType.Unknown)
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage("{PropertyName} cannot be Unknown");

        RuleFor(r => r.Host).ValidString(true);

        RuleFor(r => r.Port)
            .InclusiveBetween(ApplicationConstants.MinPort, ApplicationConstants.MaxPort)
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage("{PropertyName} must be between {From} and {To}");

        RuleFor(r => r.Username).ValidString(true);

        RuleFor(r => r.KeyPath).ValidString(false);

        RuleFor(r => r.Password).ValidString(false);
    }
}
