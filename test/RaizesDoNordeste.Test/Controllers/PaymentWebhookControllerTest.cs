using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RaizesDoNordeste.API.Controllers;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Payments;

namespace RaizesDoNordeste.Test.Controllers
{
    [TestFixture]
    public class PaymentWebhookControllerTest
    {
        private ApplicationDbContext _context;
        private PaymentWebhookController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _controller = new PaymentWebhookController(_context);
            
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task ReceiveNotification_ShouldUpdatePaymentToPaid_WhenOrderAndPaymentExistAndStatusIsApproved()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = 100, // Safe ID
                PublicId = orderId,
                Status = OrderStatus.Ready,
                Channel = OrderChannel.BALCAO,
                RestaurantId = Guid.NewGuid(),
                AccountId = 5,
                TotalPrice = 100.00m
            };
            
            var payment = new Payment
            {
                Id = 10,
                Total = 100.00m,
                TotalPaid = 0,
                PaymentMethod = PaymentMethod.Pix,
                Status = PaymentStatus.Waiting
            };

            var paymentOrder = new PaymentOrder
            {
                Order = order,
                Payment = payment
            };

            _context.Orders.Add(order);
            _context.Payments.Add(payment);
            _context.PaymentOrders.Add(paymentOrder);
            await _context.SaveChangesAsync();

            var payload = new PaymentWebhookController.WebhookPayload
            {
                TransactionId = "tx-pix-111",
                OrderId = orderId,
                Status = "Approved",
                Amount = 100.00m
            };

            // Act
            var result = await _controller.ReceiveNotification(payload);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            
            var updatedPayment = await _context.Payments.FindAsync(10L);
            Assert.Multiple(() =>
            {
                Assert.That(updatedPayment, Is.Not.Null);
                Assert.That(updatedPayment.Status, Is.EqualTo(PaymentStatus.Paid));
                Assert.That(updatedPayment.TotalPaid, Is.EqualTo(100.00m));
                Assert.That(updatedPayment.Description, Contains.Substring("tx-pix-111"));
            });
        }

        [Test]
        public async Task ReceiveNotification_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var payload = new PaymentWebhookController.WebhookPayload
            {
                TransactionId = "tx-pix-111",
                OrderId = Guid.NewGuid(),
                Status = "Approved",
                Amount = 100.00m
            };

            // Act
            var result = await _controller.ReceiveNotification(payload);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task ReceiveNotification_ShouldReturnBadRequest_WhenPaymentDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = 200, // Safe ID
                PublicId = orderId,
                Status = OrderStatus.Ready,
                Channel = OrderChannel.BALCAO,
                RestaurantId = Guid.NewGuid(),
                AccountId = 5,
                TotalPrice = 100.00m
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var payload = new PaymentWebhookController.WebhookPayload
            {
                TransactionId = "tx-pix-111",
                OrderId = orderId,
                Status = "Approved",
                Amount = 100.00m
            };

            // Act
            var result = await _controller.ReceiveNotification(payload);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Não foi encontrado um registro de pagamento para este pedido."));
        }

        [Test]
        public async Task ReceiveNotification_ShouldReturnBadRequest_WhenStatusIsNotApproved()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = 300, // Safe ID
                PublicId = orderId,
                Status = OrderStatus.Ready,
                Channel = OrderChannel.BALCAO,
                RestaurantId = Guid.NewGuid(),
                AccountId = 5,
                TotalPrice = 100.00m
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var payload = new PaymentWebhookController.WebhookPayload
            {
                TransactionId = "tx-pix-111",
                OrderId = orderId,
                Status = "Rejected",
                Amount = 100.00m
            };

            // Act
            var result = await _controller.ReceiveNotification(payload);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }
    }
}
