using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public class AddStockIngredientDto : IUseCaseRequest
    {
        public string Name { get; set; } = null!;
        public IngredientUnit Unit { get; set; } = IngredientUnit.Unit;
        public decimal Quantity { get; set; }
    }
}
