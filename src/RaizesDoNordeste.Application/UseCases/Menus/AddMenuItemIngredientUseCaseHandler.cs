using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
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

            var menuItem = await _context.MenuItems
                .Include(m => m.Menu)
                .FirstOrDefaultAsync(m => m.Id == parameter.MenuItemId, cancellation);

            if (menuItem == null)
            {
                return Result<AddMenuItemIngredientResponseDto>.FailureNotFound("Produto do cardápio não encontrado.");
            }

            if (menuItem.Menu?.RestaurantId != _currentUser.RestaurantId)
            {
                return Result<AddMenuItemIngredientResponseDto>.Failure(
                    new Error("Você não tem permissão para alterar produtos de outro restaurante."),
                    HttpStatusCode.Forbidden
                );
            }

            var stockIngredient = await _context.StockIngredients
                .Include(s => s.Stock)
                .FirstOrDefaultAsync(s => s.Id == parameter.StockIngredientId, cancellation);

            if (stockIngredient == null)
            {
                return Result<AddMenuItemIngredientResponseDto>.FailureNotFound("Ingrediente de estoque não encontrado.");
            }

            if (stockIngredient.Stock?.RestaurantId != _currentUser.RestaurantId)
            {
                return Result<AddMenuItemIngredientResponseDto>.Failure(
                    new Error("O ingrediente de estoque pertence a outra unidade."),
                    HttpStatusCode.Forbidden
                );
            }

            var menuItemIngredient = await _context.MenuItemIngredients
                .FirstOrDefaultAsync(x => x.MenuItemId == parameter.MenuItemId && x.StockIngredientId == parameter.StockIngredientId, cancellation);

            if (menuItemIngredient != null)
            {
                menuItemIngredient.QuantityUseToOrder = parameter.QuantityUseToOrder;
            }
            else
            {
                menuItemIngredient = new MenuItemIngredient
                {
                    MenuItemId = parameter.MenuItemId,
                    StockIngredientId = parameter.StockIngredientId,
                    QuantityUseToOrder = parameter.QuantityUseToOrder
                };
                await _context.MenuItemIngredients.AddAsync(menuItemIngredient, cancellation);
            }

            await _context.SaveChangesAsync(cancellation);

            var response = new AddMenuItemIngredientResponseDto
            {
                Id = menuItemIngredient.Id ?? 0L,
                MenuItemId = menuItem.Id,
                MenuItemName = menuItem.Title,
                StockIngredientId = stockIngredient.Id,
                StockIngredientName = stockIngredient.Name,
                QuantityUseToOrder = menuItemIngredient.QuantityUseToOrder
            };

            return Result<AddMenuItemIngredientResponseDto>.Success(response);
        }
    }
}
