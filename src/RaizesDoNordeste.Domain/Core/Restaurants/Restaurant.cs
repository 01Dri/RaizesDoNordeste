using RaizesDoNordeste.Domain.Core.Loyalit;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Domain.Core.Restaurants;

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
    public long? LoyalityProgramId { get; set; }
    public virtual LoyalitProgram? LoyalitProgram { get; set; }        
}
