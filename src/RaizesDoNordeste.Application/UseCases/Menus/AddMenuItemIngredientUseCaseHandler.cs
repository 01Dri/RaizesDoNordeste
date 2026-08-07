using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class AddMenuItemIngredientUseCaseHandler : IUseCaseHandler<AddMenuItemIngredientDto, AddMenuItemIngredientResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<AddMenuItemIngredientDto> _validator;
        private readonly ICurrentUser _currentUser;

        public AddMenuItemIngredientUseCaseHandler(ApplicationDbContext context, IValidator<AddMenuItemIngredientDto> validator, ICurrentUser currentUser)
        {
            _context = context;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<AddMenuItemIngredientResponseDto>> HandleAsync(AddMenuItemIngredientDto parameter, CancellationToken cancellation = default)
        {
            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
            {
                return validation.ToResultFailure<AddMenuItemIngredientResponseDto>();
            }

            MenuItem? menuItem = null;

            if (parameter.PublicMenuItemId != Guid.Empty)
            {
                menuItem = await _context.MenuItems
                    .Include(m => m.Menu)
                    .FirstOrDefaultAsync(m => m.PublicId == parameter.PublicMenuItemId, cancellation);
            }
            else if (parameter.MenuItemId > 0)
            {
                menuItem = await _context.MenuItems
                    .Include(m => m.Menu)
                    .FirstOrDefaultAsync(m => m.Id == parameter.MenuItemId, cancellation);
            }

            if (menuItem == null)
            {
                return Result<AddMenuItemIngredientResponseDto>.FailureNotFound("Item do cardápio não encontrado.");
            }

            if (menuItem.Menu?.RestaurantId != _currentUser.RestaurantId)
            {
                return Result<AddMenuItemIngredientResponseDto>.Failure(
                    new Error("Você não tem permissão para alterar produtos de outro restaurante."),
                    HttpStatusCode.Forbidden
                );
            }

            StockIngredient? stockIngredient = null;

            if (parameter.PublicStockIngredientId.HasValue && parameter.PublicStockIngredientId.Value != Guid.Empty)
            {
                stockIngredient = await _context.StockIngredients
                    .Include(s => s.Stock)
                    .FirstOrDefaultAsync(s => s.PublicId == parameter.PublicStockIngredientId.Value, cancellation);

                if (stockIngredient == null)
                {
                    return Result<AddMenuItemIngredientResponseDto>.FailureNotFound("Ingrediente de estoque não encontrado pelo Public ID.");
                }

                if (stockIngredient.Stock?.RestaurantId != _currentUser.RestaurantId)
                {
                    return Result<AddMenuItemIngredientResponseDto>.Failure(
                        new Error("O ingrediente de estoque pertence a outra unidade."),
                        HttpStatusCode.Forbidden
                    );
                }
            }
            else if (parameter.StockIngredientId.HasValue && parameter.StockIngredientId.Value > 0)
            {
                stockIngredient = await _context.StockIngredients
                    .Include(s => s.Stock)
                    .FirstOrDefaultAsync(s => s.Id == parameter.StockIngredientId.Value, cancellation);

                if (stockIngredient == null)
                {
                    return Result<AddMenuItemIngredientResponseDto>.FailureNotFound("Ingrediente de estoque não encontrado pelo ID.");
                }

                if (stockIngredient.Stock?.RestaurantId != _currentUser.RestaurantId)
                {
                    return Result<AddMenuItemIngredientResponseDto>.Failure(
                        new Error("O ingrediente de estoque pertence a outra unidade."),
                        HttpStatusCode.Forbidden
                    );
                }
            }
            else if (!string.IsNullOrWhiteSpace(parameter.Name))
            {
                var restaurantId = _currentUser.RestaurantId;
                var stock = await _context.Stocks
                    .Include(s => s.Items)
                    .FirstOrDefaultAsync(s => s.RestaurantId == restaurantId, cancellation);

                if (stock == null)
                {
                    stock = new Stock
                    {
                        PublicId = Guid.NewGuid(),
                        RestaurantId = restaurantId,
                        Items = []
                    };
                    await _context.Stocks.AddAsync(stock, cancellation);
                    await _context.SaveChangesAsync(cancellation);
                }

                var trimmedName = parameter.Name.Trim();
                stockIngredient = stock.Items
                    .FirstOrDefault(i => i.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

                if (stockIngredient == null)
                {
                    stockIngredient = new StockIngredient
                    {
                        PublicId = Guid.NewGuid(),
                        Name = trimmedName,
                        Unit = parameter.Unit,
                        Quantity = parameter.InitialStockQuantity,
                        StockId = stock.Id
                    };
                    await _context.StockIngredients.AddAsync(stockIngredient, cancellation);
                    await _context.SaveChangesAsync(cancellation);
                }
            }
            else
            {
                return Result<AddMenuItemIngredientResponseDto>.Failure(
                    new Error("Informe o ID, o Public ID ou o Nome do ingrediente."),
                    HttpStatusCode.BadRequest
                );
            }

            var menuItemIngredient = await _context.MenuItemIngredients
                .FirstOrDefaultAsync(x => x.MenuItemId == menuItem.Id && x.StockIngredientId == stockIngredient.Id, cancellation);

            if (menuItemIngredient != null)
            {
                menuItemIngredient.QuantityUseToOrder = parameter.QuantityUseToOrder;
            }
            else
            {
                menuItemIngredient = new MenuItemIngredient
                {
                    MenuItemId = menuItem.Id,
                    StockIngredientId = stockIngredient.Id,
                    QuantityUseToOrder = parameter.QuantityUseToOrder
                };
                await _context.MenuItemIngredients.AddAsync(menuItemIngredient, cancellation);
            }

            await _context.SaveChangesAsync(cancellation);

            var response = new AddMenuItemIngredientResponseDto
            {
                Id = menuItemIngredient.Id ?? 0L,
                MenuItemId = menuItem.Id,
                PublicMenuItemId = menuItem.PublicId,
                MenuItemName = menuItem.Title,
                StockIngredientId = stockIngredient.Id,
                PublicStockIngredientId = stockIngredient.PublicId,
                StockIngredientName = stockIngredient.Name,
                QuantityUseToOrder = menuItemIngredient.QuantityUseToOrder
            };

            return Result<AddMenuItemIngredientResponseDto>.Success(response);
        }
    }
}
