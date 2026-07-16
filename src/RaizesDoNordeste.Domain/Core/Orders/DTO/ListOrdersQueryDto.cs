using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO
{
    public class ListOrdersQueryDto : IUseCaseRequest
    {
        public OrderStatus? Status { get; set; }
        public OrderChannel? Channel { get; set; }

        public ListOrdersQueryDto() { }

        public ListOrdersQueryDto(OrderStatus? status, OrderChannel? channel = null)
        {
            Status = status;
            Channel = channel;
        }
    }
}
