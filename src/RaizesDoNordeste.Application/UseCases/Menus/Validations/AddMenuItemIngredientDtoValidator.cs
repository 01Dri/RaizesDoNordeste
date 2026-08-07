using System;
using FluentValidation;
using RaizesDoNordeste.Domain.Core.Menus.DTO;

namespace RaizesDoNordeste.Application.UseCases.Menus.Validations
{
    public class AddMenuItemIngredientDtoValidator : AbstractValidator<AddMenuItemIngredientDto>
    {
        public AddMenuItemIngredientDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => x.PublicMenuItemId != Guid.Empty || x.MenuItemId > 0)
                .WithMessage("O identificador do item do cardápio (PublicId ou ID) é obrigatório.");

            RuleFor(x => x)
                .Must(x => (x.PublicStockIngredientId.HasValue && x.PublicStockIngredientId != Guid.Empty) ||
                           (x.StockIngredientId.HasValue && x.StockIngredientId > 0) ||
                           !string.IsNullOrWhiteSpace(x.Name))
                .WithMessage("Informe o PublicId do insumo, o StockIngredientId ou o Nome do ingrediente a ser cadastrado.");

            RuleFor(x => x.QuantityUseToOrder)
                .GreaterThan(0).WithMessage("A quantidade utilizada por pedido deve ser maior que zero.");
        }
    }
}
