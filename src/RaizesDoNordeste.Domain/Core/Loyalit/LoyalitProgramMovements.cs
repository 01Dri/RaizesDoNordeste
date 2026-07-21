namespace RaizesDoNordeste.Domain.Core.Loyalit
{
    public class LoyalitProgramMovements
    {
        public long? Id { get; set; }
        public required LoyalitProgramMovementType Type { get; set; }
        public int Points { get; set; }
        public required long LoyalityProgramId { get; set; }
        public virtual required LoyalitProgram LoyalitProgram { get; set; }
        public required DateTime MovementAt { get; set; }
    }
}
