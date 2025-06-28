using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IValidatorFactory = ConnectionManager.Core.Services.Interfaces.IValidatorFactory;

namespace ConnectionManager.Core.Services.Implementations;

internal sealed class ValidatorFactory : IValidatorFactory
{
    #region construction

    private readonly ILogger<ValidatorFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ValidatorFactory(ILogger<ValidatorFactory> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    #endregion

    public List<IValidator<T>> GetValidators<T>()
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
}
