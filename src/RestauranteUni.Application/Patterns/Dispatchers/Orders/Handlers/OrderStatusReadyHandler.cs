using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.Patterns.Dispatchers.Orders.Handlers;

public sealed class OrderStatusReadyHandler : IOrderStatusHandler
{

    public OrderStatus Status { get; set; } = OrderStatus.Ready;
    public Task<Result> HandleAsync(Order order, ICurrentUser user, ApplicationDbContext context)
    {
        if (!user.InRole(RoleType.Professional))
        {
            return Task.FromResult(Result.Failure(new Error("Usuário não possui permissão")));
        }
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