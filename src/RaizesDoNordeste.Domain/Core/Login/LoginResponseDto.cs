using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Login;

public record LoginResponseDto : IUseCaseResponse
{
    public LoginResponseDto(string token)
    {
        Token = token;
    }

    public LoginResponseDto(string token, string refreshToken)
    {
        Token = token;
        RefreshToken = refreshToken;
    }

    public string Token { get; set; }
    public string? RefreshToken { get; set; }
    public Error? ErrorResponse { get; set; }
}
