using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Accounts.Roles;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class RoleBuilder : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.HasIndex(x => x.Id)
                .HasDatabaseName("ix_roles_type")
                .IsUnique();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasData(
                new Role { Id = RoleType.Customer, Name = nameof(RoleType.Customer) },
                new Role { Id = RoleType.Manager, Name = nameof(RoleType.Manager) },
                new Role { Id = RoleType.Admin, Name = nameof(RoleType.Admin) },
                new Role { Id = RoleType.Professional, Name = nameof(RoleType.Professional) },
                new Role { Id = RoleType.Owner, Name = nameof(RoleType.Owner) });
        }
    }
}
