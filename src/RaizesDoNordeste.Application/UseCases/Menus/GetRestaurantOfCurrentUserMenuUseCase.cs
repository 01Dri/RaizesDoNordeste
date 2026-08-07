using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Restaurants.Menus
{
     public sealed class GetRestaurantOfCurrentUserMenuUseCase  : IUseCaseHandler<MenuResponseDto>
     {
         private readonly ICurrentUser _currentUser;
         private readonly ApplicationDbContext _context;
         public GetRestaurantOfCurrentUserMenuUseCase(ICurrentUser currentUser, ApplicationDbContext context)
         {
             _currentUser = currentUser;
             _context = context;
         }

         public async Task<Result<MenuResponseDto>> HandleAsync(CancellationToken cancellation = default)
         {
             var menu = await _context.Menus
                 .Where(x => x.RestaurantId == _currentUser.RestaurantId)
                 .Select(x => new MenuResponseDto
                 {
                     Id = x.Id,
                     Name = x.Name,
                     RestaurantName = x.Restaurant.Name,
                     RestaurantId = x.RestaurantId.Value,
                     Items = x.Items
                         .Where(i => i.IsAvailable)
                         .OrderBy(i => i.DisplayOrder)
                         .Select(i => new MenuItemResponseDto
                         {
                             PublicId = i.PublicId,
                             Title = i.Title,
                             Description = i.Description,
                             Price = i.Price,
                             ImageUrl = i.ImageUrl,
                             IsAvailable = i.IsAvailable,
                             PreparationTimeInMinutes = i.PreparationTimeInMinutes,
                             IsFeatured = i.IsFeatured,
                             DisplayOrder = i.DisplayOrder
                         })
                         .ToList()
                 })
                 .FirstOrDefaultAsync(cancellation);
             // Por enquanto, retornamos apenas um menu por restaurant
             return menu == null ? Result<MenuResponseDto>.FailureNotFound("Cardápio não encontrado.") 
                 : Result<MenuResponseDto>.Success(menu);
         }
    }
}

