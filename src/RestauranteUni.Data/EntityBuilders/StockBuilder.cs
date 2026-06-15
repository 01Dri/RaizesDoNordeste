using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Stocks;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class StockBuilder : BaseEntityBuilder<long, Stock>
    {
        private readonly Guid StockId  = Guid.Parse("00000000-0000-0000-0000-000000000001");
        protected override void ConfigureEntity(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("stock");

            builder.HasIndex(x => x.PublicId)
                .IsUnique();

            builder.Property(x => x.PublicId)
                .HasColumnName("public_id")
                .IsRequired();


            builder.Property(x => x.RestaurantId)
                .HasColumnName("restaurant_id");

            builder.HasOne(x => x.Restaurant)
                .WithOne(s => s.Stock)
                .HasForeignKey<Stock>(x => x.RestaurantId);

            builder.Navigation(x => x.Restaurant);
            builder.Navigation(x => x.Items);
            
            builder.HasData(new Stock()
            {
                Id = 1,
                PublicId = StockId,
                RestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a"),
                CreatedAt = new DateTime(2026, 1, 1),
                Active = true,
            });
        }
    }
}
