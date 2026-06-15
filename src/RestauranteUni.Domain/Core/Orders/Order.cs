using RestauranteUni.Domain.Core.Accounts;
using RestauranteUni.Domain.Core.Restaurants;

namespace RestauranteUni.Domain.Core.Orders;

public class Order : BaseDomain<long>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long? AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public Guid? RestaurantId { get; set; }
    public virtual Restaurant Restaurant { get; set; } = null!;
    public virtual ICollection<OrderItem> Items { get; set; } = [];
}