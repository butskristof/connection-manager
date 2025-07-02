using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConnectionManager.Core.Common.Validation;

internal interface IValidationService
{
    ValidationResult Validate<T>(T request);
}

internal sealed class ValidationService : IValidationService
{
    #region construction

    private readonly ILogger<ValidationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(ILogger<ValidationService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    #endregion

    private List<IValidator<T>> GetValidators<T>()
    {
        var typeName = typeof(T).Name;
        _logger.LogDebug("Resolving validators for type {TypeName}", typeName);

        var validators = _serviceProvider.GetServices<IValidator<T>>().ToList();

        _logger.LogDebug(
            "Found {ValidatorCount} validator(s) for type {TypeName}",
            validators.Count,
            typeName
        );

        return validators;
    }

    public ValidationResult Validate<T>(T request)
    {
        var validators = GetValidators<T>();
        var allFailures = validators
            .SelectMany(validator => validator.Validate(request).Errors)
            .ToList();

        _logger.LogDebug(
            "Validation completed for type {TypeName} with {FailureCount} failure(s)",
            typeof(T).Name,
            allFailures.Count
        );

        return new ValidationResult(allFailures);
    }
}
