using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Loyalit;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Loyality
{
    public class LoyalityJoinUseCaseHandler : IUseCaseHandler<LoyalityJoinRequestDto, LoyalityJoinResponseDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ApplicationDbContext _context;

        public LoyalityJoinUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<LoyalityJoinResponseDto>> HandleAsync(LoyalityJoinRequestDto parameter, CancellationToken cancellation = default)
        {
            if (!_currentUser.InRole(RoleType.Manager))
            {
                return Result<LoyalityJoinResponseDto>.Failure(new Error("Apenas o gerente do restaurante pode adicionar clientes ao programa de fidelidade."));
            }

            var accountExists = await _context.Accounts
                .AnyAsync(x => x.Id == parameter.CustomerAccountId, cancellation);

            if (!accountExists)
            {
                return Result<LoyalityJoinResponseDto>.Failure(new Error("Cliente não encontrado."));
            }

            var existingProgram = await _context.LoyalitPrograms
                .FirstOrDefaultAsync(x => x.AccountId == parameter.CustomerAccountId && x.RestaurantId == _currentUser.RestaurantId, cancellation);

            if (existingProgram != null && existingProgram.Active && existingProgram.LeavedAt == null)
            {
                return Result<LoyalityJoinResponseDto>.Failure(new Error("O cliente já está no programa de fidelidade."));
            }

            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var orderCount = await _context.Orders
                .CountAsync(x => x.AccountId == parameter.CustomerAccountId && x.RestaurantId == _currentUser.RestaurantId && x.CreatedAt >= oneMonthAgo, cancellation);

            if (orderCount < 3)
            {
                return Result<LoyalityJoinResponseDto>.Failure(new Error("O cliente precisa ter realizado pelo menos 3 pedidos no último mês para entrar no programa de fidelidade."));
            }

            if (existingProgram != null)
            {
                existingProgram.Active = true;
                existingProgram.LeavedAt = null;
                existingProgram.JoinedAt = DateTime.UtcNow;
            }
            else
            {
                var program = new LoyalitProgram
                {
                    AccountId = parameter.CustomerAccountId,
                    RestaurantId = _currentUser.RestaurantId,
                    JoinedAt = DateTime.UtcNow,
                    Active = true,
                    LeavedAt = null
                };
                await _context.AddAsync(program, cancellation);
            }

            await _context.SaveChangesAsync(cancellation);

            return Result<LoyalityJoinResponseDto>.Success(new LoyalityJoinResponseDto(), System.Net.HttpStatusCode.Created);
        }
    }
}
