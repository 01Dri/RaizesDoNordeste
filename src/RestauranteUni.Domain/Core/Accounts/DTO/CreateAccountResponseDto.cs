using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Domain.Core.Accounts.DTO;

public class CreateAccountUseCaseResponseDto
    : IUseCaseResponse<long>
{
    public Email Email { get; set; } = null!;
    public List<RoleType> Roles { get; set; } = [];
    public long Id { get; set; }
}