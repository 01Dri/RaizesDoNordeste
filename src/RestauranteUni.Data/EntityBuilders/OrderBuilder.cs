using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Orders;

namespace RestauranteUni.Data.EntityBuilders;

public class OrderBuilder : BaseEntityBuilder<long, Order>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        
        builder.Property(x => x.PublicId)
            .HasColumnName("public_id").IsRequired();

        builder.Property(x => x.AccountId)
            .HasColumnName("account_id").IsRequired();
        
        builder.HasOne(x => x.Account)
            .WithMany(a => a.Orders)
            .HasForeignKey(x => x.AccountId);
        
        builder.Property(x => x.RestaurantId)
            .HasColumnName("restaurant_id").IsRequired();
        
        builder.HasOne(x => x.Restaurant)
            .WithMany(r => r.Orders)
            .HasForeignKey(x => x.RestaurantId);
        
        builder.HasMany(x => x.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Navigation(x => x.Items);
        
        builder.HasData(new Order()
        {
            Id = 1,
            PublicId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8c"),
            AccountId = 1,
            RestaurantId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a"),
            CreatedAt = new DateTime(2026, 1, 1),
            Active = true,
        });
    }
}