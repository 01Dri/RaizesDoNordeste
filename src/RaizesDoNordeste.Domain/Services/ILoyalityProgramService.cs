namespace RaizesDoNordeste.Domain.Services
{
    public record ApplyDiscountResult(bool PointsConsumed, decimal DiscountAmount);
    public record EarnPointsResult(bool PointsEarned, int PointsAmount, int? TotalPointsInRestaurant);

    public record UseLoyalityProgramRequest(decimal OrderValue, long AccountId, Guid RestaurantId, int PointsToUse);

    public interface ILoyalityProgramService
    {
        Task<ApplyDiscountResult> ApplyDiscountAsync(UseLoyalityProgramRequest request, CancellationToken cancellationToken = default);
        Task<EarnPointsResult> EarnPointsAsync(decimal amountPaid, long accountId, Guid restaurantId, CancellationToken cancellationToken = default);
        Task<int?> GetUserPointsAsync(long accountId, Guid restaurantId, CancellationToken cancellationToken = default);
    }
}
