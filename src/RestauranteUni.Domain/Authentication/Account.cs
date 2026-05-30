using RestauranteUni.Domain.Authentication.Roles;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Domain.Authentication
{
    public class Account : BaseDomain<long>
    {
        public Email Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public virtual List<RoleAccount> Roles { get; set; } = [];
    }
}
