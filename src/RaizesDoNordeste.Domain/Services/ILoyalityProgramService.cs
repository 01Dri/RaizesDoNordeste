using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Domain.Services
{
    public interface ILoyalityProgramService
    {
        Task<decimal> ApplyDiscountAsync(decimal orderValue, long accountId, Guid restaurantId, CancellationToken cancellationToken = default);
        Task<int> EarnPointsAsync(decimal amountPaid, long accountId, Guid restaurantId, CancellationToken cancellationToken = default);
    }
}
