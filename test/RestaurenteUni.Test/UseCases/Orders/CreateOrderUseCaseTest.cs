using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RestauranteUni.Application.UseCases.Orders;
using RestauranteUni.Application.UseCases.Orders.Validations;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Menus;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Orders.DTO;
using RestauranteUni.Domain.Core.Stocks;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestauranteUni.Domain.Core.Restaurants;
using RestauranteUni.Domain.ValuesObjects;

namespace RestaurenteUni.Test.UseCases.Orders
{
    [TestFixture]
    public class CreateOrderUseCaseTest
    {
        private ApplicationDbContext _context;
        private CreateOrderUseCaseHandler _handler;
        private Mock<ICurrentUser> _currentUserMock;
        private readonly Guid _restaurantId = Guid.NewGuid();
        private readonly long _accountId = 10;
        private readonly string _userEmail = "user@example.com";
        private Restaurant _seededRestaurant;
        private Menu _seededMenu;
        private Stock _seededStock;

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
            _currentUserMock.Setup(x => x.AccountId).Returns(_accountId);
            _currentUserMock.Setup(x => x.Email).Returns(_userEmail);

            var validator = new CreateOrderDtoValidator();
            _handler = new CreateOrderUseCaseHandler(_context, _currentUserMock.Object, validator);

            SeedBaseData();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private void SeedBaseData()
        {
            _seededRestaurant = new Restaurant
            {
                Id = _restaurantId,
                Name = "Restaurante Central",
                Description = "Restaurante Universitário da Faculdade",
                Phone = new Phone("11999999999"),
                Address = new Address("Rua das Oliveiras", "123", "Bairro Novo", "São Paulo", "SP", "01234567"),
                Email = new Email("ru@faculdade.com"),
                Cnpj = new Cnpj("12345678000195"),
                Active = true
            };
            _context.Restaurants.Add(_seededRestaurant);

            var account = new RestauranteUni.Domain.Core.Accounts.Account
            {
                Id = _accountId,
                Email = new Email(_userEmail),
                Password = "somehashedpassword"
            };
            _context.Accounts.Add(account);

            _seededMenu = new Menu
            {
                Name = "Menu Principal",
                RestaurantId = _restaurantId,
                Restaurant = _seededRestaurant,
                Active = true
            };
            _context.Menus.Add(_seededMenu);

            _seededStock = new Stock
            {
                RestaurantId = _restaurantId,
                Restaurant = _seededRestaurant
            };
            _context.Stocks.Add(_seededStock);

            _context.SaveChanges();
        }

        private MenuItem SeedMenuItem(string title, decimal price, bool isAvailable = true)
        {
            var menuItem = new MenuItem
            {
                Title = title,
                Price = price,
                IsAvailable = isAvailable,
                PublicId = Guid.NewGuid(),
                MenuId = _seededMenu.Id,
                Menu = _seededMenu
            };
            _context.MenuItems.Add(menuItem);
            _context.SaveChanges();
            return menuItem;
        }

        private StockIngredient SeedStockIngredient(string name, decimal quantity)
        {
            var stockIngredient = new StockIngredient
            {
                PublicId = Guid.NewGuid(),
                Name = name,
                Unit = IngredientUnit.Unit,
                Quantity = quantity,
                StockId = _seededStock.Id,
                Stock = _seededStock
            };
            _context.StockIngredients.Add(stockIngredient);
            _context.SaveChanges();
            return stockIngredient;
        }

        private void LinkIngredientToMenuItem(MenuItem menuItem, StockIngredient stockIngredient, decimal quantityUseToOrder)
        {
            var menuItemIngredient = new MenuItemIngredient
            {
                MenuItemId = menuItem.Id,
                MenuItem = menuItem,
                StockIngredientId = stockIngredient.Id,
                StockIngredient = stockIngredient,
                QuantityUseToOrder = quantityUseToOrder
            };
            menuItem.Ingredients.Add(menuItemIngredient);
            _context.SaveChanges();
        }

