using System;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public class AddMenuItemIngredientDto : IUseCaseRequest
    {
        public Guid PublicMenuItemId { get; set; }
        public long MenuItemId { get; set; }
        public Guid? PublicStockIngredientId { get; set; }
        public long? StockIngredientId { get; set; }
        public string? Name { get; set; }
        public IngredientUnit Unit { get; set; } = IngredientUnit.Unit;
        public decimal QuantityUseToOrder { get; set; } = 1;
        public decimal InitialStockQuantity { get; set; } = 100;
    }
}
