using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Loyalit;
using RaizesDoNordeste.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.Services
{
    public class LoyalityProgramService : ILoyalityProgramService
    {
        private readonly ApplicationDbContext _dbContext;
        private const decimal PointsToCashRatio = 10.0m; // 10 pontos = R$ 1.00
        private const decimal CashToPointsEarningRatio = 1.0m; // R$ 1.00 gasto = 1 ponto ganho

        public LoyalityProgramService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApplyDiscountResult> ApplyDiscountAsync(decimal orderValue, long accountId, Guid restaurantId, CancellationToken cancellationToken = default)
        {
            if (orderValue <= 0)
            {
                return new ApplyDiscountResult(false, 0m);
            }

            var program = await _dbContext.LoyalitPrograms
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.RestaurantId == restaurantId && x.Active, cancellationToken);

            if (program == null || program.Points <= 0)
            {
                return new ApplyDiscountResult(false, 0m);
            }

            decimal discountFromPoints = program.Points / PointsToCashRatio;
            decimal discountApplied = Math.Min(discountFromPoints, orderValue);

            if (discountApplied > 0)
            {
                int pointsToConsume = (int)(discountApplied * PointsToCashRatio);

                program.Points -= pointsToConsume;

                var movement = new LoyalitProgramMovements
                {
                    Type = LoyalitProgramMovementType.Consume,
                    Points = pointsToConsume,
                    LoyalityProgramId = program.Id.GetValueOrDefault(),
                    LoyalitProgram = program,
                    MovementAt = DateTime.UtcNow
                };

                await _dbContext.LoyalitProgramMovements.AddAsync(movement, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new ApplyDiscountResult(true, discountApplied);
            }

            return new ApplyDiscountResult(false, 0m);
        }

        public async Task<EarnPointsResult> EarnPointsAsync(decimal amountPaid, long accountId, Guid restaurantId, CancellationToken cancellationToken = default)
        {
            var program = await _dbContext.LoyalitPrograms
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.RestaurantId == restaurantId && x.Active, cancellationToken);

            if (program == null)
            {
                return new EarnPointsResult(false, 0, null);
            }

            if (amountPaid <= 0)
            {
                return new EarnPointsResult(false, 0, program.Points);
            }

            int pointsEarned = (int)Math.Floor(amountPaid * CashToPointsEarningRatio);

            if (pointsEarned > 0)
            {
                program.Points += pointsEarned;

                var movement = new LoyalitProgramMovements
                {
                    Type = LoyalitProgramMovementType.Earn,
                    Points = pointsEarned,
                    LoyalityProgramId = program.Id.GetValueOrDefault(),
                    LoyalitProgram = program,
                    MovementAt = DateTime.UtcNow
                };

                await _dbContext.LoyalitProgramMovements.AddAsync(movement, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new EarnPointsResult(true, pointsEarned, program.Points);
            }

            return new EarnPointsResult(false, 0, program.Points);
        }

        public async Task<int?> GetUserPointsAsync(long accountId, Guid restaurantId, CancellationToken cancellationToken = default)
        {
            var program = await _dbContext.LoyalitPrograms
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.RestaurantId == restaurantId && x.Active, cancellationToken);

            return program?.Points;
        }
    }
}
