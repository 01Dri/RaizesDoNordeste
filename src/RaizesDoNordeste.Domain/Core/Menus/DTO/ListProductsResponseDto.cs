using RaizesDoNordeste.Domain.UseCases;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record ListProductsResponseDto : IUseCaseResponse
    {
        public List<ProductResponseDto> Products { get; init; } = [];
        public Error? ErrorResponse { get; set; }
    }
}
