using RaizesDoNordeste.Domain.UseCases;
using System;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public class StockIngredientResponseDto : IUseCaseResponse
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public string Name { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public Error? ErrorResponse { get; set; }
    }
}
