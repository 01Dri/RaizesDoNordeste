using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Payments;

namespace RestauranteUni.Data.EntityBuilders;

public class PaymentOrderBuilder : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        
        builder.ToTable("payment_order");
        
        builder.Property(x => x.Id)
            .HasColumnName("id");
        builder.HasKey(x => x.Id);


        builder.Property(x => x.OrderId)
            .HasColumnName("order_id");
        
        builder.HasOne(x => x.Order)
            .WithOne(X => X.PaymentOrder)
            .HasForeignKey<PaymentOrder>(s => s.OrderId).IsRequired();
        
        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id");
        
        builder.HasOne(x => x.Payment)
            .WithOne(x => x.PaymentOrder)
            .HasForeignKey<PaymentOrder>(s => s.PaymentId).IsRequired();

        builder.Navigation(x => x.Payment);
        builder.Navigation(x => x.Order);

        
    }
}