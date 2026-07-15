using FluentValidation;
using RaizesDoNordeste.Domain.Core.Payments.DTO;

namespace RaizesDoNordeste.Application.UseCases.Payments.Validations;

public sealed class PaymentRequestDtoValidator : AbstractValidator<PaymentRequestDto>
{
    public PaymentRequestDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotNull()
            .WithMessage("O ID do pedido é obrigatório.")
            .NotEqual(Guid.Empty)
            .WithMessage("O ID do pedido é obrigatório.");

        RuleFor(x => x.PaymentMethod)
            .NotNull()
            .WithMessage("A forma de pagamento é obrigatória.");

        RuleFor(x => x.PaymentMethod)
            .SetValidator(new PaymentMethodDtoValidator())
            .When(x => x.PaymentMethod != null);

    }
}

public sealed class PaymentMethodDtoValidator : AbstractValidator<PaymentMethodDto>
{
    public PaymentMethodDtoValidator()
    {
        RuleFor(x => x.Method)
            .IsInEnum()
            .WithMessage("Forma de pagamento inválida.");
    }
}

