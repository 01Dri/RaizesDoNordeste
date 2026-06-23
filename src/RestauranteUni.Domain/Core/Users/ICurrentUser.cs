using RestauranteUni.Domain.Core.Accounts.Roles;

namespace RestauranteUni.Domain.Core.Users;

public interface ICurrentUser
{
    public long AccountId { get;}
    public Guid RestaurantId { get;  }
    public string RestaurantName { get;  }
    public string Email { get;  }
    public bool InRole(RoleType role);
}