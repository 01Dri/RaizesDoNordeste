using FluentValidation;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;

namespace RaizesDoNordeste.Application.UseCases.Stocks.Validations
{
    public class CreateStockDtoValidation : AbstractValidator<CreateStockRequestDto>
    {
        public CreateStockDtoValidation()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("O ID do restaurante é obrigatório.");

            RuleForEach(x => x.Items).ChildRules(items =>
            {
                items.RuleFor(i => i.Name)
                    .NotEmpty().WithMessage("O nome do ingrediente é obrigatório.");
                items.RuleFor(i => i.Quantity)
                    .GreaterThanOrEqualTo(0).WithMessage("A quantidade não pode ser negativa.");
            });
        }
    }
}
