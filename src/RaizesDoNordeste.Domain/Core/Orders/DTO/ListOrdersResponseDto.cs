using RaizesDoNordeste.Domain.UseCases;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO
{
    public class ListOrdersResponseDto : IUseCaseResponse
    {
        public List<OrderResponseDto> Orders { get; set; } = [];
        public Error? ErrorResponse { get; set; }
    }
}
