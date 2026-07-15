using FluentValidation;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;

namespace RaizesDoNordeste.Application.UseCases.Stocks.Validations
{
    public sealed class StockMovementRequestDtoValidator : AbstractValidator<StockMovementRequestDto>
    {
        public StockMovementRequestDtoValidator()
        {
            RuleFor(x => x.StockIngredientId)
                .GreaterThan(0).WithMessage("ID do ingrediente de estoque inválido.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Tipo de movimentação inválido.");
        }
    }
}
