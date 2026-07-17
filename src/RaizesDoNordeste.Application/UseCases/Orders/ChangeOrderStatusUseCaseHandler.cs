using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Patterns.Dispatchers;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Orders;

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
            .FirstOrDefaultAsync(x => 
                x.PublicId == parameter.OrderId && x.RestaurantId == _currentUser.RestaurantId, cancellation);

        if (order == null)
        {
            return Result<OrderStatusChangeResponseDto>.FailureNotFound("Pedido não encontrado.");
        }

        var result = await _orderStatusDispatcher.HandleAsync(parameter.Status, order);

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
