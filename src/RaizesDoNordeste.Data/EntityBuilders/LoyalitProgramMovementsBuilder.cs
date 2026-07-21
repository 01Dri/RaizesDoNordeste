using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaizesDoNordeste.Domain.Core.Loyalit;

namespace RaizesDoNordeste.Data.EntityBuilders
{
    internal sealed class LoyalitProgramMovementsBuilder : IEntityTypeConfiguration<LoyalitProgramMovements>
    {
        public void Configure(EntityTypeBuilder<LoyalitProgramMovements> builder)
        {
            builder.ToTable("loyalit_program_movements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Points)
                .HasColumnName("points")
                .IsRequired();

            builder.Property(x => x.LoyalityProgramId)
                .HasColumnName("loyality_program_id")
                .IsRequired();

            builder.Property(x => x.MovementAt)
                .HasColumnName("movement_at")
                .IsRequired();

            builder.HasOne(x => x.LoyalitProgram)
                .WithMany(x => x.Movements)
                .HasForeignKey(x => x.LoyalityProgramId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
