using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaizesDoNordeste.Domain.Core.Accounts;

namespace RaizesDoNordeste.Data.EntityBuilders
{
    internal sealed class UserRefreshTokenBuilder : BaseEntityBuilder<long, UserRefreshToken>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ToTable("user_refresh_tokens");

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(x => x.Token)
                .HasColumnName("token")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            builder.Property(x => x.Revoked)
                .HasColumnName("revoked")
                .IsRequired();

            builder.Property(x => x.RestaurantId)
                .HasColumnName("restaurant_id")
                .IsRequired();

            builder.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
