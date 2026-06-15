using Microsoft.EntityFrameworkCore;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Menus.DTO;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.UseCases.Restaurants.Menus
{
     public sealed class GetRestaurantMenuUseCase  : IUseCaseHandler<MenuResponseDto>
     {
         private readonly ICurrentUser _currentUser;
         private readonly ApplicationDbContext _context;
         public GetRestaurantMenuUseCase(ICurrentUser currentUser, ApplicationDbContext context)
         {
             _currentUser = currentUser;
             _context = context;
         }

         public async Task<Result<MenuResponseDto>> HandleAsync(CancellationToken cancellation = default)
         {
             var menu = await _context.Menus
                 .Include(x => x.Restaurant)
                 .Include(x => x.Items)
                 .FirstOrDefaultAsync(x => x.RestaurantId == _currentUser.RestaurantId, cancellation);
             if (menu == null)
             {
                 return Result<MenuResponseDto>.FailureNotFound("Menu not found.");
             }

             var items = menu.Items.OrderBy(x => x.DisplayOrder)
                 .Select(x => new MenuItemResponseDto()
             {
                 Title = x.Title,
                 Description = x.Description,
                 Price = x.Price,
                 ImageUrl = x.ImageUrl,
                 IsAvailable = x.IsAvailable,
                 PreparationTimeInMinutes = x.PreparationTimeInMinutes,
                 IsFeatured = x.IsFeatured
             }).ToList();

             return Result<MenuResponseDto>.Success(new MenuResponseDto()
             {
                 Name = menu.Name,
                 RestaurantName = menu.Restaurant!.Name,
                 Items =  items
             });
     }
    }
}
