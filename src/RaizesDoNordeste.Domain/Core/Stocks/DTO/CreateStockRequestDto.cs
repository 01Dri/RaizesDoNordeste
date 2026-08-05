using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public class CreateStockRequestDto : IUseCaseRequest
    {
        public Guid RestaurantId { get; set; }
        public List<CreateStockIngredientItemDto>? Items { get; set; }
    }

    public class CreateStockIngredientItemDto
    {
        public string Name { get; set; } = null!;
        public IngredientUnit Unit { get; set; }
        public decimal Quantity { get; set; }
    }
}
