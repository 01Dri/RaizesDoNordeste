using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public record StockMovementRequestDto : IUseCaseRequest
    {
        public long StockIngredientId { get; init; }
        public decimal Quantity { get; init; }
        public StockMovementType Type { get; init; }
        public string? Description { get; init; }
    }
}
