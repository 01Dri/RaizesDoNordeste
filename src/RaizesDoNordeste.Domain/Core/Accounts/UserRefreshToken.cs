using System;

namespace RaizesDoNordeste.Domain.Core.Accounts
{
    public class UserRefreshToken : BaseDomain<long>
    {
        public long AccountId { get; set; }
        public virtual Account Account { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; }
        public Guid RestaurantId { get; set; }
    }
}
