using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Restaurants.Menus;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class MenuBuilder : BaseEntityBuilder<Guid, Menu>
    {
        public static readonly Guid MenuId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");
        protected override void ConfigureEntity(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable("menus");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
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
                Id = MenuId,
                Name = "Teste",
                RestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a"),
                CreatedAt = new DateTime(2026, 1, 1),
                Active = true,
            });


        }
    }
}
