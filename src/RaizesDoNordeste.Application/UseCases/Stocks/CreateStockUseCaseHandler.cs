using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Stocks
{
    public sealed class CreateStockUseCaseHandler : IUseCaseHandler<CreateStockRequestDto, StockResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateStockRequestDto> _validator;

        public CreateStockUseCaseHandler(ApplicationDbContext context, IValidator<CreateStockRequestDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result<StockResponseDto>> HandleAsync(CreateStockRequestDto parameter, CancellationToken cancellation = default)
        {
            var validationResult = await _validator.ValidateAsync(parameter, cancellation);
            if (validationResult.ContainsErrors())
            {
                return validationResult.ToResultFailure<StockResponseDto>();
            }

            var restaurant = await _context.Restaurants
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .FirstOrDefaultAsync(r => r.Id == parameter.RestaurantId, cancellation);

            if (restaurant == null)
            {
                return Result<StockResponseDto>.FailureNotFound("Restaurante não encontrado.");
            }

            var stockExists = await _context.Stocks
                .AnyAsync(s => s.RestaurantId == parameter.RestaurantId, cancellation);

            if (stockExists)
            {
                return Result<StockResponseDto>.Failure(
                    new Error("Já existe um estoque cadastrado para este restaurante."),
                    HttpStatusCode.Conflict
                );
            }

            var stock = new Stock
            {
                PublicId = Guid.NewGuid(),
                RestaurantId = restaurant.Id,
                Items = parameter.Items?.Select(item => new StockIngredient
                {
                    PublicId = Guid.NewGuid(),
                    Name = item.Name,
                    Unit = item.Unit,
                    Quantity = item.Quantity
                }).ToList() ?? []
            };

            await _context.Stocks.AddAsync(stock, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var response = new StockResponseDto
            {
                Id = stock.Id,
                PublicId = stock.PublicId,
                RestaurantId = restaurant.Id,
                RestaurantName = restaurant.Name,
                Items = stock.Items.Select(i => new StockIngredientDto
                {
                    Id = i.Id,
                    PublicId = i.PublicId,
                    Name = i.Name,
                    Unit = i.Unit.ToString(),
                    Quantity = i.Quantity
                }).ToList()
            };

            return Result<StockResponseDto>.Success(response);
        }
    }
}
