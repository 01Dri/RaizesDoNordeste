using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.Core.Users;

namespace RestauranteUni.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RolesAuthorizeAttribute : TypeFilterAttribute
{
    public RolesAuthorizeAttribute(params RoleType[] roles)
        : base(typeof(RolesAuthorizeFilter))
    {
        Arguments = [roles];
    }
}

public class RolesAuthorizeFilter : IAuthorizationFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly RoleType[] _roles;

    public RolesAuthorizeFilter(
        ICurrentUser currentUser,
        RoleType[] roles)
    {
        _currentUser = currentUser;
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!_roles.All(_currentUser.InRole))
        {
            context.Result = new ForbidResult();
        }
    }
}