using System.Text.Json.Serialization;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Loyalit.DTO
{
    public class LoyalityJoinRequestDto : IUseCaseRequest
    {
        public long CustomerAccountId { get; set; }

        [JsonPropertyName("accountId")]
        public long AccountId
        {
            get => CustomerAccountId;
            set => CustomerAccountId = value;
        }
    }
}
