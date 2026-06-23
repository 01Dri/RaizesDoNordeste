using System.Security.Claims;
using System.Text.Json;
using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.Core.Users;

namespace RestauranteUni.API
{
    public sealed class CurrentUserContext : ICurrentUser
    {
        public readonly IHttpContextAccessor _accessor;
        public CurrentUserContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            var user = accessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                AccountId = long.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                RestaurantId = Guid.Parse(user.FindFirst("restaurant_id")!.Value);
                RestaurantName = user.FindFirst("restaurant_name")!.Value;
                Email = user.FindFirst(ClaimTypes.Email)!.Value;
            }
        }
        public long AccountId { get; }
        public Guid RestaurantId { get; }
        public string RestaurantName { get; set; }
        public string Email { get; }
        
        public List<RoleType> Roles { get; }
        
        public bool InRole(RoleType role)
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var rolesAccount = GetRolesAccount(user);
                var roleAccount = rolesAccount.FirstOrDefault(x => x.RoleId == role);
                if (roleAccount == null)
                {
                    return false;
                }
                return roleAccount.RoleStatus == RoleStatus.Enable && roleAccount.AccountId == AccountId;
            }

            return false;
        }

        private List<RoleAccount> GetRolesAccount(ClaimsPrincipal user)
        {
            var userRolesValues = user.Claims.Where(x => x.Type == ClaimTypes.Role)
                .Select(x => x.Value)
                .ToList();

            var roles = new List<RoleAccount>();
            foreach (var value in userRolesValues)
            {
                roles.Add(JsonSerializer.Deserialize<RoleAccount>(value));
            }
            return roles;
        }
        
    }
}
