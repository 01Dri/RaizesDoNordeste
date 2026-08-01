using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Services;

namespace RaizesDoNordeste.Application.Services;

public class LoginService  : ILoginService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    
    public LoginService(ApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }
    
    public async Task<UserRefreshToken> CreateRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = GenerateRefreshTokenHash();
        var existingToken = await _context.UserRefreshTokens
            .Include(t => t.Account)
            .ThenInclude(a => a.RoleAccounts).Include(userRefreshToken => userRefreshToken.Account)
            .ThenInclude(account => account.Email)
            .FirstOrDefaultAsync(t => t.Token == refreshToken  && !t.Revoked && t.ExpiresAt > Calendar.Now, cancellationToken);

        var newRefreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var newRefreshToken = new UserRefreshToken
        {
            AccountId = existingToken.AccountId,
            Token = newRefreshTokenValue,
            ExpiresAt = Calendar.Now.AddDays(7),
            Revoked = false,
            RestaurantId = existingToken.RestaurantId
        };

        return newRefreshToken;
    }

    public List<Claim> MountRolesClaims(Account account)
    {
        var roles = account.RoleAccounts;
        return roles
            .Select(roleType => new { roleType.RoleId, roleType.RoleStatus, roleType.AccountId })
            .Select(value => new Claim(ClaimTypes.Role, JsonSerializer.Serialize(value))).ToList();
    }

    public string GenerateToken(Account account, List<Claim> claims)
        =>  _tokenService.WriteToken(account.Id, account.Email.Value, claims);

    private static string GenerateRefreshTokenHash()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}