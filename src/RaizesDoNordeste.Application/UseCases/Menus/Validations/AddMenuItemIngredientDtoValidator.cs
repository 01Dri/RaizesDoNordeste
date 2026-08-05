using FluentValidation;
using RaizesDoNordeste.Domain.Core.Menus.DTO;

namespace RaizesDoNordeste.Application.UseCases.Menus.Validations
{
    public class AddMenuItemIngredientDtoValidator : AbstractValidator<AddMenuItemIngredientDto>
    {
        public AddMenuItemIngredientDtoValidator()
        {
            RuleFor(x => x.MenuItemId)
                .GreaterThan(0).WithMessage("O ID do item do cardápio é obrigatório.");

            RuleFor(x => x.StockIngredientId)
                .GreaterThan(0).WithMessage("O ID do ingrediente do estoque é obrigatório.");

            RuleFor(x => x.QuantityUseToOrder)
                .GreaterThan(0).WithMessage("A quantidade utilizada por pedido deve ser maior que zero.");
        }
    }
}
