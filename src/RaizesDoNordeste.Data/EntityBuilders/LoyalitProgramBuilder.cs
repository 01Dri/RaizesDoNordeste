using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Loyalit;

namespace RaizesDoNordeste.Data.EntityBuilders
{
    internal sealed class LoyalitProgramBuilder : IEntityTypeConfiguration<LoyalitProgram>
    {
        public void Configure(EntityTypeBuilder<LoyalitProgram> builder)
        {
            builder.ToTable("loyalit_programs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.HasIndex(x => new { x.AccountId, x.RestaurantId })
                .HasDatabaseName("ix_loyalit_programs_account_restaurant")
                .IsUnique();

            builder.Property(x => x.Active)
                .HasColumnName("active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.JoinedAt)
                .HasColumnName("joined_at")
                .IsRequired();

            builder.Property(x => x.LeavedAt)
                .HasColumnName("leaved_at");

            builder.Property(x => x.Points)
                .HasColumnName("points")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(x => x.RestaurantId)
                .HasColumnName("restaurant_id")
                .IsRequired();

            builder.HasOne(x => x.Account)
                .WithOne(x => x.LoyalitProgram)
                .HasForeignKey<LoyalitProgram>(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Restaurant)
                .WithOne(x => x.LoyalitProgram)
                .HasForeignKey<LoyalitProgram>(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
