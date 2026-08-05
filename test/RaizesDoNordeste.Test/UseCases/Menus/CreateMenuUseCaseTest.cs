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
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Restaurants;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Test.UseCases.Menus
{
    [TestFixture]
    public class CreateMenuUseCaseTest
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private CreateMenuUseCaseHandler _handler;
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

            _handler = new CreateMenuUseCaseHandler(_context, new CreateMenuDtoValidator(), _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreateMenu_ShouldReturnSuccess_WhenValidData()
        {
            // Arrange
            var dto = new CreateMenuDto
            {
                Name = "Cardápio de Sobremesas",
                RestaurantId = _restaurantId
            };

            // Act
            var result = await _handler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.Name, Is.EqualTo("Cardápio de Sobremesas"));
                Assert.That(result.Data.RestaurantId, Is.EqualTo(_restaurantId));
            });

            var dbMenu = await _context.Menus.FirstOrDefaultAsync(m => m.RestaurantId == _restaurantId && m.Name == "Cardápio de Sobremesas");
            Assert.That(dbMenu, Is.Not.Null);
        }

        [Test]
        public async Task CreateMenu_ShouldReturnBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            var dto = new CreateMenuDto
            {
                Name = "",
                RestaurantId = _restaurantId
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

        [Test]
        public async Task CreateMenu_ShouldReturnNotFound_WhenRestaurantDoesNotExist()
        {
            // Arrange
            var dto = new CreateMenuDto
            {
                Name = "Cardápio Especial",
                RestaurantId = Guid.NewGuid()
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
        public async Task CreateMenu_ShouldReturnConflict_WhenMenuWithSameNameAlreadyExists()
        {
            // Arrange
            // Seeded database already has "Teste" for _restaurantId
            var dto = new CreateMenuDto
            {
                Name = "Teste",
                RestaurantId = _restaurantId
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
    }
}
