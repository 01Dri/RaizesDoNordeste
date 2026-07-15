using RaizesDoNordeste.Domain.UseCases;
using System;

namespace RaizesDoNordeste.Domain.Core.Stocks.DTO
{
    public record GetStockByRestaurantQueryDto(Guid RestaurantId) : IUseCaseRequest;
}
