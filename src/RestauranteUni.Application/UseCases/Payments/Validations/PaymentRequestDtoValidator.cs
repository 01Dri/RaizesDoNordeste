using FluentValidation;
using RestauranteUni.Domain.Core.Payments.DTO;

namespace RestauranteUni.Application.UseCases.Payments.Validations;

public sealed class PaymentRequestDtoValidator : AbstractValidator<PaymentRequestDto>
{
    public PaymentRequestDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("O ID do pedido é obrigatório.");

        RuleFor(x => x.PaymentMethod)
            .NotNull()
            .WithMessage("A forma de pagamento é obrigatória.");

        RuleFor(x => x.PaymentMethod)
            .SetValidator(new PaymentMethodDtoValidator())
            .When(x => x.PaymentMethod != null);

        RuleFor(x => x.PaymentDetails)
            .NotNull()
            .WithMessage("As informações necessárias para o pagamento são obrigatórias.");

        RuleFor(x => x.PaymentDetails)
            .SetValidator(new PaymentDetailsDtoValidator())
            .When(x => x.PaymentDetails != null);
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

public sealed class PaymentDetailsDtoValidator : AbstractValidator<PaymentDetailsDto>
{
    public PaymentDetailsDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("O valor do pagamento deve ser maior que zero.");
    }
}
