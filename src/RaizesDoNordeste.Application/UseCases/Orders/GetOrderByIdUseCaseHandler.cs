using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

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
                .Select(order => new OrderResponseDto()
                {
                    Id = order.PublicId,
                    RestaurantId = order.RestaurantId,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    AccountId = order.AccountId.GetValueOrDefault(),
                    AccountEmail = order.Account.Email.Value,
                    Status = order.Status,
                    Channel = order.Channel,
                    TotalPrice = order.TotalPrice,
                    Items = order.Items.Select(x => new OrderItemResponseDto
                    {
                        Id = x.Id.GetValueOrDefault(),
                        MenuId = x.MenuItem.Menu.PublicId,
                        MenuItemId = x.MenuItem.PublicId,
                        MenuItemName = x.MenuItem.Title,
                        UnitPrice = x.MenuItem.Price,
                        Quantity = x.Quantity
                    }).ToImmutableList()
                })
                .FirstOrDefaultAsync(o => o.Id == parameter.Id && o.RestaurantId == _currentUser.RestaurantId, cancellation);

            return order == null ? Result<OrderResponseDto>.FailureNotFound("Pedido não encontrado.") : Result<OrderResponseDto>.Success(order);
        }
    }
}
