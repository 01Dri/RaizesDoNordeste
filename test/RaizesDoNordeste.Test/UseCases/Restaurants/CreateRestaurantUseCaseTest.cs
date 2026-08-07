using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Restaurants;
using RaizesDoNordeste.Application.UseCases.Restaurants.Validations;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Restaurants.DTO;

namespace RaizesDoNordeste.Test.UseCases.Restaurants
{
    [TestFixture]
    public class CreateRestaurantUseCaseTest
    {
        private ApplicationDbContext _context;
        private CreateRestaurantUseCaseHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            var validator = new CreateRestaurantDtoValidation();
            _handler = new CreateRestaurantUseCaseHandler(_context, validator);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ValidDto_ShouldCreateRestaurant()
        {
            // Arrange
            var dto = new CreateRestaurantDto
            {
                Name = "Raízes do Nordeste - Unidade Recife",
                Description = "Unidade de atendimento no centro de Recife",
                Phone = "81988887777",
                Email = "recife@raizesdonordeste.com",
                Cnpj = "12345678000276",
                AddressStreet = "Rua do Sol",
                AddressNumber = "100",
                AddressDistrict = "Santo Antônio",
                AddressCity = "Recife",
                AddressState = "PE",
                AddressZipCode = "50010000"
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Name, Is.EqualTo("Raízes do Nordeste - Unidade Recife"));
                Assert.That(result.Data.Cnpj, Is.EqualTo("12345678000276"));
                Assert.That(result.Data.Phone, Is.EqualTo("81988887777"));
                Assert.That(result.Data.AddressCity, Is.EqualTo("Recife"));
            });
        }

        [Test]
        public async Task HandleAsync_DuplicateCnpj_ShouldReturnConflict()
        {
            // Arrange - CNPJ from seeded builder: 12345678000195
            var dto = new CreateRestaurantDto
            {
                Name = "Raízes do Nordeste - Duplicada",
                Description = "Teste duplicado",
                Phone = "11988887777",
                Email = "dup@raizesdonordeste.com",
                Cnpj = "12.345.678/0001-95",
                AddressStreet = "Rua Teste",
                AddressNumber = "1",
                AddressDistrict = "Bairro",
                AddressCity = "São Paulo",
                AddressState = "SP",
                AddressZipCode = "01001000"
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Conflict));
        }

        [Test]
        public async Task HandleAsync_DuplicateEmail_ShouldReturnConflict()
        {
            // Arrange - Email from seeded builder: central@raizesdonordeste.com
            var dto = new CreateRestaurantDto
            {
                Name = "Raízes do Nordeste - Duplicada Email",
                Description = "Teste duplicado email",
                Phone = "11988887777",
                Email = "cantina@raizesdonordeste.com",
                Cnpj = "12.345.678/0002-76",
                AddressStreet = "Rua Teste",
                AddressNumber = "1",
                AddressDistrict = "Bairro",
                AddressCity = "São Paulo",
                AddressState = "SP",
                AddressZipCode = "01001000"
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Conflict));
            Assert.That(result.ErrorData?.Message, Contains.Substring("e-mail"));
        }
    }
}
