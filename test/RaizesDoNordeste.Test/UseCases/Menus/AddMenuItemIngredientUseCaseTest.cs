using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Menus;
using RaizesDoNordeste.Application.UseCases.Menus.Validations;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.Core.Users;

namespace RaizesDoNordeste.Test.UseCases.Menus
{
    [TestFixture]
    public class AddMenuItemIngredientUseCaseTest
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private AddMenuItemIngredientUseCaseHandler _handler;
        private readonly Guid _restaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.RestaurantId).Returns(_restaurantId);

            _handler = new AddMenuItemIngredientUseCaseHandler(_context, new AddMenuItemIngredientDtoValidator(), _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task AddIngredient_ShouldReturnSuccess_WhenValidParameters()
        {
            // Arrange
            // StockIngredient 1 (Tomate) is pre-seeded with Stock belonging to _restaurantId
            // MenuItem 1 (Baião de Dois) is pre-seeded belonging to Menu belonging to _restaurantId
            var dto = new AddMenuItemIngredientDto
            {
                MenuItemId = 1L,
                StockIngredientId = 1L,
                QuantityUseToOrder = 0.200m
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.MenuItemId, Is.EqualTo(1L));
                Assert.That(result.Data.StockIngredientId, Is.EqualTo(1L));
                Assert.That(result.Data.QuantityUseToOrder, Is.EqualTo(0.200m));
            });

            var dbLink = await _context.MenuItemIngredients
                .FirstOrDefaultAsync(x => x.MenuItemId == 1L && x.StockIngredientId == 1L);
            Assert.That(dbLink, Is.Not.Null);
            Assert.That(dbLink!.QuantityUseToOrder, Is.EqualTo(0.200m));
        }

        [Test]
        public async Task AddIngredient_ShouldReturnNotFound_WhenMenuItemDoesNotExist()
        {
            // Arrange
            var dto = new AddMenuItemIngredientDto
            {
                MenuItemId = 9999L,
                StockIngredientId = 1L,
                QuantityUseToOrder = 0.5m
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
        public async Task AddIngredient_ShouldReturnNotFound_WhenStockIngredientDoesNotExist()
        {
            // Arrange
            var dto = new AddMenuItemIngredientDto
            {
                MenuItemId = 1L,
                StockIngredientId = 9999L,
                QuantityUseToOrder = 0.5m
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
        public async Task AddIngredient_ShouldReturnBadRequest_WhenQuantityIsZeroOrNegative()
        {
            // Arrange
            var dto = new AddMenuItemIngredientDto
            {
                MenuItemId = 1L,
                StockIngredientId = 1L,
                QuantityUseToOrder = 0.0m
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            });
        }
    }
}
