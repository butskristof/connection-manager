using ErrorOr;
using FluentValidation.Results;

namespace ConnectionManager.Core.Common;

internal static class ErrorOrExtensions
{
    internal static Error ToValidationError(this ValidationFailure validationFailure) =>
        Error.Validation(
            validationFailure.ErrorCode,
            validationFailure.ErrorMessage,
            new Dictionary<string, object> { { "Property", validationFailure.PropertyName } }
        );

    internal static List<Error> ToValidationErrors(
        this IEnumerable<ValidationFailure> validationFailures
    ) => validationFailures.Select(ToValidationError).ToList();

    internal static List<Error> ToValidationErrors(this ValidationResult validationResult) =>
        validationResult.Errors.ToValidationErrors();
}
