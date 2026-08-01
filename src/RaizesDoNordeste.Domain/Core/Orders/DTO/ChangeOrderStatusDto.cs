using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO;

public sealed class ChangeOrderStatusDto : IUseCaseRequest
{
    public Guid? OrderId { get; set; }
    public required OrderStatus Status { get; set; }
}

public sealed class OrderStatusChangeResponseDto : IUseCaseResponse
{
    public required Guid OrderId { get; set; }
    public required OrderStatus Status { get; set; }
    public Error? ErrorResponse { get; set; }
}
