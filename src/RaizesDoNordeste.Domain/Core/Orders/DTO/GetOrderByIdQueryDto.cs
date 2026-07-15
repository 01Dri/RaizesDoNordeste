using RaizesDoNordeste.Domain.UseCases;
using System;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO
{
    public record GetOrderByIdQueryDto(Guid Id) : IUseCaseRequest;
}
