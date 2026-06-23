using RestauranteUni.Domain.Core.Menus;

namespace RestauranteUni.Domain.Core.Orders.DTO;

public sealed class IngredientOrderConsumptionDto
{
    public MenuItemIngredient Ingredient { get; set; }
    public decimal QuantityToUseInOrder { get; set; }
}