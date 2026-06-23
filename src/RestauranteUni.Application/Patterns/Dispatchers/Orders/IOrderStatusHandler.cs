using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.Patterns.Dispatchers.Orders;

public interface IOrderStatusHandler
{
    OrderStatus Status { get; }
    public Result Handle(Order order, ICurrentUser user);
}