using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RaizesDoNordeste.Application.Services;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Loyalit;
using System;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Test.Services
{
    [TestFixture]
    public class LoyalityProgramServiceTest
    {
        private ApplicationDbContext _context;
        private LoyalityProgramService _service;
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

            _service = new LoyalityProgramService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task ApplyDiscountAsync_ShouldReturnOriginalValue_WhenLoyaltyProgramDoesNotExist()
        {
            // Arrange
            decimal orderValue = 50.0m;

            // Act
            decimal finalValue = await _service.ApplyDiscountAsync(orderValue, _accountId, _restaurantId);

            // Assert
            Assert.That(finalValue, Is.EqualTo(orderValue));
        }

        [Test]
        public async Task ApplyDiscountAsync_ShouldReturnOriginalValue_WhenLoyaltyProgramHasNoPoints()
        {
            // Arrange
            var program = new LoyalitProgram
            {
                AccountId = _accountId,
                RestaurantId = _restaurantId,
                Points = 0,
                Active = true,
                JoinedAt = DateTime.UtcNow
            };
            await _context.LoyalitPrograms.AddAsync(program);
            await _context.SaveChangesAsync();

            decimal orderValue = 50.0m;

            // Act
            decimal finalValue = await _service.ApplyDiscountAsync(orderValue, _accountId, _restaurantId);

            // Assert
            Assert.That(finalValue, Is.EqualTo(orderValue));
        }

        [Test]
        public async Task ApplyDiscountAsync_ShouldApplyPartialDiscount_WhenPointsValueIsLessThanOrderValue()
        {
            // Arrange
            // 200 points = R$ 20.00 discount
            var program = new LoyalitProgram
            {
                AccountId = _accountId,
                RestaurantId = _restaurantId,
                Points = 200,
                Active = true,
                JoinedAt = DateTime.UtcNow
            };
            await _context.LoyalitPrograms.AddAsync(program);
            await _context.SaveChangesAsync();

            decimal orderValue = 50.0m;

            // Act
            decimal finalValue = await _service.ApplyDiscountAsync(orderValue, _accountId, _restaurantId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(finalValue, Is.EqualTo(30.0m)); // 50 - 20 = 30
                Assert.That(program.Points, Is.EqualTo(0)); // All points consumed
            });

            var movement = await _context.LoyalitProgramMovements.FirstOrDefaultAsync();
            Assert.Multiple(() =>
            {
                Assert.That(movement, Is.Not.Null);
                Assert.That(movement.Type, Is.EqualTo(LoyalitProgramMovementType.Consume));
                Assert.That(movement.Points, Is.EqualTo(200));
            });
        }

        [Test]
        public async Task ApplyDiscountAsync_ShouldApplyFullDiscount_WhenPointsValueIsGreaterOrEqualToOrderValue()
        {
            // Arrange
            // 600 points = R$ 60.00 discount
            var program = new LoyalitProgram
            {
                AccountId = _accountId,
                RestaurantId = _restaurantId,
                Points = 600,
                Active = true,
                JoinedAt = DateTime.UtcNow
            };
            await _context.LoyalitPrograms.AddAsync(program);
            await _context.SaveChangesAsync();

            decimal orderValue = 50.0m;

            // Act
            decimal finalValue = await _service.ApplyDiscountAsync(orderValue, _accountId, _restaurantId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(finalValue, Is.EqualTo(0.0m)); // Fully covered
                Assert.That(program.Points, Is.EqualTo(100)); // 600 - 500 consumed = 100 left
            });

            var movement = await _context.LoyalitProgramMovements.FirstOrDefaultAsync();
            Assert.Multiple(() =>
            {
                Assert.That(movement, Is.Not.Null);
                Assert.That(movement.Type, Is.EqualTo(LoyalitProgramMovementType.Consume));
                Assert.That(movement.Points, Is.EqualTo(500));
            });
        }

        [Test]
        public async Task EarnPointsAsync_ShouldReturnZero_WhenAmountPaidIsZeroOrNegative()
        {
            // Arrange
            decimal amountPaid = 0.0m;

            // Act
            int pointsEarned = await _service.EarnPointsAsync(amountPaid, _accountId, _restaurantId);

            // Assert
            Assert.That(pointsEarned, Is.EqualTo(0));
        }

        [Test]
        public async Task EarnPointsAsync_ShouldEarnPointsAndRecordMovement_WhenAmountPaidIsPositive()
        {
            // Arrange
            var program = new LoyalitProgram
            {
                AccountId = _accountId,
                RestaurantId = _restaurantId,
                Points = 50,
                Active = true,
                JoinedAt = DateTime.UtcNow
            };
            await _context.LoyalitPrograms.AddAsync(program);
            await _context.SaveChangesAsync();

            decimal amountPaid = 35.50m;

            // Act
            int pointsEarned = await _service.EarnPointsAsync(amountPaid, _accountId, _restaurantId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(pointsEarned, Is.EqualTo(35)); // Math.Floor(35.50) = 35
                Assert.That(program.Points, Is.EqualTo(85)); // 50 + 35 = 85
            });

            var movement = await _context.LoyalitProgramMovements.FirstOrDefaultAsync();
            Assert.Multiple(() =>
            {
                Assert.That(movement, Is.Not.Null);
                Assert.That(movement.Type, Is.EqualTo(LoyalitProgramMovementType.Earn));
                Assert.That(movement.Points, Is.EqualTo(35));
            });
        }
    }
}
