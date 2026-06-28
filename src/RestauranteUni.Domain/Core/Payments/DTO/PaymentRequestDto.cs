using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.Domain.Core.Payments.DTO;

public class PaymentRequestDto : IUseCaseRequest
{
    public Guid? OrderId { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; } = null!;
    public PaymentDetailsDto PaymentDetails { get; set; } = null!;
}

public class PaymentMethodDto
{
    public PaymentMethod Method { get; set; }
}

public class PaymentDetailsDto
{
    public decimal Amount { get; set; }
    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpirationDate { get; set; }
    public string? SecurityCode { get; set; }
    public string? PixKey { get; set; }
}
