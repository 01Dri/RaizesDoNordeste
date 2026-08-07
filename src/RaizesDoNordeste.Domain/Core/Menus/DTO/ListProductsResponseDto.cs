using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public class ListProductsResponseDto : IUseCaseResponse
    {
        public List<ProductResponseDto> Products { get; set; } = [];
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => Limit > 0 ? (int)Math.Ceiling((double)TotalItems / Limit) : 0;
        public Error? ErrorResponse { get; set; }
    }
}
