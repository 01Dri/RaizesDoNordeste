using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Payments;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Domain.Services
{
    public record RegisterPaymentResult(Payment Payment, int EarnedLoyaltyPoints, int? TotalPointsInRestaurant);

    public interface IPaymentTransactionService
    {
        Task<RegisterPaymentResult> RegisterPaymentAsync(
            Order order,
            PaymentMethod paymentMethod,
            PaymentStatus status,
            decimal totalToPay,
            string? externalPaymentId,
            bool usedLoyaltyPoints,
            string description,
            CancellationToken cancellationToken = default
        );

        Task<bool> ConfirmPaymentAsync(
            Payment payment,
            long accountId,
            Guid restaurantId,
            decimal amountPaid,
            string externalPaymentId,
            string description,
            CancellationToken cancellationToken = default
        );
    }
}
