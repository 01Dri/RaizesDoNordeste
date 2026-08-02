using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Payments.DTO;

public class PaymentRequestDto : IUseCaseRequest
{
    public Guid? OrderId { get; set; }
    public PaymentMethodDto PaymentMethod { get; init; } = null!;
    public bool UseLoyalityPoints => LoyalityPointToUse > 0;
    public int LoyalityPointToUse { get; init;  }
}

public class PaymentMethodDto
{
    public PaymentMethod Method { get; init; }
}

