using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.Domain.Core.Login;

public record LoginResponseDto : IUseCaseResponse
{
    public LoginResponseDto(string token)
    {
        Token = token;
    }

    public string Token { get; set; }
}