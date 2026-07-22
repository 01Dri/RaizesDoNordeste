using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Loyalit.DTO
{
    public class LoyalityLeaveResponseDto : IUseCaseResponse
    {
        public Error? ErrorResponse { get; set; }
    }
}
