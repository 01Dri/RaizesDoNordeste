using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Loyality;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Loyalit;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using System;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Test.UseCases.Loyality
{
    [TestFixture]
    public class LoyalityLeaveUseCaseHandlerTest
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private LoyalityLeaveUseCaseHandler _handler;
        private readonly Guid _restaurantId = Guid.NewGuid();
        private readonly long _customerAccountId = 10;
        private readonly long _managerAccountId = 20;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.AccountId).Returns(_customerAccountId);
            _currentUserMock.Setup(x => x.RestaurantId).Returns(_restaurantId);
            _currentUserMock.Setup(x => x.InRole(RoleType.Manager)).Returns(false);

            _handler = new LoyalityLeaveUseCaseHandler(_context, _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ShouldAllowCustomerToLeave_WhenLeavingTheirOwnProgram()
        {
            // Arrange
            var program = new LoyalitProgram
            {
                AccountId = _customerAccountId,
                RestaurantId = _restaurantId,
                JoinedAt = DateTime.UtcNow,
                Active = true
            };
            await _context.LoyalitPrograms.AddAsync(program);
            await _context.SaveChangesAsync();

            var request = new LoyalityLeaveRequestDto { CustomerAccountId = null };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(program.Active, Is.False);
                Assert.That(program.LeavedAt, Is.Not.Null);
            });
        }

        [Test]
        public async Task HandleAsync_ShouldAllowManagerToRemove_AnotherCustomerFromProgram()
        {
            // Arrange
            _currentUserMock.Setup(x => x.AccountId).Returns(_managerAccountId);
            _currentUserMock.Setup(x => x.InRole(RoleType.Manager)).Returns(true);

            var program = new LoyalitProgram
            {
                AccountId = _customerAccountId,
                RestaurantId = _restaurantId,
                JoinedAt = DateTime.UtcNow,
                Active = true
            };
            await _context.LoyalitPrograms.AddAsync(program);
            await _context.SaveChangesAsync();

            var request = new LoyalityLeaveRequestDto { CustomerAccountId = _customerAccountId };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(program.Active, Is.False);
                Assert.That(program.LeavedAt, Is.Not.Null);
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenNonManagerTriesToRemoveAnotherCustomer()
        {
            // Arrange
            _currentUserMock.Setup(x => x.AccountId).Returns(_customerAccountId);
            _currentUserMock.Setup(x => x.InRole(RoleType.Manager)).Returns(false);

            long otherCustomerAccountId = 99;
            var request = new LoyalityLeaveRequestDto { CustomerAccountId = otherCustomerAccountId };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.Error.Message, Contains.Substring("gerente"));
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenCustomerIsNotInProgram()
        {
            // Arrange
            var request = new LoyalityLeaveRequestDto { CustomerAccountId = null };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailure, Is.True);
                Assert.That(result.Error.Message, Contains.Substring("não faz parte do programa"));
            });
        }
    }
}
