using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Stocks
{
    public sealed class AddStockIngredientUseCaseHandler : IUseCaseHandler<AddStockIngredientDto, StockIngredientResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<AddStockIngredientDto> _validator;
        private readonly ICurrentUser _currentUser;

        public AddStockIngredientUseCaseHandler(ApplicationDbContext context, IValidator<AddStockIngredientDto> validator, ICurrentUser currentUser)
        {
            _context = context;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<StockIngredientResponseDto>> HandleAsync(AddStockIngredientDto parameter, CancellationToken cancellation = default)
        {
            // Guard 1: Validação do DTO
            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
                return validation.ToResultFailure<StockIngredientResponseDto>();

            // Guard 2: Validação do Restaurante
            var restaurantId = _currentUser.RestaurantId;
            if (restaurantId == Guid.Empty)
                return Result<StockIngredientResponseDto>.Failure(new Error("Restaurante do usuário não identificado."));

            // Obter ou criar o estoque da unidade
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
            var existingItem = stock.Items
                .FirstOrDefault(i => i.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

            StockIngredient ingredient;
            if (existingItem != null)
            {
                // Se já existe no estoque da unidade, adiciona a quantidade enviada
                existingItem.Quantity += parameter.Quantity;
                ingredient = existingItem;
            }
            else
            {
                // Se novo, adiciona ao estoque da unidade
                ingredient = new StockIngredient
                {
                    PublicId = Guid.NewGuid(),
                    Name = trimmedName,
                    Unit = parameter.Unit,
                    Quantity = parameter.Quantity,
                    StockId = stock.Id
                };
                await _context.StockIngredients.AddAsync(ingredient, cancellation);
            }

            await _context.SaveChangesAsync(cancellation);

            var response = new StockIngredientResponseDto
            {
                Id = ingredient.Id,
                PublicId = ingredient.PublicId,
                Name = ingredient.Name,
                Unit = ingredient.Unit.ToString(),
                Quantity = ingredient.Quantity
            };

            return Result<StockIngredientResponseDto>.Success(response, HttpStatusCode.Created);
        }
    }
}
