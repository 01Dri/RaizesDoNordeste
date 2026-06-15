using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography;
using RestauranteUni.Domain.Core.Menus;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class MenuBuilder : BaseEntityBuilder<long, Menu>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable("menus");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(x => x.PublicId)
                .IsUnique();

            builder.Property(x => x.PublicId)
                .HasColumnName("public_id")
                .IsRequired();


            builder.Property(x => x.RestaurantId)
                .HasColumnName("restaurant_id")
                .IsRequired();


            builder.HasOne(x => x.Restaurant)
                .WithOne(x => x.Menu)
                .HasForeignKey<Menu>(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Menu)
                .HasForeignKey(x => x.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Restaurant).AutoInclude();
            builder.Navigation(x => x.Items).AutoInclude();

            builder.HasData(new Menu
            {
                Id = 1,
                PublicId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8b"),
                Name = "Teste",
                RestaurantId = Guid.Parse("9A88024D-2618-4E25-87F5-35217F7A7C8A"),
                CreatedAt = new DateTime(2026, 1, 1),
                Active = true,
            });


        }
    }
}
