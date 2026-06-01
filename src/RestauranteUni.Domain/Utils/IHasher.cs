namespace RestauranteUni.Domain.Utils
{
    public interface IHasher
    {
        string HashPassword(string value);
    }
}
