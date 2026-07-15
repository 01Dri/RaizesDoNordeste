using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain;
using RaizesDoNordeste.Domain.Core.Login;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Login
{
    public sealed class LogoutUseCaseHandler : IUseCaseHandler<LogoutRequestDto, LogoutResponseDto>
    {
        private readonly ApplicationDbContext _context;

        public LogoutUseCaseHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<LogoutResponseDto>> HandleAsync(LogoutRequestDto parameter, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(parameter.RefreshToken))
            {
                return Result<LogoutResponseDto>.Failure(new Error("Refresh token é obrigatório."), HttpStatusCode.BadRequest);
            }

            var token = await _context.UserRefreshTokens
                .FirstOrDefaultAsync(t => t.Token == parameter.RefreshToken, cancellation);

            if (token == null)
            {
                return Result<LogoutResponseDto>.Failure(new Error("Refresh token não encontrado."), HttpStatusCode.NotFound);
            }

            token.Revoked = true;
            token.Active = false;
            token.UpdatedAt = Calendar.Now;

            await _context.SaveChangesAsync(cancellation);

            return Result<LogoutResponseDto>.Success(new LogoutResponseDto { Success = true });
        }
    }
}
