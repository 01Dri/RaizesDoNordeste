using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.Patterns.Dispatchers.Orders.Handlers;

public sealed class OrderStatusOrderStatusReadyHandler : IOrderStatusHandler
{

    public OrderStatus Status { get; set; } = OrderStatus.Ready;
    public  Result Handle(Order order, ICurrentUser user)
    {
        try
        {
            if (!user.InRole(RoleType.Professional))
            {
                return Result.Failure(new Error("Usuário não possui permissão"));
            }
            if (order.Status == Status)  
            {
                return Result.Success();
            }

            var currentStatus = order.Status;
            if (currentStatus != OrderStatus.Chicken)
            {
                return Result.Failure(new Error("O pedido precisa estar no status de cozinha."));
            }
            order.Status = OrderStatus.Ready;
            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(new Error()
            {
                Message = exception.Message
            });
        }
    }
}