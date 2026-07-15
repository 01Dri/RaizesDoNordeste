using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public class StockResponseDto : IUseCaseResponse
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = null!;
        public List<StockIngredientDto> Items { get; set; } = [];
        public Error? ErrorResponse { get; set; }
    }
}
