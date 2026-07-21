using RaizesDoNordeste.Domain.Core.Ingredients.Enums;

namespace RaizesDoNordeste.Domain.Core.Payments;

public class Payment : BaseDomain<long>
{
    public decimal Total { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDiscount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Description { get; set; }
    public string? ExternalPaymentId { get; set; }
    public virtual PaymentOrder? PaymentOrder { get; set; } 
}
