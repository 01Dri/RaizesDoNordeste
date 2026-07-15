using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Login
{
    public record RefreshRequestDto : IUseCaseRequest
    {
        public string RefreshToken { get; init; } = null!;
    }
}
