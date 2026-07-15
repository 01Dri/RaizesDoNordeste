using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Accounts.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Accounts
{
    public sealed class GetUserProfileUseCase : IUseCaseHandler<UserProfileResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUser _currentUser;

        public GetUserProfileUseCase(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<UserProfileResponseDto>> HandleAsync(CancellationToken cancellation = default)
        {
            var accountId = _currentUser.AccountId;

            var account = await _context.Accounts
                .Include(a => a.RoleAccounts)
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellation);

            if (account == null)
            {
                return Result<UserProfileResponseDto>.FailureNotFound("Usuário não encontrado.");
            }

            var response = new UserProfileResponseDto
            {
                Id = account.Id,
                Email = account.Email.Value,
                Roles = account.RoleAccounts
                    .Where(x => x.RoleId.HasValue)
                    .Select(x => x.RoleId!.Value.ToString())
                    .ToList(),
                CreatedAt = account.CreatedAt,
                Active = account.Active
            };

            return Result<UserProfileResponseDto>.Success(response);
        }
    }
}
