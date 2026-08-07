using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Stocks
{
    public sealed class RegisterStockMovementUseCaseHandler : IUseCaseHandler<StockMovementRequestDto, StockMovementResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<StockMovementRequestDto> _validator;
        private readonly ICurrentUser _currentUser;

        public RegisterStockMovementUseCaseHandler(ApplicationDbContext context, IValidator<StockMovementRequestDto> validator, ICurrentUser currentUser)
        {
            _context = context;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<StockMovementResponseDto>> HandleAsync(StockMovementRequestDto parameter, CancellationToken cancellation = default)
        {
            var validationResult = await _validator.ValidateAsync(parameter, cancellation);
            if (validationResult.ContainsErrors())
            {
                return validationResult.ToResultFailure<StockMovementResponseDto>();
            }

            StockIngredient? item = null;

            if (parameter.PublicStockIngredientId.HasValue && parameter.PublicStockIngredientId.Value != Guid.Empty)
            {
                item = await _context.StockIngredients
                    .Include(si => si.Stock)
                    .Where(si => si.Stock.RestaurantId == _currentUser.RestaurantId)
                    .FirstOrDefaultAsync(si => si.PublicId == parameter.PublicStockIngredientId.Value, cancellation);
            }
            else if (parameter.StockIngredientId > 0)
            {
                item = await _context.StockIngredients
                    .Include(si => si.Stock)
                    .Where(si => si.Stock.RestaurantId == _currentUser.RestaurantId)
                    .FirstOrDefaultAsync(si => si.Id == parameter.StockIngredientId, cancellation);
            }

            if (item == null)
            {
                return Result<StockMovementResponseDto>.FailureNotFound("Ingrediente de estoque não encontrado.");
            }

            switch (parameter.Type)
            {
                case StockMovementType.Entry:
                    item.Quantity += parameter.Quantity;
                    break;
                case StockMovementType.Consumption:
                case StockMovementType.Loss:
                    if (item.Quantity < parameter.Quantity)
                    {
                        return Result<StockMovementResponseDto>.Failure(new Error("Estoque insuficiente para esta saída."));
                    }
                    item.Quantity -= parameter.Quantity;
                    break;
                case StockMovementType.Adjustment:
                    item.Quantity += parameter.Quantity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var movement = new StockIngredientMovement
            {
                StockIngredientId = item.Id,
                Quantity = parameter.Quantity,
                Type = parameter.Type,
                Description = parameter.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _context.StockIngredientMovements.AddAsync(movement, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var response = new StockMovementResponseDto
            {
                Id = movement.Id ?? 0L,
                StockIngredientId = movement.StockIngredientId,
                PublicStockIngredientId = item.PublicId,
                IngredientName = item.Name,
                QuantityMoved = movement.Quantity,
                NewStockQuantity = item.Quantity,
                Type = movement.Type.ToString(),
                Description = movement.Description,
                CreatedAt = movement.CreatedAt
            };

            return Result<StockMovementResponseDto>.Success(response);
        }
    }
}
