using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Loyalit;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Loyality
{
    public class LoyalityJoinUseCaseHandler : IUseCaseHandler<LoyalityJoinResponseDto>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ApplicationDbContext _context;

        public LoyalityJoinUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<LoyalityJoinResponseDto>> HandleAsync(CancellationToken cancellation = default)
        {
            var accountId = _currentUser.AccountId;
            var restaurantId = _currentUser.RestaurantId;

            var alreadyJoined = await _context.LoyalitPrograms
                .AnyAsync(x => x.AccountId == accountId && x.RestaurantId == restaurantId);

            if (alreadyJoined)
            {
                return Result<LoyalityJoinResponseDto>.Failure
                (
                  new Error("O usuário já está no programa de fidelidade.")
                );
            }
            var program = new LoyalitProgram()
            {
                AccountId = accountId,
                RestaurantId = restaurantId,
                JoinedAt = DateTime.UtcNow,
            };

            await _context.AddAsync(program, cancellation);
            await _context.SaveChangesAsync();
            return Result<LoyalityJoinResponseDto>.Success(new LoyalityJoinResponseDto(), System.Net.HttpStatusCode.Created);
        }
    }
}
