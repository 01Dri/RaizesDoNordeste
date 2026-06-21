using FluentValidation;
using RestauranteUni.Domain.Core.Orders.DTO;

namespace RestauranteUni.Application.UseCases.Orders.Validations;

public sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.PublicMenuItemId)
            .NotEmpty()
            .WithMessage("Menu item id is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");
        
    }
}