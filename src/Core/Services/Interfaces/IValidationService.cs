using FluentValidation.Results;

namespace ConnectionManager.Core.Services.Interfaces;

internal interface IValidationService
{
    ValidationResult Validate<T>(T request);
}
