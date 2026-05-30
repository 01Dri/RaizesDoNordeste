using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Domain.Authentication
{
    public class Account : BaseDomain<long>
    {
        public Email Email { get; set; }
        public string Password { get; set; }
        //public virtual List<Roles> Roles { get; set; }
    }
}
