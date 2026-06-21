using FluentValidation;
using RestauranteUni.Domain.Core.Orders.DTO;

namespace RestauranteUni.Application.UseCases.Orders.Validations;

public sealed class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Channel)
            .IsInEnum()
            .WithMessage("Invalid order channel.");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Items is required.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("The order must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemValidator());
    }
}