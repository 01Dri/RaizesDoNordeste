using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Orders
{
    public sealed class ListOrdersUseCaseHandler : IUseCaseHandler<ListOrdersQueryDto, ListOrdersResponseDto>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public ListOrdersUseCaseHandler(ApplicationDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<Result<ListOrdersResponseDto>> HandleAsync(ListOrdersQueryDto parameter, CancellationToken cancellation = default)
        {
            var query = _dbContext.Orders
                .Include(o => o.Account)
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                        .ThenInclude(m => m.Menu)
                .Where(o => o.RestaurantId == _currentUser.RestaurantId);

            if (parameter.Status.HasValue)
            {
                query = query.Where(o => o.Status == parameter.Status.Value);
            }

            if (parameter.Channel.HasValue)
            {
                query = query.Where(o => o.Channel == parameter.Channel.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(order => new OrderResponseDto
                {
                    Id = order.PublicId,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    AccountId = order.AccountId.GetValueOrDefault(),
                    AccountEmail = order.Account != null && order.Account.Email != null ? order.Account.Email.Value : "",
                    Status = order.Status,
                    Channel = order.Channel,
                    TotalPrice = order.TotalPrice,
                    Items = order.Items.Select(x => new OrderItemResponseDto
                    {
                        Id = x.Id.GetValueOrDefault(),
                        MenuId = x.MenuItem != null && x.MenuItem.Menu != null ? x.MenuItem.Menu.PublicId : System.Guid.Empty,
                        MenuItemId = x.MenuItem != null ? x.MenuItem.PublicId : System.Guid.Empty,
                        MenuItemName = x.MenuItem != null ? x.MenuItem.Title : "",
                        UnitPrice = x.MenuItem != null ? x.MenuItem.Price : 0,
                        Quantity = x.Quantity
                    }).ToImmutableList()
                })
                .ToListAsync(cancellation);

            return Result<ListOrdersResponseDto>.Success(new ListOrdersResponseDto
            {
                Orders = orders
            });
        }
    }
}
