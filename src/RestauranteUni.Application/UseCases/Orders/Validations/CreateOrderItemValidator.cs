using FluentValidation;
using RestauranteUni.Domain.Core.Orders.DTO;

namespace RestauranteUni.Application.UseCases.Orders.Validations;

public sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.PublicMenuItemId)
            .NotEmpty()
            .WithMessage("O ID do item do cardápio é obrigatório.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("A quantidade deve ser maior que zero.");
        
    }
}