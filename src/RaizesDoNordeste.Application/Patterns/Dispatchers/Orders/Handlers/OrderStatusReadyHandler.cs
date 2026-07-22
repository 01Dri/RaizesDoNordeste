using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.Patterns.Dispatchers.Orders.Handlers;

public sealed class OrderStatusReadyHandler : IOrderStatusHandler
{

    public OrderStatus Status { get; set; } = OrderStatus.Ready;
    public Task<Result> HandleAsync(Order order, ICurrentUser user, ApplicationDbContext context)
    {
        if (order.Status == Status)  
        {
            return Task.FromResult(Result.Success());
        }

        var currentStatus = order.Status;
        if (currentStatus != OrderStatus.Chicken)
        {
            return Task.FromResult(Result.Failure(new Error("O pedido precisa estar no status de cozinha.")));
        }
        
        order.Status = OrderStatus.Ready;
        return Task.FromResult(Result.Success());
    }
}
