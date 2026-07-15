using System;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public class StockIngredientDto
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public string Name { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
