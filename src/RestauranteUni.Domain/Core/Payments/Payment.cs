using RestauranteUni.Domain.Core.Ingredients.Enums;

namespace RestauranteUni.Domain.Core.Payments;

public class Payment : BaseDomain<long>
{
    public decimal Total { get; set; }
    public decimal TotalPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Description { get; set; }
    public virtual PaymentOrder? PaymentOrder { get; set; } 
}