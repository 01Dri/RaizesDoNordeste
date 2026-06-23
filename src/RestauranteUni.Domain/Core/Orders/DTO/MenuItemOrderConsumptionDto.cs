using RestauranteUni.Domain.Core.Menus;

namespace RestauranteUni.Domain.Core.Orders.DTO;

public sealed class MenuItemOrderConsumptionDto
{
    public long ItemId { get; set; }
    public MenuItem Item { get; set; }
    public List<IngredientOrderConsumptionDto> IngredientConsumptions { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public decimal TotalQuantity { get; set; }
    public bool HaveIngredientStock { get; set; } = true;
    public List<string> IngredientsWithoutStock { get; set; } = [];
}