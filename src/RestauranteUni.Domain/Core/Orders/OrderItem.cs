using RestauranteUni.Domain.Core.Menus;

namespace RestauranteUni.Domain.Core.Orders;

public class OrderItem 
{
    public long? Id { get; set; }
    public long? OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public long? MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public decimal Quantity { get; set; }
    
}