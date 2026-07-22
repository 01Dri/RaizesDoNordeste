using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Loyalit.DTO
{
    public class LoyalityLeaveRequestDto : IUseCaseRequest
    {
        public long? CustomerAccountId { get; set; }
    }
}
