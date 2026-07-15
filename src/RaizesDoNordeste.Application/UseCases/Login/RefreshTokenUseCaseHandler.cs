using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Login;
using RaizesDoNordeste.Domain.Services;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Login
{
    public sealed class RefreshTokenUseCaseHandler : IUseCaseHandler<RefreshRequestDto, LoginResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public RefreshTokenUseCaseHandler(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponseDto>> HandleAsync(RefreshRequestDto parameter, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(parameter.RefreshToken))
            {
                return Result<LoginResponseDto>.Failure(new Error("Refresh token é obrigatório."), HttpStatusCode.BadRequest);
            }

            var existingToken = await _context.UserRefreshTokens
                .Include(t => t.Account)
                .ThenInclude(a => a.RoleAccounts)
                .FirstOrDefaultAsync(t => t.Token == parameter.RefreshToken && !t.Revoked && t.ExpiresAt > Calendar.Now, cancellation);

            if (existingToken == null)
            {
                return Result<LoginResponseDto>.Failure(new Error("Refresh token inválido ou expirado."), HttpStatusCode.Unauthorized);
            }

            var restaurant = await _context.Restaurants
                .Select(x => new { x.Id, x.Name })
                .FirstOrDefaultAsync(x => x.Id == existingToken.RestaurantId, cancellation);

            if (restaurant == null)
            {
                return Result<LoginResponseDto>.Failure(new Error("Restaurante associado não encontrado."), HttpStatusCode.BadRequest);
            }

            // Revoke old token
            existingToken.Revoked = true;
            existingToken.Active = false;
            existingToken.UpdatedAt = Calendar.Now;

            // Generate new token
            var newRefreshTokenValue = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            var newRefreshToken = new UserRefreshToken
            {
                AccountId = existingToken.AccountId,
                Token = newRefreshTokenValue,
                ExpiresAt = Calendar.Now.AddDays(7),
                Revoked = false,
                RestaurantId = existingToken.RestaurantId
            };

            await _context.UserRefreshTokens.AddAsync(newRefreshToken, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var claims = MountRolesClaims(existingToken.Account);
            claims.Add(new Claim("restaurant_id", restaurant.Id.ToString()));
            claims.Add(new Claim("restaurant_name", restaurant.Name));

            var newJwtToken = _tokenService.WriteToken(existingToken.AccountId, existingToken.Account.Email.Value, claims);
            var response = new LoginResponseDto(newJwtToken, newRefreshTokenValue);

            return Result<LoginResponseDto>.Success(response);
        }

        private static List<Claim> MountRolesClaims(Account account)
        {
            var roles = account.RoleAccounts;
            var claims = new List<Claim>();

            foreach (var roleType in roles)
            {
                var value = new
                {
                    roleType.RoleId,
                    roleType.RoleStatus,
                    roleType.AccountId
                };
                claims.Add(new Claim(ClaimTypes.Role, JsonSerializer.Serialize(value)));
            }

            return claims;
        }
    }
}
