using System.Linq;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class GetProductByIdUseCaseHandler : IUseCaseHandler<GetProductByIdQueryDto, ProductResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUser _currentUser;

        public GetProductByIdUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<ProductResponseDto>> HandleAsync(GetProductByIdQueryDto parameter, CancellationToken cancellation = default)
        {
            var item = await _context.MenuItems
                .Include(x => x.Ingredients)
                    .ThenInclude(ing => ing.StockIngredient)
                .FirstOrDefaultAsync(x => x.Id == parameter.Id && x.Menu != null && x.Menu.RestaurantId == _currentUser.RestaurantId, cancellation);

            if (item == null)
            {
                return Result<ProductResponseDto>.FailureNotFound("Produto não encontrado.");
            }

            var response = new ProductResponseDto
            {
                Id = item.Id,
                PublicId = item.PublicId,
                Title = item.Title,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                IsAvailable = item.IsAvailable,
                DisplayOrder = item.DisplayOrder,
                PreparationTimeInMinutes = item.PreparationTimeInMinutes,
                IsFeatured = item.IsFeatured,
                MenuId = item.MenuId ?? 0L,
                Ingredients = item.Ingredients.Select(ing => new ProductIngredientResponseDto
                {
                    Id = ing.Id ?? 0L,
                    StockIngredientId = ing.StockIngredientId ?? 0L,
                    StockIngredientName = ing.StockIngredient?.Name ?? string.Empty,
                    QuantityUseToOrder = ing.QuantityUseToOrder
                }).ToList()
            };

            return Result<ProductResponseDto>.Success(response);
        }
    }
}
