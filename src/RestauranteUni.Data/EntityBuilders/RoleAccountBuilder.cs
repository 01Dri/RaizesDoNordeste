using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Accounts.Roles;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class RoleAccountBuilder : IEntityTypeConfiguration<RoleAccount>
    {
        public void Configure(EntityTypeBuilder<RoleAccount> builder)
        {
            builder.ToTable("role_accounts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(x => x.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId);

            builder.Property(x => x.RoleStatus)
                .HasColumnName("role_status")
                .IsRequired();

            builder.HasOne(x => x.Account)
                .WithMany(x => x.RoleAccounts)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
