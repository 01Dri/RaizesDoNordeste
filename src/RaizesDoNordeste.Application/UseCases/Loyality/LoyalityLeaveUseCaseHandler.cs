using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Loyality
{
    public class LoyalityLeaveUseCaseHandler : IUseCaseHandler<LoyalityLeaveRequestDto, LoyalityLeaveResponseDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ApplicationDbContext _context;

        public LoyalityLeaveUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<LoyalityLeaveResponseDto>> HandleAsync(LoyalityLeaveRequestDto parameter, CancellationToken cancellation = default)
        {
            long targetAccountId = parameter?.CustomerAccountId.HasValue == true && parameter.CustomerAccountId.Value > 0
                ? parameter.CustomerAccountId.Value
                : _currentUser.AccountId;

            if (targetAccountId != _currentUser.AccountId && !_currentUser.InRole(RoleType.Manager))
            {
                return Result<LoyalityLeaveResponseDto>.Failure(
                    new Error("Apenas o gerente pode remover outro cliente do programa de fidelidade.")
                );
            }

            var program = await _context.LoyalitPrograms
                .FirstOrDefaultAsync(x => x.AccountId == targetAccountId && x.RestaurantId == _currentUser.RestaurantId && x.Active && x.LeavedAt == null, cancellation);

            if (program == null)
            {
                return Result<LoyalityLeaveResponseDto>.Failure(
                    new Error("O cliente não faz parte do programa de fidelidade deste estabelecimento.")
                );
            }

            program.Active = false;
            program.LeavedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellation);

            return Result<LoyalityLeaveResponseDto>.Success(new LoyalityLeaveResponseDto(), System.Net.HttpStatusCode.OK);
        }
    }
}
