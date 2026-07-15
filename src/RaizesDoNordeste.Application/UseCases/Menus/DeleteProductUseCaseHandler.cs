using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
            var item = await _context.MenuItems
                .Include(i => i.Menu)
                .FirstOrDefaultAsync(i => i.Id == parameter.Id, cancellation);

            if (item == null)
            {
                return Result<DeleteProductResponseDto>.FailureNotFound("Produto não encontrado.");
            }

            if (item.Menu == null || item.Menu.RestaurantId != _currentUser.RestaurantId)
            {
                return Result<DeleteProductResponseDto>.Failure(new Error("Você não tem permissão para remover este produto."), HttpStatusCode.Forbidden);
            }

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync(cancellation);

            return Result<DeleteProductResponseDto>.Success(new DeleteProductResponseDto { Success = true });
        }
    }
}
