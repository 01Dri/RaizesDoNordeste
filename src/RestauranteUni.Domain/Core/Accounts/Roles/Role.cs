namespace RestauranteUni.Domain.Core.Accounts.Roles;

public sealed class Role : IEquatable<Role>
{
    public RoleType Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool Equals(Role? other)
    {
        return other is not null && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Role role => Equals(role),
            RoleType roleType => Id == roleType,
            _ => false
        };
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Role? role, RoleType roleType)
    {
        return role?.Id == roleType;
    }

    public static bool operator !=(Role? role, RoleType roleType)
    {
        return !(role == roleType);
    }

    public static implicit operator Role(RoleType roleType)
    {
        return new Role
        {
            Id = roleType
        };
    }
}