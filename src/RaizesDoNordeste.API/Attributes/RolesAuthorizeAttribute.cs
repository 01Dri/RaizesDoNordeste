using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Users;

namespace RaizesDoNordeste.API.Attributes;

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
        if (!_roles.Any(_currentUser.InRole))
        {
            context.Result = new ForbidResult();
        }
    }
}
