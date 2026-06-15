using RestauranteUni.Domain.Core.Restaurants;

namespace RestauranteUni.Domain.Core.Menus;

public class Menu : BaseDomain<long>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public virtual List<MenuItem> Items { get; set; } = [];

    public Guid? RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
}