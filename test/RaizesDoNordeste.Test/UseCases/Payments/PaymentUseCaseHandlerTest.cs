using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Payments;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Core.Payments.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using UninterPayment.SDK;

namespace RaizesDoNordeste.Test.UseCases.Payments
{
    [TestFixture]
    public class PaymentUseCaseHandlerTest
    {
        private ApplicationDbContext _context;
        private Mock<IValidator<PaymentRequestDto>> _validatorMock;
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<IUninterPaymentClient> _paymentClientMock;
        private PaymentUseCaseHandler _handler;
        private readonly long _accountId = 5;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _validatorMock = new Mock<IValidator<PaymentRequestDto>>();
            _currentUserMock = new Mock<ICurrentUser>();
            _paymentClientMock = new Mock<IUninterPaymentClient>();

            _currentUserMock.Setup(x => x.AccountId).Returns(_accountId);

            _validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<PaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _handler = new PaymentUseCaseHandler(
                _context,
                _validatorMock.Object,
                _currentUserMock.Object,
                _paymentClientMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private Order CreateTestOrder(long id, Guid orderId, decimal totalPrice, OrderStatus status = OrderStatus.Ready)
        {
            var order = new Order
            {
                Id = id,
                PublicId = orderId,
                Status = status,
                Channel = OrderChannel.BALCAO,
                RestaurantId = Guid.NewGuid(),
                AccountId = _accountId,
                TotalPrice = totalPrice
            };

            var menuItem = new MenuItem
            {
                Id = id,
                Title = "Test Item",
                Price = totalPrice,
                MenuId = 1 // Set valid MenuId
            };

            var orderItem = new OrderItem
            {
                Order = order,
                MenuItem = menuItem,
                Quantity = 1
            };

            order.Items = new List<OrderItem> { orderItem };
            return order;
        }

        [Test]
        public async Task HandleAsync_ShouldReturnSuccessApproved_WhenCreditCardPaymentSucceeds()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateTestOrder(100, orderId, 50.00m);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var dto = new PaymentRequestDto
            {
                OrderId = orderId,
                PaymentMethod = new PaymentMethodDto { Method = PaymentMethod.Credit }
            };

            _paymentClientMock
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<UninterPayment.SDK.PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResult
                {
                    TransactionId = "tx-123",
                    Status = UninterPaymentStatus.Approved
                });

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Status, Is.EqualTo(PaymentStatus.Paid));
                Assert.That(result.Data.AmountPaid, Is.EqualTo(50.00m));
            });

            var dbPayment = await _context.Payments.FirstOrDefaultAsync();
            Assert.Multiple(() =>
            {
                Assert.That(dbPayment, Is.Not.Null);
                Assert.That(dbPayment.Status, Is.EqualTo(PaymentStatus.Paid));
                Assert.That(dbPayment.TotalPaid, Is.EqualTo(50.00m));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnSuccessWaiting_WhenPixPaymentIsPending()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateTestOrder(200, orderId, 120.00m);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var dto = new PaymentRequestDto
            {
                OrderId = orderId,
                PaymentMethod = new PaymentMethodDto { Method = PaymentMethod.Pix }
            };

            _paymentClientMock
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<UninterPayment.SDK.PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResult
                {
                    TransactionId = "tx-pix-999",
                    Status = UninterPaymentStatus.Waiting
                });

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Status, Is.EqualTo(PaymentStatus.Waiting));
                Assert.That(result.Data.AmountPaid, Is.EqualTo(0m));
            });

            var dbPayment = await _context.Payments.FirstOrDefaultAsync();
            Assert.Multiple(() =>
            {
                Assert.That(dbPayment, Is.Not.Null);
                Assert.That(dbPayment.Status, Is.EqualTo(PaymentStatus.Waiting));
                Assert.That(dbPayment.TotalPaid, Is.EqualTo(0m));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenOrderNotFound()
        {
            // Arrange
            var dto = new PaymentRequestDto
            {
                OrderId = Guid.NewGuid(),
                PaymentMethod = new PaymentMethodDto { Method = PaymentMethod.Credit }
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenOrderStatusIsNotReady()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateTestOrder(300, orderId, 30.00m, OrderStatus.Process);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var dto = new PaymentRequestDto
            {
                OrderId = orderId,
                PaymentMethod = new PaymentMethodDto { Method = PaymentMethod.Credit }
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData.Message, Contains.Substring("O pedido precisa estar pronto"));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenSdkFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = CreateTestOrder(400, orderId, 80.00m);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var dto = new PaymentRequestDto
            {
                OrderId = orderId,
                PaymentMethod = new PaymentMethodDto { Method = PaymentMethod.Credit }
            };

            _paymentClientMock
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<UninterPayment.SDK.PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResult
                {
                    Status = UninterPaymentStatus.Failed,
                    ErrorMessage = "Cartão recusado pelo banco emissor."
                });

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData.Message, Is.EqualTo("Cartão recusado pelo banco emissor."));
            });
        }
    }
}
