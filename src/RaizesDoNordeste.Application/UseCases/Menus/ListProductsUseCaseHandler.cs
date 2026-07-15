using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class ListProductsUseCaseHandler : IUseCaseHandler<ListProductsResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUser _currentUser;

        public ListProductsUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<ListProductsResponseDto>> HandleAsync(CancellationToken cancellation = default)
        {
            var products = await _context.MenuItems
                .Include(i => i.Menu)
                .Where(i => i.Menu != null && i.Menu.RestaurantId == _currentUser.RestaurantId)
                .OrderBy(i => i.DisplayOrder)
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
                    MenuId = i.MenuId ?? 0L
                })
                .ToListAsync(cancellation);

            return Result<ListProductsResponseDto>.Success(new ListProductsResponseDto
            {
                Products = products
            });
        }
    }
}
