using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.Domain.Core.Accounts.DTO;

public record CreateAccountDto : IUseCaseRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required DateTime BirthDate { get; set; }
}