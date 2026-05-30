namespace RestauranteUni.Domain.Authentication.Roles
{
    public sealed class Role
    {
        public int Id { get; set; }
        public RoleType Type { get; set; }
        public string Name { get; set; } = null!;
    }
}
