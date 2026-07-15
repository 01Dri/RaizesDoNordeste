using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Login
{
    public record LogoutRequestDto : IUseCaseRequest
    {
        public string RefreshToken { get; init; } = null!;
    }
}
