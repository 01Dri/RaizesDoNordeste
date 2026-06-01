using RestauranteUni.Domain.Accounts.Roles;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Domain.Accounts.DTO
{
    public class CreateAccountUseCaseResponseDto 
        : IUseCaseResponse<long>
    {
        public long Id { get; set; }
        public Email Email { get; set; } = null!;
        public List<RoleType> Roles { get; set; } = [];
    }
}
