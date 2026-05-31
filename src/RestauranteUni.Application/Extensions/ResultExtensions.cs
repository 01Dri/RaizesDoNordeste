using FluentValidation.Results;
using RestauranteUni.Domain;

namespace RestauranteUni.Application.Extensions
{
    public static class ResultExtensions
    {

        extension(ValidationResult validation)
        {
            public Result<T> ToResultFailure<T>()
            {
                return new Result<T>(
                    validation.Errors
                        .GroupBy(x => x.PropertyName)
                        .Select(group => new Validation(
                            group.Key,
                            group.Select(x => x.ErrorMessage).ToList()))
                        .ToList());
            }
        }
    }
}
