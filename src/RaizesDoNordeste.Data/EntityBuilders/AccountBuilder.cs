using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Data.EntityBuilders
{
    internal sealed class AccountBuilder : BaseEntityBuilder<long, Account>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("accounts");

            builder.Property(x => x.Email)
                .HasConversion(
                    email => email.Value,
                    value => new Email(value))
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .HasDatabaseName("ix_accounts_email")
                .IsUnique();

            builder.Property(x => x.Password)
                .HasColumnName("password")
                .HasMaxLength(255)
                .IsRequired();
            
            builder.Navigation(x => x.RoleAccounts);
            builder.Navigation(x => x.Orders);
            
        }
    }
}

