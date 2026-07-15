using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO
{
    public record ListOrdersQueryDto(OrderStatus? Status) : IUseCaseRequest;
}
