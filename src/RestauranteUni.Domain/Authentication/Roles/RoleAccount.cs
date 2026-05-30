namespace RestauranteUni.Domain.Authentication.Roles
{
    public class RoleAccount
    {
        public long Id { get; set; }

        public long AccountId { get; set; }
        public virtual Account Account { get; set; } = null!;

        public long RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public RoleStatus RoleStatus { get; set; }
    }
}
