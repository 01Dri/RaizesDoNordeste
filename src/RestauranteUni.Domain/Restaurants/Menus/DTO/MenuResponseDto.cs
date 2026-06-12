using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.Domain.Restaurants.Menus.DTO
{
    public sealed class MenuResponseDto : IUseCaseResponse
    {
        public string Name { get; set; }
        public string RestaurantName { get; set; }

        public IReadOnlyCollection<MenuItemResponseDto> Items { get; set; } = [];

    }
}
