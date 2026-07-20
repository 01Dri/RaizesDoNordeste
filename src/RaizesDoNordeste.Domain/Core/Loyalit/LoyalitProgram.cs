using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Restaurants;

namespace RaizesDoNordeste.Domain.Core.Loyalit
{
    public class LoyalitProgram
    {
        public long? Id { get; set; }
        public bool Active { get; set; }
        public DateTime JoinedAt { get; set; }  = DateTime.UtcNow;
        public DateTime? LeavedAt { get; set; }
        public int Points { get; set; }
        public required long AccountId { get; set; }
        public  virtual required Account Account { get; set; }
        public required Guid RestaurantId { get; set; }
        public required Restaurant Restaurant { get; set; }
        public virtual List<LoyalitProgramMovements> Movements { get; set; } = [];
    }
}
