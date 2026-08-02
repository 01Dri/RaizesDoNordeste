using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Menus
{
    public sealed class DeleteProductUseCaseHandler : IUseCaseHandler<DeleteProductDto, DeleteProductResponseDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUser _currentUser;

        public DeleteProductUseCaseHandler(ApplicationDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<DeleteProductResponseDto>> HandleAsync(DeleteProductDto parameter, CancellationToken cancellation = default)
        {
            var rowsAffected = await _context.MenuItems
                .Where(i => i.Id == parameter.Id &&
                            i.Menu.RestaurantId == _currentUser.RestaurantId)
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(i => i.Active, false),
                    cancellation);
            return rowsAffected == 0 ? Result<DeleteProductResponseDto>.FailureNotFound("Produto não encontrado.") : Result<DeleteProductResponseDto>.Success(new DeleteProductResponseDto { Success = true });
        }
    }
}
