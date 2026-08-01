using System.Net;
using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
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
        private readonly ILoginService _loginService;
        public LoginUseCaseHandler
        (
            ApplicationDbContext context,   
            IValidator<LoginDto> validator,
            IHasherService hasherService,
            ILoginService loginService
        )
        {
            _context = context;
            _validator = validator;
            _hasherService = hasherService;
            _loginService = loginService;
        }

        public async Task<Result<LoginResponseDto>> HandleAsync(LoginDto parameter, CancellationToken cancellation = default)
        {
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
            var account = await _context.Accounts
                .Include(x => x.RoleAccounts)
                .FirstOrDefaultAsync(x => x.Email == email, cancellation);
            
            if (account == null || !_hasherService.VerifyPassword(parameter.Password, account.Password))
            {
                return Result<LoginResponseDto>.Failure
                (
                    new Error("Credenciais inválidas"),
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

            var claims = _loginService.MountRolesClaims(account);
            claims.Add(new Claim("restaurant_id", restaurant.Id.ToString()));
            claims.Add(new Claim("restaurant_name", restaurant.Name));


            var userRefreshToken = _loginService.CreateRefreshToken(account.Id, restaurant.Id);
            
            await _context.UserRefreshTokens.AddAsync(userRefreshToken, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var token = _loginService.GenerateToken(account, claims);
            var response = new LoginResponseDto(token, userRefreshToken.Token);

            return Result<LoginResponseDto>.Success(response);
        }
    }
}

