using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Orders;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Test.UseCases.Orders
{
    [TestFixture]
    public class OrderQueryUseCaseTests
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private readonly Guid _userRestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");

        private GetOrderByIdUseCaseHandler _getByIdHandler;
        private ListOrdersUseCaseHandler _listHandler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            // Seed Account with Id = 1
            var account = new RaizesDoNordeste.Domain.Core.Accounts.Account
            {
                Id = 1L,
                Email = new Email("test@raizesdonordeste.com"),
                Password = "testpassword",
                Active = true
            };
            _context.Accounts.Add(account);
            _context.SaveChanges();

            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.RestaurantId).Returns(_userRestaurantId);

            _getByIdHandler = new GetOrderByIdUseCaseHandler(_context, _currentUserMock.Object);
            _listHandler = new ListOrdersUseCaseHandler(_context, _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetById_ShouldReturnOrderDetails_WhenExistsAndOwner()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = 500,
                PublicId = orderId,
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.TOTEM,
                RestaurantId = _userRestaurantId,
                AccountId = 1L,
                TotalPrice = 45.00m,
                Active = true
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _getByIdHandler.HandleAsync(new GetOrderByIdQueryDto(orderId), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Id, Is.EqualTo(orderId));
                Assert.That(result.Data.Status, Is.EqualTo(OrderStatus.Delivered));
                Assert.That(result.Data.TotalPrice, Is.EqualTo(45.00m));
            });
        }

        [Test]
        public async Task GetById_ShouldReturnForbidden_WhenOrderBelongsToOtherRestaurant()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = 501,
                PublicId = orderId,
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.TOTEM,
                RestaurantId = Guid.NewGuid(), // Different restaurant
                AccountId = 1L,
                TotalPrice = 45.00m,
                Active = true
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _getByIdHandler.HandleAsync(new GetOrderByIdQueryDto(orderId), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
            });
        }

        [Test]
        public async Task List_ShouldOnlyReturnRestaurantOrders()
        {
            // Arrange
            var order1 = new Order
            {
                Id = 600,
                PublicId = Guid.NewGuid(),
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.TOTEM,
                RestaurantId = _userRestaurantId,
                AccountId = 1L,
                TotalPrice = 30.00m,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            var order2 = new Order
            {
                Id = 601,
                PublicId = Guid.NewGuid(),
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.TOTEM,
                RestaurantId = Guid.NewGuid(), // Other restaurant
                AccountId = 1L,
                TotalPrice = 40.00m,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.AddRange(order1, order2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _listHandler.HandleAsync(new ListOrdersQueryDto(null), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Orders.Any(o => o.Id == order1.PublicId), Is.True);
                Assert.That(result.Data.Orders.Any(o => o.Id == order2.PublicId), Is.False);
            });
        }

        [Test]
        public async Task List_ShouldFilterByStatus_WhenSpecified()
        {
            // Arrange
            var order1 = new Order
            {
                Id = 610,
                PublicId = Guid.NewGuid(),
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.TOTEM,
                RestaurantId = _userRestaurantId,
                AccountId = 1L,
                TotalPrice = 30.00m,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            var order2 = new Order
            {
                Id = 611,
                PublicId = Guid.NewGuid(),
                Status = OrderStatus.Process, // Different status
                Channel = OrderChannel.TOTEM,
                RestaurantId = _userRestaurantId,
                AccountId = 1L,
                TotalPrice = 40.00m,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.AddRange(order1, order2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _listHandler.HandleAsync(new ListOrdersQueryDto(OrderStatus.Delivered), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Orders.Count, Is.EqualTo(1));
                Assert.That(result.Data.Orders[0].Id, Is.EqualTo(order1.PublicId));
            });
        }

        [Test]
        public async Task List_ShouldFilterByChannel_WhenSpecified()
        {
            // Arrange
            var order1 = new Order
            {
                Id = 620,
                PublicId = Guid.NewGuid(),
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.BALCAO,
                RestaurantId = _userRestaurantId,
                AccountId = 1L,
                TotalPrice = 30.00m,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            var order2 = new Order
            {
                Id = 621,
                PublicId = Guid.NewGuid(),
                Status = OrderStatus.Delivered,
                Channel = OrderChannel.TOTEM,
                RestaurantId = _userRestaurantId,
                AccountId = 1L,
                TotalPrice = 40.00m,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.AddRange(order1, order2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _listHandler.HandleAsync(new ListOrdersQueryDto(null, OrderChannel.BALCAO), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Orders.Count, Is.EqualTo(1));
                Assert.That(result.Data.Orders[0].Id, Is.EqualTo(order1.PublicId));
            });
        }
    }
}
