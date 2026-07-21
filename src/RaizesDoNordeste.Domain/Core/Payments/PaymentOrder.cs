using RaizesDoNordeste.Domain.Core.Orders;

namespace RaizesDoNordeste.Domain.Core.Payments;

public class PaymentOrder 
{
    public long Id { get; set; }
    public long? OrderId { get; set; }
    public virtual Order Order { get; set; }
    public long? PaymentId { get; set; }
    public virtual Payment Payment { get; set; }

    public bool UsedLoyalityPoints { get; set; }

}
