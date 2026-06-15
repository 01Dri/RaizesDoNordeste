using RestauranteUni.Domain.Core.Stocks;

namespace RestauranteUni.Domain.Core.Menus;

public class MenuItemIngredient
{
    public long? Id { get; set; }
    public long? MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public long? StockIngredientId { get; set; }
    public StockIngredient StockIngredient { get; set; } = null!;

    public decimal Quantity { get; set; }
}