using System.Security.Claims;
using RaizesDoNordeste.Domain.Core.Accounts;

namespace RaizesDoNordeste.Domain.Services;

public interface ILoginService
{
    Task<UserRefreshToken> CreateRefreshTokenAsync(CancellationToken cancellationToken = default);
    List<Claim> MountRolesClaims(Account account);
    string GenerateToken(Account account, List<Claim> claims);
}