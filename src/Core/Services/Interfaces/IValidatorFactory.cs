using FluentValidation;

namespace ConnectionManager.Core.Services.Interfaces;

internal interface IValidatorFactory
{
    List<IValidator<T>> GetValidators<T>();
}
