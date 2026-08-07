using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Orders.DTO
{
    public class ListOrdersResponseDto : IUseCaseResponse
    {
        public List<OrderResponseDto> Orders { get; set; } = [];
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => Limit > 0 ? (int)Math.Ceiling((double)TotalItems / Limit) : 0;
        public Error? ErrorResponse { get; set; }
    }
}
