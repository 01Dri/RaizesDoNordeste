using System.Net;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Login;
using RaizesDoNordeste.Domain.Services;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Login
{
    public sealed class LoginUseCaseHandler : IUseCaseHandler<LoginDto,  LoginResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<LoginDto> _validator;
        private readonly IHasherService _hasherService;
        private readonly ITokenService _tokenService;
        public LoginUseCaseHandler
        (
            ApplicationDbContext context,   
            IValidator<LoginDto> validator,
            IHasherService hasherService,
            ITokenService tokenService
        )
        {
            _context = context;
            _validator = validator;
            _hasherService = hasherService;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponseDto>> HandleAsync(LoginDto parameter, CancellationToken cancellation = default)
        {
            var t = 2;
            t = 3;

            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
            {
                var propertyName = validation.Errors.First().PropertyName!;
                return Result<LoginResponseDto>.Failure
                (
                    [new Validation(propertyName, $"{propertyName} inválido")]
                );
            } 
            
            
             
            var email = new Email(parameter.Email);
            var account = await _context.Accounts.Include(x => x.RoleAccounts)
                .FirstOrDefaultAsync(x => x.Email == email, cancellation);
            if (account == null || !_hasherService.VerifyPassword(parameter.Password, account.Password))
            {
                return Result<LoginResponseDto>.Failure
                (
                    [new Validation("Email", "Credenciais inválidas")],
                    HttpStatusCode.Unauthorized
                );
            }
            var restaurant = await _context.Restaurants.Select(x => new 
                {
                   x.Id,
                   x.Name
                })
                .FirstOrDefaultAsync(x => x.Id == parameter.RestaurantId, cancellation);


            if (restaurant == null)
            {
                return Result<LoginResponseDto>.FailureNotFound("Restaurante não encontrado.");
            }

            var claims = MountRolesClaims(account);
            claims.Add(new Claim("restaurant_id", restaurant.Id.ToString()));
            claims.Add(new Claim("restaurant_name", restaurant.Name));

            var refreshTokenValue = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            var userRefreshToken = new RaizesDoNordeste.Domain.Core.Accounts.UserRefreshToken
            {
                AccountId = account.Id,
                Token = refreshTokenValue,
                ExpiresAt = RaizesDoNordeste.Domain.Calendar.Now.AddDays(7),
                Revoked = false,
                RestaurantId = restaurant.Id
            };
            await _context.UserRefreshTokens.AddAsync(userRefreshToken, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var token = _tokenService.WriteToken(account.Id, account.Email.Value, claims);
            var response = new LoginResponseDto(token, refreshTokenValue);

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


    class Teste
    {
        public string Conta { get; init; }
        public List<RolesTest> Roles { get; set; }
    }

    class RolesTest
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}

