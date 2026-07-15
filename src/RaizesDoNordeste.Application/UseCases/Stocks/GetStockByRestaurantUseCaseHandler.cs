using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                .Include(s => s.Restaurant)
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.RestaurantId == parameter.RestaurantId, cancellation);

            if (stock == null)
            {
                return Result<StockResponseDto>.FailureNotFound("Estoque da unidade não encontrado.");
            }

            var items = stock.Items
                .Select(i => new StockIngredientDto
                {
                    Id = i.Id,
                    PublicId = i.PublicId,
                    Name = i.Name,
                    Unit = i.Unit.ToString(),
                    Quantity = i.Quantity
                })
                .ToList();

            var response = new StockResponseDto
            {
                Id = stock.Id,
                PublicId = stock.PublicId,
                RestaurantId = stock.RestaurantId ?? System.Guid.Empty,
                RestaurantName = stock.Restaurant.Name,
                Items = items
            };

            return Result<StockResponseDto>.Success(response);
        }
    }
}
