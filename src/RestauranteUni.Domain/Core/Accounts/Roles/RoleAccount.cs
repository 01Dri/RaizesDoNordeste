namespace RestauranteUni.Domain.Core.Accounts.Roles;

public class RoleAccount
{
    public long? Id { get; set; }
    public long? AccountId { get; set; }
    public virtual Account? Account { get; set; } = null!;
    public RoleType? RoleId { get; set; }
    public virtual Role? Role { get; set; } = null!;
    public required RoleStatus RoleStatus { get; set; }

    public static RoleAccount Create(RoleType roleType, RoleStatus roleStatus)
    {
        return new RoleAccount
        {
            RoleStatus = roleStatus,
            RoleId = roleType
        };
    }
}