using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Stocks;
using RaizesDoNordeste.Application.UseCases.Stocks.Validations;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.Core.Users;

namespace RaizesDoNordeste.Test.UseCases.Stocks
{
    [TestFixture]
    public class AddStockIngredientUseCaseTest
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private AddStockIngredientUseCaseHandler _handler;
        private readonly Guid _restaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.RestaurantId).Returns(_restaurantId);

            _handler = new AddStockIngredientUseCaseHandler(_context, new AddStockIngredientDtoValidation(), _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ValidIngredient_ShouldAddToStock()
        {
            // Arrange
            var dto = new AddStockIngredientDto
            {
                Name = "Farinha de Mandioca Nova",
                Unit = IngredientUnit.Kilogram,
                Quantity = 50.0m
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Name, Is.EqualTo("Farinha de Mandioca Nova"));
                Assert.That(result.Data.Quantity, Is.EqualTo(50.0m));
            });
        }
    }
}
