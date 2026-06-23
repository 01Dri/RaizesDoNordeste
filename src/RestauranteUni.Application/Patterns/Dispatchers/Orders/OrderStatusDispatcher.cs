using System.Net;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.Patterns.Dispatchers.Orders;

public sealed class OrderStatusDispatcher : IDispatcher<OrderStatus, Order>
{
    private readonly Dictionary<OrderStatus, IOrderStatusHandler> _handlers;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ICurrentUser _currentUser;

    public OrderStatusDispatcher(IEnumerable<IOrderStatusHandler> handlers, ICurrentUser currentUser, ApplicationDbContext applicationDbContext)
    {
        _currentUser = currentUser;
        _applicationDbContext = applicationDbContext;
        _handlers = handlers.ToDictionary(x => x.Status);
    }

    public Result Handle(OrderStatus parameter1, Order parameter2)
    {
        if (_handlers.TryGetValue(parameter1, out var handler))
        {
            return handler.Handle(parameter2, _currentUser);
        }

        return Result.Failure(new Error()
        {
            Message = "This order status not have handler. Please contact a support."
            
        }, HttpStatusCode.InternalServerError);
    }
}