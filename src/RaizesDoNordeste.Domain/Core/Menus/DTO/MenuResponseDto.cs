using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO;

public sealed class MenuResponseDto : IUseCaseResponse
{
    public string Name { init; get; }
    public string RestaurantName { init; get; }
    public Guid RestaurantId { init; get; }
    public IReadOnlyCollection<MenuItemResponseDto> Items { init; get; } = [];
    public Error? ErrorResponse { set; get; }
}
