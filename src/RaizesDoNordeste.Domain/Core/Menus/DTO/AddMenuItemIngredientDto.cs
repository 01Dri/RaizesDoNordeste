using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public class AddMenuItemIngredientDto : IUseCaseRequest
    {
        public long MenuItemId { get; set; }
        public long StockIngredientId { get; set; }
        public decimal QuantityUseToOrder { get; set; }
    }
}
