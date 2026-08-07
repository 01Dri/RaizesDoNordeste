using System;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public class AddMenuItemIngredientResponseDto : IUseCaseResponse
    {
        public long Id { get; set; }
        public long MenuItemId { get; set; }
        public Guid PublicMenuItemId { get; set; }
        public string MenuItemName { get; set; } = null!;
        public long StockIngredientId { get; set; }
        public Guid PublicStockIngredientId { get; set; }
        public string StockIngredientName { get; set; } = null!;
        public decimal QuantityUseToOrder { get; set; }
        public Error? ErrorResponse { get; set; }
    }
}
