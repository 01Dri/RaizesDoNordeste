using FluentValidation;
using RestauranteUni.Domain.Core.Orders.DTO;

namespace RestauranteUni.Application.UseCases.Orders.Validations;

public sealed class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Channel)
            .IsInEnum()
            .WithMessage("Canal de pedido inválido.");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Os itens são obrigatórios.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("O pedido deve conter pelo menos um item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemValidator());
    }
}