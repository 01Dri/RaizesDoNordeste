using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RestauranteUni.Domain.Core.Users;

namespace RestauranteUni.API
{
    public sealed class CurrentUserContext : ICurrentUser
    {

        public CurrentUserContext(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                AccountId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                RestaurantId = Guid.Parse(user.FindFirst("restaurant_id")!.Value);
                RestaurantName = user.FindFirst("restaurant_name")!.Value;
                Email = user.FindFirst(ClaimTypes.Email)!.Value;
            }
        }
        public long AccountId { get; set; }
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string Email { get; set; }
    }
}
