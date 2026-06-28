using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.Domain.Core.Payments.DTO;

public class PaymentResponseDto : IUseCaseResponse
{
    public Guid OrderId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal AmountPaid { get; set; }
    public Error? ErrorResponse { get; set; }
}
