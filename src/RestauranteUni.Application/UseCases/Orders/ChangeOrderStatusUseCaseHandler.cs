using Microsoft.EntityFrameworkCore;
using RestauranteUni.Application.Patterns.Dispatchers;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Orders.DTO;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.UseCases.Orders;

public sealed class ChangeOrderStatusUseCaseHandler : IUseCaseHandler<ChangeOrderStatusDto, OrderStatusChangeResponseDto>
{

    private readonly IDispatcher<OrderStatus, Order> _orderStatusDispatcher;
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ChangeOrderStatusUseCaseHandler(IDispatcher<OrderStatus, Order> orderStatusDispatcher, ApplicationDbContext dbContext, ICurrentUser currentUser)
    {
        _orderStatusDispatcher = orderStatusDispatcher;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    /**
     * Criar um chains of responsability para cada status
     */
    public async Task<Result<OrderStatusChangeResponseDto>> HandleAsync(ChangeOrderStatusDto parameter, CancellationToken cancellation = default)
    {

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.PublicId == parameter.OrderId &&
                                                               x.RestaurantId == _currentUser.RestaurantId, cancellation);

        if (order == null)
        {
            return Result<OrderStatusChangeResponseDto>.FailureNotFound("Order not found.");
        }

        var result = _orderStatusDispatcher.Handle(parameter.Status, order);

        if (!result.IsSuccess)
        {
            return Result<OrderStatusChangeResponseDto>.Failure(result.ErrorData, result.StatusCode);
        }

        await _dbContext.SaveChangesAsync(cancellation);

        return Result<OrderStatusChangeResponseDto>.Success(new OrderStatusChangeResponseDto()
        {
            OrderId = order.PublicId,
            Status = order.Status
        });
    }
}