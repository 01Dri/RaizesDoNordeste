using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Payments.DTO;

public class PaymentResponseDto : IUseCaseResponse
{
    public Guid OrderId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal AmountPaid { get; set; }
    public int EarnedLoyaliyPoints { get; set; }
    public int? TotalPointsInRestaurant { get; set; }
    public Error? ErrorResponse { get; set; }
}
