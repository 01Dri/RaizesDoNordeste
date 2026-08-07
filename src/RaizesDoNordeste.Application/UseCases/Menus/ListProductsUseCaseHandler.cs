using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class ListProductsUseCaseHandler : IUseCaseHandler<ListProductsQueryDto, ListProductsResponseDto>, IUseCaseHandler<ListProductsResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUser _currentUser;

        public ListProductsUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public Task<Result<ListProductsResponseDto>> HandleAsync(CancellationToken cancellation = default)
            => HandleAsync(new ListProductsQueryDto(1, 10), cancellation);

        public async Task<Result<ListProductsResponseDto>> HandleAsync(ListProductsQueryDto query, CancellationToken cancellation = default)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var limit = query.Limit < 1 ? 10 : (query.Limit > 100 ? 100 : query.Limit);

            var baseQuery = _context.MenuItems
                .Where(i => i.Menu != null && i.Menu.RestaurantId == _currentUser.RestaurantId);

            var totalItems = await baseQuery.CountAsync(cancellation);

            var products = await baseQuery
                .OrderBy(i => i.DisplayOrder)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(i => new ProductResponseDto
                {
                    Id = i.Id,
                    PublicId = i.PublicId,
                    Title = i.Title,
                    Description = i.Description,
                    Price = i.Price,
                    ImageUrl = i.ImageUrl,
                    IsAvailable = i.IsAvailable,
                    DisplayOrder = i.DisplayOrder,
                    PreparationTimeInMinutes = i.PreparationTimeInMinutes,
                    IsFeatured = i.IsFeatured,
                    MenuId = i.MenuId ?? 0L,
                    Ingredients = i.Ingredients.Select(ing => new ProductIngredientResponseDto
                    {
                        Id = ing.Id ?? 0L,
                        StockIngredientId = ing.StockIngredientId ?? 0L,
                        StockIngredientName = ing.StockIngredient.Name,
                        QuantityUseToOrder = ing.QuantityUseToOrder
                    }).ToList()
                })
                .ToListAsync(cancellation);

            return Result<ListProductsResponseDto>.Success(new ListProductsResponseDto
            {
                Products = products,
                Page = page,
                Limit = limit,
                TotalItems = totalItems
            });
        }
    }
}
