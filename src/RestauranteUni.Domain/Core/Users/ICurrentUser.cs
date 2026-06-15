namespace RestauranteUni.Domain.Core.Users;

public interface ICurrentUser
{
    public long AccountId { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; }
    public string Email { get; set; }
}