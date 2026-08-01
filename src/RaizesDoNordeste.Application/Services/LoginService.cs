using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using RaizesDoNordeste.Domain;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Login;
using RaizesDoNordeste.Domain.Services;

namespace RaizesDoNordeste.Application.Services;

public class LoginService : ILoginService
{
    private readonly ITokenService _tokenService;
    
    public LoginService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    public UserRefreshToken CreateRefreshToken(long accountId, Guid restaurantId)
    {
        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new UserRefreshToken
        {
            AccountId = accountId,
            Token = refreshTokenValue,
            ExpiresAt = Calendar.Now.AddDays(7),
            Revoked = false,
            Active = true,
            RestaurantId = restaurantId
        };
    }

    public List<Claim> MountRolesClaims(Account account)
    {
        var roles = account.RoleAccounts;
        return roles
            .Select(roleType => new { roleType.RoleId, roleType.RoleStatus, roleType.AccountId })
            .Select(value => new Claim(ClaimTypes.Role, JsonSerializer.Serialize(value)))
            .ToList();
    }

    public string GenerateToken(Account account, List<Claim> claims)
        => _tokenService.WriteToken(account.Id, account.Email.Value, claims);
}