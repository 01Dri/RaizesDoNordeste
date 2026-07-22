using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Loyalit.DTO
{
    public class LoyalityJoinRequestDto : IUseCaseRequest
    {
        public long CustomerAccountId { get; set; }
    }
}
