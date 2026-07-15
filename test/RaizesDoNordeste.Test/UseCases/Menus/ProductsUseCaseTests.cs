using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Menus;
using RaizesDoNordeste.Application.UseCases.Menus.Validations;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Restaurants;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Test.UseCases.Menus
{
    [TestFixture]
    public class ProductsUseCaseTests
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private readonly Guid _userRestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");

        private CreateProductUseCaseHandler _createHandler;
        private UpdateProductUseCaseHandler _updateHandler;
        private DeleteProductUseCaseHandler _deleteHandler;
        private GetProductByIdUseCaseHandler _getByIdHandler;
        private ListProductsUseCaseHandler _listHandler;

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

            _createHandler = new CreateProductUseCaseHandler(_context, new CreateProductDtoValidator(), _currentUserMock.Object);
            _updateHandler = new UpdateProductUseCaseHandler(_context, new UpdateProductDtoValidator(), _currentUserMock.Object);
            _deleteHandler = new DeleteProductUseCaseHandler(_context, _currentUserMock.Object);
            _getByIdHandler = new GetProductByIdUseCaseHandler(_context, _currentUserMock.Object);
            _listHandler = new ListProductsUseCaseHandler(_context, _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Create_ShouldAddNewProduct_WhenRequestIsValid()
        {
            // Arrange
            // Note: Menu with RestaurantId _userRestaurantId is pre-seeded in database (id = 1)
            var dto = new CreateProductDto
            {
                Title = "Coxinha Nordestina",
                Description = "Coxinha de frango desfiado com tempero especial",
                Price = 8.50m,
                PreparationTimeInMinutes = 10,
                IsAvailable = true
            };

            // Act
            var result = await _createHandler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Title, Is.EqualTo("Coxinha Nordestina"));
                Assert.That(result.Data.Price, Is.EqualTo(8.50m));
                Assert.That(result.Data.MenuId, Is.EqualTo(1L));
            });

            var dbItem = await _context.MenuItems.FirstOrDefaultAsync(i => i.Title == "Coxinha Nordestina");
            Assert.Multiple(() =>
            {
                Assert.That(dbItem, Is.Not.Null);
                Assert.That(dbItem.Price, Is.EqualTo(8.50m));
            });
        }

        [Test]
        public async Task Create_ShouldReturnValidationFailure_WhenPriceIsNegative()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Title = "Coxinha Nordestina",
                Price = -5.00m // Invalid
            };

            // Act
            var result = await _createHandler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Empty);
            });
        }

        [Test]
        public async Task Update_ShouldModifyProduct_WhenOwnerAndRequestAreValid()
        {
            // Arrange
            var item = new MenuItem
            {
                Id = 150,
                Title = "Old Title",
                Price = 10.00m,
                MenuId = 1L // Pre-seeded Menu belonging to user restaurant
            };
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            var dto = new UpdateProductDto
            {
                Id = 150,
                Title = "New Title",
                Price = 12.00m,
                IsAvailable = true
            };

            // Act
            var result = await _updateHandler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Title, Is.EqualTo("New Title"));
                Assert.That(result.Data.Price, Is.EqualTo(12.00m));
            });

            var dbItem = await _context.MenuItems.FindAsync(150L);
            Assert.Multiple(() =>
            {
                Assert.That(dbItem, Is.Not.Null);
                Assert.That(dbItem.Title, Is.EqualTo("New Title"));
                Assert.That(dbItem.Price, Is.EqualTo(12.00m));
            });
        }

        [Test]
        public async Task Update_ShouldReturnForbidden_WhenProductBelongsToOtherRestaurant()
        {
            // Arrange
            // Seed a menu for other restaurant
            var otherMenu = new Menu
            {
                Id = 50,
                Name = "Other Restaurant Menu",
                RestaurantId = Guid.Parse("f02884ad-1725-4fcb-9bb6-cbf0b8f5fef6"),
                Active = true
            };
            var item = new MenuItem
            {
                Id = 160,
                Title = "Other Product",
                Price = 15.00m,
                MenuId = 50,
                Menu = otherMenu,
                Active = true
            };
            _context.Menus.Add(otherMenu);
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            var dto = new UpdateProductDto
            {
                Id = 160,
                Title = "Att Title",
                Price = 20.00m
            };

            // Act
            var result = await _updateHandler.HandleAsync(dto, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
            });
        }

        [Test]
        public async Task Delete_ShouldRemoveProduct_WhenOwner()
        {
            // Arrange
            var item = new MenuItem
            {
                Id = 170,
                Title = "To Delete",
                Price = 5.00m,
                MenuId = 1L
            };
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var result = await _deleteHandler.HandleAsync(new DeleteProductDto(170L), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Success, Is.True);
            });

            var dbItem = await _context.MenuItems.FindAsync(170L);
            Assert.That(dbItem, Is.Null);
        }

        [Test]
        public async Task GetById_ShouldReturnProduct_WhenExistsAndOwner()
        {
            // Arrange
            var item = new MenuItem
            {
                Id = 180,
                Title = "Find Me",
                Price = 25.00m,
                MenuId = 1L
            };
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var result = await _getByIdHandler.HandleAsync(new GetProductByIdQueryDto(180L), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data.Title, Is.EqualTo("Find Me"));
                Assert.That(result.Data.Price, Is.EqualTo(25.00m));
            });
        }

        [Test]
        public async Task List_ShouldOnlyReturnProductsOfUserRestaurant()
        {
            // Arrange
            var item1 = new MenuItem { Id = 190, Title = "My Restaurant Product", Price = 10m, MenuId = 1L };
            
            var otherMenu = new Menu { Id = 80, Name = "Other", RestaurantId = Guid.NewGuid() };
            var item2 = new MenuItem { Id = 191, Title = "Other Restaurant Product", Price = 15m, Menu = otherMenu };

            _context.Menus.Add(otherMenu);
            _context.MenuItems.AddRange(item1, item2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _listHandler.HandleAsync(CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                // We should find the pre-seeded items + item1, but NOT item2!
                Assert.That(result.Data.Products.Any(p => p.Id == 190L), Is.True);
                Assert.That(result.Data.Products.Any(p => p.Id == 191L), Is.False);
            });
        }
    }
}
