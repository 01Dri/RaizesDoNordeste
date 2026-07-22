using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoyalityProgramService _loyalityProgramService;

        public PaymentTransactionService(ApplicationDbContext dbContext, ILoyalityProgramService loyalityProgramService)
        {
            _dbContext = dbContext;
            _loyalityProgramService = loyalityProgramService;
        }

        public async Task<RegisterPaymentResult> RegisterPaymentAsync(
            Order order,
            PaymentMethod paymentMethod,
            PaymentStatus status,
            decimal totalToPay,
            string? externalPaymentId,
            bool usedLoyaltyPoints,
            string description,
            CancellationToken cancellationToken = default)
        {
            var payment = new Payment
            {
                Total = order.TotalPrice,
                TotalDiscount = order.TotalPrice - totalToPay,
                TotalPaid = status == PaymentStatus.Paid ? totalToPay : 0,
                PaymentMethod = paymentMethod,
                Status = status,
                ExternalPaymentId = externalPaymentId,
                Description = description
            };

            _dbContext.Payments.Add(payment);

            var paymentOrder = new PaymentOrder
            {
                Order = order,
                Payment = payment,
                UsedLoyalityPoints = usedLoyaltyPoints
            };
            _dbContext.PaymentOrders.Add(paymentOrder);

            int earnedPoints = 0;
            int? totalPointsInRestaurant = null;

            if (status == PaymentStatus.Paid)
            {
                var earnResult = await _loyalityProgramService.EarnPointsAsync(
                    totalToPay,
                    order.AccountId.GetValueOrDefault(),
                    order.RestaurantId,
                    cancellationToken
                );

                earnedPoints = earnResult.PointsAmount;
                totalPointsInRestaurant = earnResult.TotalPointsInRestaurant;
            }
            else
            {
                totalPointsInRestaurant = await _loyalityProgramService.GetUserPointsAsync(
                    order.AccountId.GetValueOrDefault(),
                    order.RestaurantId,
                    cancellationToken
                );
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RegisterPaymentResult(payment, earnedPoints, totalPointsInRestaurant);
        }

        public async Task<bool> ConfirmPaymentAsync(
            Payment payment,
            long accountId,
            Guid restaurantId,
            decimal amountPaid,
            string externalPaymentId,
            string description,
            CancellationToken cancellationToken = default)
        {
            payment.Status = PaymentStatus.Paid;
            payment.TotalPaid = amountPaid;
            payment.ExternalPaymentId = externalPaymentId;
            payment.Description = description;

            await _loyalityProgramService.EarnPointsAsync(
                amountPaid,
                accountId,
                restaurantId,
                cancellationToken
            );

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
