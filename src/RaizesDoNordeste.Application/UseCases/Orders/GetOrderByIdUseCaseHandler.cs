using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Orders
{
    public sealed class GetOrderByIdUseCaseHandler : IUseCaseHandler<GetOrderByIdQueryDto, OrderResponseDto>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public GetOrderByIdUseCaseHandler(ApplicationDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<Result<OrderResponseDto>> HandleAsync(GetOrderByIdQueryDto parameter, CancellationToken cancellation = default)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Account)
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                        .ThenInclude(m => m.Menu)
                .FirstOrDefaultAsync(o => o.PublicId == parameter.Id, cancellation);

            if (order == null)
            {
                return Result<OrderResponseDto>.FailureNotFound("Pedido não encontrado.");
            }

            if (order.RestaurantId != _currentUser.RestaurantId)
            {
                return Result<OrderResponseDto>.Failure(new Error("Você não tem permissão para visualizar este pedido."), HttpStatusCode.Forbidden);
            }

            var response = new OrderResponseDto
            {
                Id = order.PublicId,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                AccountId = order.AccountId.GetValueOrDefault(),
                AccountEmail = order.Account?.Email?.Value ?? "",
                Status = order.Status,
                Channel = order.Channel,
                TotalPrice = order.TotalPrice,
                Items = order.Items.Select(x => new OrderItemResponseDto
                {
                    Id = x.Id.GetValueOrDefault(),
                    MenuId = x.MenuItem?.Menu?.PublicId ?? System.Guid.Empty,
                    MenuItemId = x.MenuItem?.PublicId ?? System.Guid.Empty,
                    MenuItemName = x.MenuItem?.Title ?? "",
                    UnitPrice = x.MenuItem?.Price ?? 0,
                    Quantity = x.Quantity
                }).ToImmutableList()
            };

            return Result<OrderResponseDto>.Success(response);
        }
    }
}
