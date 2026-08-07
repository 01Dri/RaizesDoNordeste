using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using MenuItem = RaizesDoNordeste.Domain.Core.Menus.MenuItem;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class CreateProductUseCaseHandler : IUseCaseHandler<CreateProductDto, ProductResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateProductDto> _validator;
        private readonly ICurrentUser _currentUser;

        public CreateProductUseCaseHandler(ApplicationDbContext context, IValidator<CreateProductDto> validator, ICurrentUser currentUser)
        {
            _context = context;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<ProductResponseDto>> HandleAsync(CreateProductDto parameter, CancellationToken cancellation = default)
        {
            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
                return validation.ToResultFailure<ProductResponseDto>();

            var restaurantId = _currentUser.RestaurantId;
            if (restaurantId == Guid.Empty)
                return Result<ProductResponseDto>.Failure(new Error("Restaurante do usuário não identificado."));

            var menu = await GetOrCreateMenuAsync(restaurantId, cancellation);
            var stock = await GetOrCreateStockAsync(restaurantId, cancellation);

            var item = new MenuItem
            {
                Title = parameter.Title,
                Description = parameter.Description,
                Price = parameter.Price,
                ImageUrl = parameter.ImageUrl,
                IsAvailable = parameter.IsAvailable,
                DisplayOrder = parameter.DisplayOrder,
                PreparationTimeInMinutes = parameter.PreparationTimeInMinutes,
                IsFeatured = parameter.IsFeatured,
                MenuId = menu.Id
            };

            if (parameter.Ingredients is { Count: > 0 })
            {
                foreach (var ingInput in parameter.Ingredients)
                {
                    var stockIng = await ResolveStockIngredientAsync(stock, ingInput, cancellation);
                    item.Ingredients.Add(new MenuItemIngredient
                    {
                        MenuItem = item,
                        StockIngredientId = stockIng.Id,
                        StockIngredient = stockIng,
                        QuantityUseToOrder = ingInput.QuantityUseToOrder > 0 ? ingInput.QuantityUseToOrder : 1m
                    });
                }
            }

            await _context.MenuItems.AddAsync(item, cancellation);
            await _context.SaveChangesAsync(cancellation);

            return Result<ProductResponseDto>.Success(MapToResponseDto(item), HttpStatusCode.Created);
        }

        #region Private Subroutines & Resolvers

        private async Task<Menu> GetOrCreateMenuAsync(Guid restaurantId, CancellationToken cancellation)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(m => m.RestaurantId == restaurantId, cancellation);
            if (menu != null) return menu;

            menu = new Menu
            {
                PublicId = Guid.NewGuid(),
                Name = "Cardápio Principal",
                RestaurantId = restaurantId
            };
            await _context.Menus.AddAsync(menu, cancellation);
            await _context.SaveChangesAsync(cancellation);
            return menu;
        }

        private async Task<Stock> GetOrCreateStockAsync(Guid restaurantId, CancellationToken cancellation)
        {
            var stock = await _context.Stocks
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.RestaurantId == restaurantId, cancellation);
            if (stock != null) return stock;

            stock = new Stock
            {
                PublicId = Guid.NewGuid(),
                RestaurantId = restaurantId,
                Items = []
            };
            await _context.Stocks.AddAsync(stock, cancellation);
            await _context.SaveChangesAsync(cancellation);
            return stock;
        }

        private async Task<StockIngredient> ResolveStockIngredientAsync(Stock stock, ProductIngredientInputDto ingInput, CancellationToken cancellation)
        {
            if (ingInput.StockIngredientId is > 0)
            {
                var byId = stock.Items.FirstOrDefault(x => x.Id == ingInput.StockIngredientId.Value)
                    ?? await _context.StockIngredients.FirstOrDefaultAsync(x => x.Id == ingInput.StockIngredientId.Value && x.StockId == stock.Id, cancellation);
                if (byId != null) return byId;
            }

            if (!string.IsNullOrWhiteSpace(ingInput.Name))
            {
                var nameTrimmed = ingInput.Name.Trim();
                var byName = stock.Items.FirstOrDefault(x => x.Name.Equals(nameTrimmed, StringComparison.OrdinalIgnoreCase))
                    ?? await _context.StockIngredients.FirstOrDefaultAsync(x => x.StockId == stock.Id && x.Name.ToLower() == nameTrimmed.ToLower(), cancellation);
                if (byName != null) return byName;
            }

            var ingName = string.IsNullOrWhiteSpace(ingInput.Name) ? "Ingrediente" : ingInput.Name.Trim();
            var newIngredient = new StockIngredient
            {
                PublicId = Guid.NewGuid(),
                Name = ingName,
                Unit = ingInput.Unit,
                Quantity = ingInput.InitialStockQuantity > 0 ? ingInput.InitialStockQuantity : 100m,
                StockId = stock.Id
            };

            await _context.StockIngredients.AddAsync(newIngredient, cancellation);
            await _context.SaveChangesAsync(cancellation);
            stock.Items.Add(newIngredient);

            return newIngredient;
        }

        private static ProductResponseDto MapToResponseDto(MenuItem item) => new()
        {
            Id = item.Id,
            PublicId = item.PublicId,
            Title = item.Title,
            Description = item.Description,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            IsAvailable = item.IsAvailable,
            DisplayOrder = item.DisplayOrder,
            PreparationTimeInMinutes = item.PreparationTimeInMinutes,
            IsFeatured = item.IsFeatured,
            MenuId = item.MenuId ?? 0L,
            Ingredients = item.Ingredients.Select(i => new ProductIngredientResponseDto
            {
                Id = i.Id ?? 0L,
                StockIngredientId = i.StockIngredientId ?? 0L,
                StockIngredientName = i.StockIngredient?.Name ?? string.Empty,
                QuantityUseToOrder = i.QuantityUseToOrder
            }).ToList()
        };

        #endregion
    }
}
