using RestauranteUni.Domain.Core.Menus;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Stocks;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Domain.Core.Restaurants;

public class Restaurant : BaseDomain<Guid>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Phone Phone { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public Cnpj Cnpj { get; set; } = null!;
    public virtual Menu? Menu { get; set; }
    public virtual Stock Stock { get; set; } = null!;
    
    public virtual ICollection<Order> Orders { get; set; } = [];
}