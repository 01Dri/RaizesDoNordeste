using FluentValidation;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;

namespace RaizesDoNordeste.Application.UseCases.Stocks.Validations
{
    public class AddStockIngredientDtoValidation : AbstractValidator<AddStockIngredientDto>
    {
        public AddStockIngredientDtoValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do ingrediente é obrigatório.")
                .MaximumLength(100).WithMessage("O nome do ingrediente deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("A quantidade do ingrediente deve ser maior ou igual a zero.");
        }
    }
}
