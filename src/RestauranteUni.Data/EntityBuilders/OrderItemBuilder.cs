using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Orders;

namespace RestauranteUni.Data.EntityBuilders;

public class OrderItemBuilder : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();
        
        builder.HasOne(x => x.MenuItem)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.MenuItemId);
        
        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();
        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId);

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity").IsRequired();
        builder.HasData(new OrderItem()
        {
            Id = 1,
            MenuItemId = 1,
            OrderId = 1,
        });
        builder.HasData(new OrderItem()
        {
            Id = 2,
            MenuItemId = 2,
            OrderId = 1,
        });
        
    }
}