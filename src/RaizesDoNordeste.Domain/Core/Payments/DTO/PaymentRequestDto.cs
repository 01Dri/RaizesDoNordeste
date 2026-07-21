using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Payments.DTO;

public class PaymentRequestDto : IUseCaseRequest
{
    public Guid? OrderId { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; } = null!;
    public bool UseLoyalityPoints { get; set; }
}

public class PaymentMethodDto
{
    public PaymentMethod Method { get; set; }
}

