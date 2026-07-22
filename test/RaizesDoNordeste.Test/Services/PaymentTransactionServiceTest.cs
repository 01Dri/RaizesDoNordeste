using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.Services;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Test.Services
{
    [TestFixture]
    public class PaymentTransactionServiceTest
    {
        private ApplicationDbContext _context;
        private Mock<ILoyalityProgramService> _loyalityProgramServiceMock;
        private PaymentTransactionService _service;
        private readonly long _accountId = 10;
        private readonly Guid _restaurantId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _loyalityProgramServiceMock = new Mock<ILoyalityProgramService>();
            _service = new PaymentTransactionService(_context, _loyalityProgramServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task RegisterPaymentAsync_ShouldCreatePaymentAndPaymentOrder_AndEarnPointsWhenPaid()
        {
            // Arrange
            var order = new Order
            {
                PublicId = Guid.NewGuid(),
                AccountId = _accountId,
                RestaurantId = _restaurantId,
                TotalPrice = 100.00m,
                Status = OrderStatus.Ready
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            _loyalityProgramServiceMock
                .Setup(x => x.EarnPointsAsync(100.00m, _accountId, _restaurantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EarnPointsResult(true, 100, 100));

            // Act
            var result = await _service.RegisterPaymentAsync(
                order,
                PaymentMethod.CreditCard,
                PaymentStatus.Paid,
                100.00m,
                "tx-123",
                false,
                "Aprovado via cartão."
            );

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Payment, Is.Not.Null);
                Assert.That(result.Payment.Status, Is.EqualTo(PaymentStatus.Paid));
                Assert.That(result.Payment.ExternalPaymentId, Is.EqualTo("tx-123"));
            });

            var paymentOrder = await _context.PaymentOrders.FirstOrDefaultAsync(po => po.OrderId == order.Id);
            Assert.That(paymentOrder, Is.Not.Null);
        }

        [Test]
        public async Task ConfirmPaymentAsync_ShouldUpdatePaymentStatusAndEarnPoints()
        {
            // Arrange
            var payment = new Payment
            {
                Total = 50.0m,
                TotalPaid = 0m,
                Status = PaymentStatus.Waiting,
                PaymentMethod = PaymentMethod.Pix
            };
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            _loyalityProgramServiceMock
                .Setup(x => x.EarnPointsAsync(50.0m, _accountId, _restaurantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EarnPointsResult(true, 50, 50));

            // Act
            var success = await _service.ConfirmPaymentAsync(
                payment,
                _accountId,
                _restaurantId,
                50.0m,
                "tx-pix-999",
                "Pagamento Pix aprovado via webhook."
            );

            // Assert
            Assert.That(success, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Paid));
                Assert.That(payment.TotalPaid, Is.EqualTo(50.0m));
                Assert.That(payment.ExternalPaymentId, Is.EqualTo("tx-pix-999"));
            });
        }
    }
}
