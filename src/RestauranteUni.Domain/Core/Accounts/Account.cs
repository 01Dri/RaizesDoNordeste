using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Domain.Core.Accounts;

public class Account : BaseDomain<long>
{
    public Email Email { get; set; } 
    public string Password { get; set; } = null!;
    public virtual ICollection<RoleAccount> RoleAccounts { get; set; } = [];
    public virtual ICollection<Order> Orders { get; set; } = [];
}