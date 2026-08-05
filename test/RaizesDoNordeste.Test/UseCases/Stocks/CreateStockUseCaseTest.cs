using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Stocks;
using RaizesDoNordeste.Application.UseCases.Stocks.Validations;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Restaurants;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Test.UseCases.Stocks
{
    [TestFixture]
    public class CreateStockUseCaseTest
    {
        private ApplicationDbContext _context;
        private CreateStockUseCaseHandler _handler;
        private readonly Guid _newRestaurantId = Guid.NewGuid();
        private readonly Guid _existingRestaurantWithStockId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            // Add a new restaurant without stock
            _context.Restaurants.Add(new Restaurant
            {
                Id = _newRestaurantId,
                Name = "Nova Unidade Recife",
                Description = "Unidade Recife",
                Phone = new Phone("(81) 98888-7777"),
                Email = new Email("recife@raizes.com"),
                Cnpj = new Cnpj("11222333000181")
            });
            _context.SaveChanges();

            _handler = new CreateStockUseCaseHandler(_context, new CreateStockDtoValidation());
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreateStock_ShouldReturnSuccess_WhenRestaurantHasNoStock()
        {
            // Arrange
            var dto = new CreateStockRequestDto
            {
                RestaurantId = _newRestaurantId,
                Items = new List<CreateStockIngredientItemDto>
                {
                    new CreateStockIngredientItemDto { Name = "Farinha de Mandioca", Unit = IngredientUnit.Kilogram, Quantity = 20.0m },
                    new CreateStockIngredientItemDto { Name = "Queijo Coalho", Unit = IngredientUnit.Kilogram, Quantity = 15.0m }
                }
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.RestaurantId, Is.EqualTo(_newRestaurantId));
                Assert.That(result.Data.Items.Count, Is.EqualTo(2));
            });

            var dbStock = await _context.Stocks.Include(s => s.Items).FirstOrDefaultAsync(s => s.RestaurantId == _newRestaurantId);
            Assert.That(dbStock, Is.Not.Null);
            Assert.That(dbStock!.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task CreateStock_ShouldReturnNotFound_WhenRestaurantDoesNotExist()
        {
            // Arrange
            var dto = new CreateStockRequestDto
            {
                RestaurantId = Guid.NewGuid(),
                Items = []
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
        public async Task CreateStock_ShouldReturnConflict_WhenStockAlreadyExistsForRestaurant()
        {
            // Arrange
            var dto = new CreateStockRequestDto
            {
                RestaurantId = _existingRestaurantWithStockId,
                Items = []
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            });
        }

        [Test]
        public async Task CreateStock_ShouldReturnBadRequest_WhenRestaurantIdIsEmpty()
        {
            // Arrange
            var dto = new CreateStockRequestDto
            {
                RestaurantId = Guid.Empty
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
