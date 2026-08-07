using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public class ProductIngredientInputDto
    {
        public long? StockIngredientId { get; set; }
        public string? Name { get; set; }
        public IngredientUnit Unit { get; set; } = IngredientUnit.Unit;
        public decimal QuantityUseToOrder { get; set; } = 1;
        public decimal InitialStockQuantity { get; set; } = 100;
    }

    public record CreateProductDto : IUseCaseRequest
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsAvailable { get; init; } = true;
        public int DisplayOrder { get; init; }
        public int PreparationTimeInMinutes { get; init; }
        public bool IsFeatured { get; init; }
        public List<ProductIngredientInputDto>? Ingredients { get; init; }
    }
}
