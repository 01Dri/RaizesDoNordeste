using System.Security.Claims;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Login;

namespace RaizesDoNordeste.Domain.Services;

public interface ILoginService
{
    UserRefreshToken CreateRefreshToken(long accountId, Guid restaurantId);
    List<Claim> MountRolesClaims(Account account);
    string GenerateToken(Account account, List<Claim> claims);
}