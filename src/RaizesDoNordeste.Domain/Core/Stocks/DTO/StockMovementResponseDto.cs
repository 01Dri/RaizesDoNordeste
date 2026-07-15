using RaizesDoNordeste.Domain.UseCases;
using System;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public record StockMovementResponseDto : IUseCaseResponse
    {
        public long Id { get; init; }
        public long StockIngredientId { get; init; }
        public string IngredientName { get; init; } = null!;
        public decimal QuantityMoved { get; init; }
        public decimal NewStockQuantity { get; init; }
        public string Type { get; init; } = null!;
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
        public Error? ErrorResponse { get; set; }
    }
}
