using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Ingredients.Enums;
using RestauranteUni.Domain.Core.Stocks;

namespace RestauranteUni.Data.EntityBuilders
{
    internal class StockIngredientBuilder : BaseEntityBuilder<long, StockIngredient>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<StockIngredient> builder)
        {
            builder.ToTable("stock_ingredient");
            
            builder.HasIndex(x => x.PublicId)
                .IsUnique();

            builder.Property(x => x.PublicId)
                .HasColumnName("public_id")
                .IsRequired();
            
            builder.Property(x => x.Unit)
                .HasColumnName("unit")
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();
            

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity").IsRequired();

            builder.Property(x => x.StockId)
                .HasColumnName("stock_id")
                .IsRequired();

            builder.HasOne(x => x.Stock)
                .WithMany(x => x.Items).HasForeignKey(x => x.StockId);
            
            
            builder.Navigation(x => x.Stock);
            
            builder.HasData(new StockIngredient()
            {
                Id = 1,
                PublicId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Tomate",
                Unit = IngredientUnit.Kilogram,
                Quantity = 100,
                StockId = 1,
                CreatedAt = new DateTime(2026, 1, 1),
                Active = true,
            });
            
            builder.HasData(new StockIngredient()
            {
                Id = 2,
                PublicId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Alface",
                Unit = IngredientUnit.Kilogram,
                Quantity = 50,
                StockId = 1,
                CreatedAt = new DateTime(2026, 1, 1),
                Active = true,
            });

        }
    }
}
