using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

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
                .FirstOrDefaultAsync(s => s.RestaurantId == _currentUser.RestaurantId, cancellation);

            return stock == null ? Result<StockResponseDto>.FailureNotFound("Estoque do restaurante não encontrado.") : Result<StockResponseDto>.Success(stock);
        }
    }
}
