using RestauranteUni.Domain.Core.Accounts;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Payments;
using RestauranteUni.Domain.Core.Restaurants;
using RestauranteUni.Domain.Core.Stocks;

namespace RestauranteUni.Domain.Core.Orders;

public class Order : BaseDomain<long>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public required OrderStatus Status { get; set; }
    public required OrderChannel Channel { get; set; }
    public long? AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public required Guid RestaurantId { get; set; }
    public virtual Restaurant Restaurant { get; set; } = null!;
    public virtual ICollection<OrderItem> Items { get; set; } = [];

    public virtual ICollection<StockIngredientMovement> StockIngredientMovements { get; set; } = [];
    
    public virtual PaymentOrder? PaymentOrder { get; set; }  
    
    public decimal TotalPrice { get; set; }

}