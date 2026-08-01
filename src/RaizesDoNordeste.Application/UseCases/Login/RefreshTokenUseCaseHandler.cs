using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain;
using RaizesDoNordeste.Domain.Core.Login;
using RaizesDoNordeste.Domain.Services;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Login
{
    public sealed class RefreshTokenUseCaseHandler : IUseCaseHandler<RefreshRequestDto, LoginResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoginService _loginService;

        public RefreshTokenUseCaseHandler(ApplicationDbContext context,ILoginService loginService)
        {
            _context = context;
            _loginService = loginService;
        }

        public async Task<Result<LoginResponseDto>> HandleAsync(RefreshRequestDto parameter, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(parameter.RefreshToken))
            {
                return Result<LoginResponseDto>.Failure(new Error("Refresh token é obrigatório."));
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
                return Result<LoginResponseDto>.Failure(new Error("Restaurante associado não encontrado."));
            }

            existingToken.Revoked = true;
            existingToken.Active = false;
            existingToken.UpdatedAt = Calendar.Now;

            var newRefreshToken = _loginService.CreateRefreshToken(existingToken.AccountId, existingToken.RestaurantId);

            await _context.UserRefreshTokens.AddAsync(newRefreshToken, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var claims = _loginService.MountRolesClaims(existingToken.Account);
            claims.Add(new Claim("restaurant_id", restaurant.Id.ToString()));
            claims.Add(new Claim("restaurant_name", restaurant.Name));

            var newJwtToken = _loginService.GenerateToken(existingToken.Account, claims);
            var response = new LoginResponseDto(newJwtToken, newRefreshToken.Token);

            return Result<LoginResponseDto>.Success(response);
        }

    }
}
