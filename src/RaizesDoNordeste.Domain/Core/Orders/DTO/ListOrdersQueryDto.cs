using System.Text.Json.Serialization;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO
{
    public class ListOrdersQueryDto : IUseCaseRequest
    {
        public OrderStatus? Status { get; set; }

        [JsonPropertyName("canalPedido")]
        public OrderChannel? Channel { get; set; }

        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;

        public ListOrdersQueryDto() { }

        public ListOrdersQueryDto(OrderStatus? status, OrderChannel? channel = null, int page = 1, int limit = 10)
        {
            Status = status;
            Channel = channel;
            Page = page < 1 ? 1 : page;
            Limit = limit < 1 ? 10 : (limit > 100 ? 100 : limit);
        }
    }
}
