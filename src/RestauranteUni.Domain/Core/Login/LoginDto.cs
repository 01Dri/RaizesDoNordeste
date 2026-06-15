using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.Domain.Core.Login;

public record LoginDto(string Email, string Password, Guid RestaurantId) : IUseCaseRequest;