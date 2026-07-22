using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Domain.Services
{
    public record ApplyDiscountResult(bool PointsConsumed, decimal DiscountAmount);
    public record EarnPointsResult(bool PointsEarned, int PointsAmount, int? TotalPointsInRestaurant);

    public interface ILoyalityProgramService
    {
        Task<ApplyDiscountResult> ApplyDiscountAsync(decimal orderValue, long accountId, Guid restaurantId, CancellationToken cancellationToken = default);
        Task<EarnPointsResult> EarnPointsAsync(decimal amountPaid, long accountId, Guid restaurantId, CancellationToken cancellationToken = default);
        Task<int?> GetUserPointsAsync(long accountId, Guid restaurantId, CancellationToken cancellationToken = default);
    }
}
