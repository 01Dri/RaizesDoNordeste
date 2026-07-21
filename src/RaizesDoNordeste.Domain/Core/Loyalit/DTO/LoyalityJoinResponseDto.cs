using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Loyalit.DTO
{
    public class LoyalityJoinResponseDto : IUseCaseResponse
    {
        public Error? ErrorResponse { get ; set; }
    }
}
