using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Menus;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class MenuItemIngredientBuilder : IEntityTypeConfiguration<MenuItemIngredient>
    {
        public void Configure(EntityTypeBuilder<MenuItemIngredient> builder)
        {
            builder.ToTable("menu_item_ingredient");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity").IsRequired();

            builder.Property(x => x.MenuItemId)
                .HasColumnName("menu_item_id");
            
            builder.HasOne(x => x.MenuItem)
                .WithMany(x => x.Ingredients)
                .HasForeignKey(x => x.MenuItemId);

            builder.Property(x => x.StockIngredientId)
                .HasColumnName("stock_ingredient_id");

            builder.HasOne(x => x.StockIngredient)
                .WithMany(m => m.MenuItemIngredients)
                .HasForeignKey(m => m.StockIngredientId);


            builder.Navigation(x => x.MenuItem);

            builder.Navigation(x => x.StockIngredient);
            
            builder.HasData(new MenuItemIngredient()
            {
                Id = 1,
                MenuItemId = 1,
                StockIngredientId = 1,
                Quantity = 0.5m,
            });
            
             builder.HasData(new MenuItemIngredient()
             {
                 Id = 2,
                 MenuItemId = 1,
                 StockIngredientId = 2,
                 Quantity = 0.2m,
             });
        }
    }
}
