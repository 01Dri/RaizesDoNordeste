using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Stocks
{
    public sealed class GetStockByRestaurantUseCaseHandler : IUseCaseHandler<GetStockByRestaurantQueryDto, StockResponseDto>
    {
        private readonly ApplicationDbContext _context;

        public GetStockByRestaurantUseCaseHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<StockResponseDto>> HandleAsync(GetStockByRestaurantQueryDto parameter, CancellationToken cancellation = default)
        {
            var stock = await _context.Stocks
                .Select(x => new StockResponseDto()
                {
                    Id = x.Id,
                    PublicId = x.PublicId,
                    RestaurantId = x.RestaurantId ?? Guid.Empty,
                    RestaurantName = x.Restaurant.Name,
                    Items = x.Items.Select(i => new StockIngredientDto()
                    {
                        Id = i.Id,
                        PublicId = i.PublicId,
                        Name = i.Name,
                        Unit = i.Unit.ToString(),
                        Quantity = i.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync(s => s.RestaurantId == parameter.RestaurantId, cancellation);

            return stock == null ? Result<StockResponseDto>.FailureNotFound("Estoque da unidade não encontrado.") : Result<StockResponseDto>.Success(stock);
        }
    }
}
