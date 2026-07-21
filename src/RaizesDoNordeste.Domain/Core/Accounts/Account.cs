using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Loyalit;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Domain.Core.Accounts;

public class Account : BaseDomain<long>
{
    public Email Email { get; set; } 
    public string Password { get; set; } = null!;
    public long? LoyalitProgramId { get; set; }
    public virtual LoyalitProgram? LoyalitProgram { get; set; }
    public virtual ICollection<RoleAccount> RoleAccounts { get; set; } = [];
    public virtual ICollection<Order> Orders { get; set; } = [];

    public bool HaveLoyalityProgramInRestaurant(Guid restaurantId)
    {
        return LoyalitProgram != null && LoyalitProgram.RestaurantId == restaurantId;
    }
}
