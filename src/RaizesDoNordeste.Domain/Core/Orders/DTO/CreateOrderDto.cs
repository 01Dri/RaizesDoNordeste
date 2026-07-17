using System.Text.Json.Serialization;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO;

public class CreateOrderDto : IUseCaseRequest
{
    [JsonPropertyName("canalPedido")]
    public OrderChannel Channel { get; set; }

    public List<CreateOrderItemDto> Items { get; set; } = [];
}

public class CreateOrderItemDto
{
    public Guid PublicMenuItemId { get; set; }

    public decimal Quantity { get; set; }
}