        [Test]
        public async Task ShouldReturnFailure_WhenChannelIsInvalid()
        {
            var dto = new CreateOrderDto
            {
                Channel = (OrderChannel)999,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Null);
                var channelValidation = result.Validations!.FirstOrDefault(v => v.Property == nameof(CreateOrderDto.Channel));
                Assert.That(channelValidation, Is.Not.Null);
                Assert.That(channelValidation!.Errors, Contains.Item("Canal de pedido inválido."));
            });
        }

        [Test]
        public async Task ShouldReturnFailure_WhenItemsIsNull()
        {
            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = null!
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Null);
                var itemsValidation = result.Validations!.FirstOrDefault(v => v.Property == nameof(CreateOrderDto.Items));
                Assert.That(itemsValidation, Is.Not.Null);
                Assert.That(itemsValidation!.Errors, Contains.Item("Os itens são obrigatórios."));
            });
        }

        [Test]
        public async Task ShouldReturnFailure_WhenItemsIsEmpty()
        {
            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>()
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Null);
                var itemsValidation = result.Validations!.FirstOrDefault(v => v.Property == nameof(CreateOrderDto.Items));
                Assert.That(itemsValidation, Is.Not.Null);
                Assert.That(itemsValidation!.Errors, Contains.Item("O pedido deve conter pelo menos um item."));
            });
        }

        [Test]
        public async Task ShouldReturnFailure_WhenMenuItemIdIsEmpty()
        {
            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = Guid.Empty, Quantity = 1 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Null);
                var itemValidation = result.Validations!.FirstOrDefault(v => v.Property.StartsWith("Items["));
                Assert.That(itemValidation, Is.Not.Null);
                Assert.That(itemValidation!.Errors, Contains.Item("O ID do item do cardápio é obrigatório."));
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-5.5)]
        public async Task ShouldReturnFailure_WhenQuantityIsZeroOrNegative(decimal quantity)
        {
            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = Guid.NewGuid(), Quantity = quantity }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Null);
                var itemValidation = result.Validations!.FirstOrDefault(v => v.Property.StartsWith("Items["));
                Assert.That(itemValidation, Is.Not.Null);
                Assert.That(itemValidation!.Errors, Contains.Item("A quantidade deve ser maior que zero."));
            });
        }

        [Test]
        public async Task ShouldReturnNotFound_WhenMenuItemsNotFound()
        {
            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = Guid.NewGuid(), Quantity = 2 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(result.ErrorData, Is.Not.Null);
                Assert.That(result.ErrorData!.Message, Is.EqualTo("Itens do cardápio não encontrados"));
            });
        }

        [Test]
        public async Task ShouldReturnFailure_WhenSomeMenuItemsIsNotAvailable()
        {
            var menuItem = SeedMenuItem("Pizza", 15.00m, isAvailable: false);

            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = menuItem.PublicId, Quantity = 1 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(result.ErrorData, Is.Not.Null);
                Assert.That(result.ErrorData!.Message, Is.EqualTo("Alguns itens do cardápio não estão disponíveis."));
                
                var details = result.ErrorData.Details as IEnumerable<object>;
                Assert.That(details, Is.Not.Null);
                var detailsList = details!.ToList();
                Assert.That(detailsList.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task ShouldReturnFailure_WhenIngredientsAreOutOfStock()
        {
            var menuItem = SeedMenuItem("Burguer", 10.00m, isAvailable: true);
            var ingredient = SeedStockIngredient("Pão", 1);
            LinkIngredientToMenuItem(menuItem, ingredient, 2);

            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = menuItem.PublicId, Quantity = 1 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(result.ErrorData, Is.Not.Null);
                Assert.That(result.ErrorData!.Message, Is.EqualTo("Alguns itens do cardápio não possuem ingredientes suficientes em estoque"));
                
                var details = result.ErrorData.Details as IEnumerable<object>;
                Assert.That(details, Is.Not.Null);
                var detailsList = details!.ToList();
                Assert.That(detailsList.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task ShouldReturnSuccess_WhenOrderCreatedSuccessfully()
        {
            var menuItem = SeedMenuItem("Pasta", 12.50m, isAvailable: true);
            var ingredient = SeedStockIngredient("Massa", 10);
            LinkIngredientToMenuItem(menuItem, ingredient, 2);

            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = menuItem.PublicId, Quantity = 3 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.TotalPrice, Is.EqualTo(37.50m));
                Assert.That(result.Data.AccountId, Is.EqualTo(_accountId));
                Assert.That(result.Data.AccountEmail, Is.EqualTo(_userEmail));
                Assert.That(result.Data.Status, Is.EqualTo(OrderStatus.Chicken));
                Assert.That(result.Data.Items, Has.Count.EqualTo(1));

                var orderItemDto = result.Data.Items[0];
                Assert.That(orderItemDto.MenuItemId, Is.EqualTo(menuItem.PublicId));
                Assert.That(orderItemDto.MenuItemName, Is.EqualTo("Pasta"));
                Assert.That(orderItemDto.Quantity, Is.EqualTo(3));
                Assert.That(orderItemDto.UnitPrice, Is.EqualTo(12.50m));
            });

            var orderInDb = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
                .FirstOrDefaultAsync(o => o.PublicId == result.Data!.Id);
            
            Assert.That(orderInDb, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(orderInDb!.Status, Is.EqualTo(OrderStatus.Chicken));
                Assert.That(orderInDb.Channel, Is.EqualTo(OrderChannel.Web));
                Assert.That(orderInDb.RestaurantId, Is.EqualTo(_restaurantId));
                Assert.That(orderInDb.Items, Has.Count.EqualTo(1));
                Assert.That(orderInDb.Items.First().MenuItemId, Is.EqualTo(menuItem.Id));
            });

            var ingredientInDb = await _context.StockIngredients.FindAsync(ingredient.Id);
            Assert.That(ingredientInDb, Is.Not.Null);
            Assert.That(ingredientInDb!.Quantity, Is.EqualTo(4));

            var movements = await _context.StockIngredientMovements
                .Where(m => m.StockIngredientId == ingredient.Id)
                .ToListAsync();
            
            Assert.That(movements, Has.Count.EqualTo(1));
            var movement = movements.First();
            Assert.Multiple(() =>
            {
                Assert.That(movement.Quantity, Is.EqualTo(6));
                Assert.That(movement.Type, Is.EqualTo(StockMovementType.Consumption));
                Assert.That(movement.OrderId, Is.EqualTo(orderInDb!.Id));
            });

            var menuItemInDb = await _context.MenuItems.FindAsync(menuItem.Id);
            Assert.That(menuItemInDb, Is.Not.Null);
            Assert.That(menuItemInDb!.IsAvailable, Is.True);
        }

        [Test]
        public async Task ShouldSetMenuItemAsNotAvailable_WhenStockDropsBelowRequiredQuantity()
        {
            var menuItem = SeedMenuItem("Pasta", 12.50m, isAvailable: true);
            var ingredient = SeedStockIngredient("Massa", 5);
            LinkIngredientToMenuItem(menuItem, ingredient, 2);

            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = menuItem.PublicId, Quantity = 2 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.That(result.IsSuccess, Is.True);

            var menuItemInDb = await _context.MenuItems.FindAsync(menuItem.Id);
            Assert.That(menuItemInDb, Is.Not.Null);
            Assert.That(menuItemInDb!.IsAvailable, Is.False);
        }

        [Test]
        public async Task ShouldGroupDuplicateItems_WhenDuplicateMenuItemsAreProvided()
        {
            var menuItem = SeedMenuItem("Pasta", 12.50m, isAvailable: true);
            var ingredient = SeedStockIngredient("Massa", 10);
            LinkIngredientToMenuItem(menuItem, ingredient, 2);

            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = menuItem.PublicId, Quantity = 1 },
                    new CreateOrderItemDto { PublicMenuItemId = menuItem.PublicId, Quantity = 2 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.TotalPrice, Is.EqualTo(37.50m));
                Assert.That(result.Data.Items, Has.Count.EqualTo(1));
                Assert.That(result.Data.Items[0].Quantity, Is.EqualTo(3));
            });
        }

        [Test]
        public async Task ShouldReturnNotFound_WhenSomeRequestedMenuItemsAreNotFound()
        {
            var menuItem = SeedMenuItem("Pasta", 12.50m, isAvailable: true);
            var dto = new CreateOrderDto
            {
                Channel = OrderChannel.Web,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { PublicMenuItemId = menuItem.PublicId, Quantity = 1 },
                    new CreateOrderItemDto { PublicMenuItemId = Guid.NewGuid(), Quantity = 2 }
                }
            };

            var result = await _handler.HandleAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(result.ErrorData, Is.Not.Null);
                Assert.That(result.ErrorData!.Message, Is.EqualTo("Alguns itens do cardápio não foram encontrados"));
            });
        }
    }
}
