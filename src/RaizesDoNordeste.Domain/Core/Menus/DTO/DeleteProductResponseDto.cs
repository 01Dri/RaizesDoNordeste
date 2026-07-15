using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record DeleteProductResponseDto : IUseCaseResponse
    {
        public bool Success { get; init; }
        public Error? ErrorResponse { get; set; }
    }
}
