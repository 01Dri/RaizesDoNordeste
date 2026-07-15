using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Stocks
{
    public sealed class GetStockOfCurrentUserUseCaseHandler : IUseCaseHandler<StockResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUser _currentUser;

        public GetStockOfCurrentUserUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<StockResponseDto>> HandleAsync(CancellationToken cancellation = default)
        {
            var stock = await _context.Stocks
                .Include(s => s.Restaurant)
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.RestaurantId == _currentUser.RestaurantId, cancellation);

            if (stock == null)
            {
                return Result<StockResponseDto>.FailureNotFound("Estoque do restaurante não encontrado.");
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
