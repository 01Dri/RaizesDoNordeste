using System;
using FluentValidation;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;

namespace RaizesDoNordeste.Application.UseCases.Stocks.Validations
{
    public sealed class StockMovementRequestDtoValidator : AbstractValidator<StockMovementRequestDto>
    {
        public StockMovementRequestDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => (x.PublicStockIngredientId.HasValue && x.PublicStockIngredientId != Guid.Empty) || x.StockIngredientId > 0)
                .WithMessage("O identificador do ingrediente de estoque (PublicId ou ID) é obrigatório.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Tipo de movimentação inválido.");
        }
    }
}
