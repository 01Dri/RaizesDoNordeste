using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.Domain.Core.Orders.DTO;

public sealed class ChangeOrderStatusDto : IUseCaseRequest
{
    public required Guid OrderId { get; set; }
    public required OrderStatus Status { get; set; }
}

public sealed class OrderStatusChangeResponseDto : IUseCaseResponse
{
    public required Guid OrderId { get; set; }
    public required OrderStatus Status { get; set; }
    public Error? ErrorResponse { get; set; }
}