using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Login
{
    public record LogoutResponseDto : IUseCaseResponse
    {
        public bool Success { get; init; }
        public Error? ErrorResponse { get; set; }
    }
}
