using System.Collections.Immutable;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO;

public class OrderResponseDto : IUseCaseResponse
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long AccountId { get; set; }

    public string AccountEmail { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; }
    public OrderChannel Channel { get; set; }

    public IImmutableList<OrderItemResponseDto> Items { get; set; } = [];
    
    public Error? ErrorResponse { get; set; }
}

public class OrderItemResponseDto
{
    public long Id { get; set; }

    public Guid MenuId { get; set; }

    public Guid MenuItemId { get; set; }

    public string MenuItemName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
