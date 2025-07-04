using ConnectionManager.Core.Common.Constants;
using FluentValidation;

namespace ConnectionManager.Core.Common.Validation;

internal static class FluentValidationExtensions
{
    internal static IRuleBuilderOptions<T, TProperty?> NotNullWithErrorCode<T, TProperty>(
        this IRuleBuilder<T, TProperty?> ruleBuilder,
        string errorCode = ErrorCodes.Required
    ) => ruleBuilder.NotNull().WithErrorCode(errorCode).WithMessage("{PropertyName} is required");

    internal static IRuleBuilderOptions<T, TProperty> NotEmptyWithErrorCode<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        string errorCode = ErrorCodes.Required
    ) => ruleBuilder.NotEmpty().WithErrorCode(errorCode).WithMessage("{PropertyName} is required");

    internal static IRuleBuilderOptions<T, string?> MaximumLengthWithErrorCode<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int maxLength = ApplicationConstants.DefaultMaxStringLength
    ) =>
        ruleBuilder
            .MaximumLength(maxLength)
            .WithErrorCode(ErrorCodes.MaxLength)
            .WithMessage("{PropertyName} cannot exceed {MaxLength} characters");

    internal static IRuleBuilderOptions<T, string?> ValidString<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool required,
        int maxLength = ApplicationConstants.DefaultMaxStringLength
    )
    {
        if (required)
            ruleBuilder = ruleBuilder.NotEmptyWithErrorCode();

        return ruleBuilder.MaximumLengthWithErrorCode(maxLength);
    }
}
