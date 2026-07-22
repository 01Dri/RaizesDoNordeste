using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Loyality;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Loyalit;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.ValuesObjects;
using System;
using System.Threading.Tasks;
using AccountEntity = RaizesDoNordeste.Domain.Core.Accounts.Account;
using RoleType = RaizesDoNordeste.Domain.Core.Accounts.Roles.RoleType;

namespace RaizesDoNordeste.Test.UseCases.Loyality
{
    [TestFixture]
    public class LoyalityJoinUseCaseHandlerTest
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private LoyalityJoinUseCaseHandler _handler;
        private readonly Guid _restaurantId = Guid.NewGuid();
        private readonly long _managerAccountId = 1;
        private readonly long _customerAccountId = 2;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.AccountId).Returns(_managerAccountId);
            _currentUserMock.Setup(x => x.RestaurantId).Returns(_restaurantId);
            _currentUserMock.Setup(x => x.InRole(RoleType.Manager)).Returns(true);

            _handler = new LoyalityJoinUseCaseHandler(_context, _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenCurrentUserIsNotManager()
        {
            // Arrange
            _currentUserMock.Setup(x => x.InRole(RoleType.Manager)).Returns(false);
            var request = new LoyalityJoinRequestDto { CustomerAccountId = _customerAccountId };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData?.Message, Contains.Substring("gerente"));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenCustomerAccountDoesNotExist()
        {
            // Arrange
            var request = new LoyalityJoinRequestDto { CustomerAccountId = 999 };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData?.Message, Contains.Substring("Cliente não encontrado"));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenCustomerAlreadyJoined()
        {
            // Arrange
            var account = new AccountEntity { Id = _customerAccountId, Email = new Email("client1@test.com"), Password = "pass" };
            await _context.Accounts.AddAsync(account);

            var program = new LoyalitProgram
            {
                AccountId = _customerAccountId,
                RestaurantId = _restaurantId,
                JoinedAt = DateTime.UtcNow,
                Active = true
            };
            await _context.LoyalitPrograms.AddAsync(program);
            await _context.SaveChangesAsync();

            var request = new LoyalityJoinRequestDto { CustomerAccountId = _customerAccountId };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData?.Message, Contains.Substring("já está no programa"));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenCustomerHasLessThan3OrdersInLastMonth()
        {
            // Arrange
            var account = new AccountEntity { Id = _customerAccountId, Email = new Email("client2@test.com"), Password = "pass" };
            await _context.Accounts.AddAsync(account);

            // Only 2 orders in last month
            for (int i = 0; i < 2; i++)
            {
                var order = new Order
                {
                    PublicId = Guid.NewGuid(),
                    AccountId = _customerAccountId,
                    RestaurantId = _restaurantId,
                    TotalPrice = 30.0m,
                    Status = OrderStatus.Delivered,
                    Channel = OrderChannel.BALCAO,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                };
                await _context.Orders.AddAsync(order);
            }
            await _context.SaveChangesAsync();

            var request = new LoyalityJoinRequestDto { CustomerAccountId = _customerAccountId };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData?.Message, Contains.Substring("pelo menos 3 pedidos"));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldSuccessfullyJoinCustomer_WhenUserIsManagerAndCustomerHas3OrMoreOrdersInLastMonth()
        {
            // Arrange
            var account = new AccountEntity { Id = _customerAccountId, Email = new Email("client3@test.com"), Password = "pass" };
            await _context.Accounts.AddAsync(account);

            // 3 orders in last month
            for (int i = 0; i < 3; i++)
            {
                var order = new Order
                {
                    PublicId = Guid.NewGuid(),
                    AccountId = _customerAccountId,
                    RestaurantId = _restaurantId,
                    TotalPrice = 30.0m,
                    Status = OrderStatus.Delivered,
                    Channel = OrderChannel.BALCAO,
                    CreatedAt = DateTime.UtcNow.AddDays(-5 * (i + 1))
                };
                await _context.Orders.AddAsync(order);
            }
            await _context.SaveChangesAsync();

            var request = new LoyalityJoinRequestDto { CustomerAccountId = _customerAccountId };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
            });

            var program = await _context.LoyalitPrograms.FirstOrDefaultAsync(x => x.AccountId == _customerAccountId && x.RestaurantId == _restaurantId);
            Assert.That(program, Is.Not.Null);
        }
    }
}
