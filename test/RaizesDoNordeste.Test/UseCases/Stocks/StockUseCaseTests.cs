using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Stocks;
using RaizesDoNordeste.Application.UseCases.Stocks.Validations;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.Core.Users;

namespace RaizesDoNordeste.Test.UseCases.Stocks
{
    [TestFixture]
    public class StockUseCaseTests
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private readonly Guid _userRestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");

        private RegisterStockMovementUseCaseHandler _movementHandler;
        private GetStockByRestaurantUseCaseHandler _getByRestaurantHandler;
        private GetStockOfCurrentUserUseCaseHandler _getCurrentUserStockHandler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.RestaurantId).Returns(_userRestaurantId);

            _movementHandler = new RegisterStockMovementUseCaseHandler(_context, new StockMovementRequestDtoValidator(), _currentUserMock.Object);
            _getByRestaurantHandler = new GetStockByRestaurantUseCaseHandler(_context);
            _getCurrentUserStockHandler = new GetStockOfCurrentUserUseCaseHandler(_context, _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Move_ShouldIncreaseStockQuantity_OnEntry()
        {
            // Arrange
            // We use pre-seeded StockIngredient with Id = 1 (Tomate, quantity = 100)
            var dto = new StockMovementRequestDto
            {
                StockIngredientId = 1L,
                Quantity = 15.5m,
                Type = StockMovementType.Entry,
                Description = "Compra de tomate"
            };

            // Act
            var result = await _movementHandler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.NewStockQuantity, Is.EqualTo(115.5m));
                Assert.That(result.Data.Type, Is.EqualTo("Entry"));
            });

            var dbIngredient = await _context.StockIngredients.FindAsync(1L);
            Assert.That(dbIngredient.Quantity, Is.EqualTo(115.5m));
        }

        [Test]
        public async Task Move_ShouldDecreaseStockQuantity_OnLoss()
        {
            // Arrange
            // We use pre-seeded StockIngredient with Id = 2 (Alface, quantity = 50)
            var dto = new StockMovementRequestDto
            {
                StockIngredientId = 2L,
                Quantity = 5.0m,
                Type = StockMovementType.Loss,
                Description = "Alface estragada"
            };

            // Act
            var result = await _movementHandler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.NewStockQuantity, Is.EqualTo(45.0m));
                Assert.That(result.Data.Type, Is.EqualTo("Loss"));
            });

            var dbIngredient = await _context.StockIngredients.FindAsync(2L);
            Assert.That(dbIngredient.Quantity, Is.EqualTo(45.0m));
        }

        [Test]
        public async Task Move_ShouldReturnBadRequest_WhenInsufficientStockForLoss()
        {
            // Arrange
            // We use pre-seeded StockIngredient with Id = 2 (Alface, quantity = 50)
            var dto = new StockMovementRequestDto
            {
                StockIngredientId = 2L,
                Quantity = 60.0m, // More than 50
                Type = StockMovementType.Loss
            };

            // Act
            var result = await _movementHandler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            });
        }

        [Test]
        public async Task GetCurrentStock_ShouldReturnStockData()
        {
            // Act
            var result = await _getCurrentUserStockHandler.HandleAsync(CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Items.Count, Is.EqualTo(2));
                
                var tomate = result.Data.Items.FirstOrDefault(i => i.Id == 1L);
                Assert.That(tomate, Is.Not.Null);
                Assert.That(tomate.Name, Is.EqualTo("Tomate"));
                Assert.That(tomate.Quantity, Is.EqualTo(100.0m));

                var alface = result.Data.Items.FirstOrDefault(i => i.Id == 2L);
                Assert.That(alface, Is.Not.Null);
                Assert.That(alface.Name, Is.EqualTo("Alface"));
                Assert.That(alface.Quantity, Is.EqualTo(50.0m));
            });
        }
    }
}
