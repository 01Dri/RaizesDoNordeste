using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Restaurants;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Restaurants;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Test.UseCases.Restaurants
{
    [TestFixture]
    public class ListRestaurantsUseCaseTest
    {
        private ApplicationDbContext _context;
        private ListRestaurantsUseCase _useCase;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _useCase = new ListRestaurantsUseCase(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ShouldReturnActiveRestaurants()
        {
            // Arrange
            // Note: DB is pre-seeded with 3 restaurants via EnsureCreated() in RestaurantBuilder.

            // Act
            var result = await _useCase.HandleAsync(CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Restaurants.Count, Is.EqualTo(3));
                
                var first = result.Data.Restaurants.FirstOrDefault(r => r.Name == "Restaurante Universitario");
                Assert.That(first, Is.Not.Null);
                Assert.That(first.Phone, Is.EqualTo("1133334444"));
                Assert.That(first.Email, Is.EqualTo("ru@raizesdonordeste.com"));
                Assert.That(first.Cnpj, Is.EqualTo("12345678000195"));
                Assert.That(first.AddressStreet, Is.EqualTo("Avenida Universitaria"));
            });
        }
    }
}
