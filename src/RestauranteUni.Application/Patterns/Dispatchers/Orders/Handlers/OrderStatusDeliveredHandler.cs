using Microsoft.EntityFrameworkCore;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Orders;
using RestauranteUni.Domain.Core.Users;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.Patterns.Dispatchers.Orders.Handlers;

public sealed class OrderStatusDeliveredHandler : IOrderStatusHandler
{
    public OrderStatus Status { get; } = OrderStatus.Delivered;
    public async Task<Result> HandleAsync(Order order, ICurrentUser user, ApplicationDbContext context)
    {
        if (!user.InRole(RoleType.Professional))
        {
            return Result.Failure(new Error("Usuário não possui permissão."));
        }
        if (order.Status == Status)  
        {
            return Result.Success();
        }
        if (order.Status != OrderStatus.Ready)
        {
            return Result.Failure(new Error("O pedido precisa estar pronto."));
        }
        var paymentOrderStatus = await context.PaymentOrders
            .Include(s => s.Payment)
            .Select(x => new
            {
                x.OrderId,
                x.Payment.Status
            }).FirstOrDefaultAsync(x => x.OrderId == order.Id);
        
        if (paymentOrderStatus == null)
        {
            return Result.Failure(new Error("Não foi possivel encontrar o pagamento desse pedido."));
        }
        switch (paymentOrderStatus.Status)
        {
            case PaymentStatus.Paid:
                return Result.Failure(new Error("O pedido ainda não foi pago."));
            case PaymentStatus.Canceled:
                return Result.Failure(new Error("O pagamento do pedido foi cancelado"));
            default:
                order.Status = OrderStatus.Delivered;
                return Result.Success();
        }
    }
}