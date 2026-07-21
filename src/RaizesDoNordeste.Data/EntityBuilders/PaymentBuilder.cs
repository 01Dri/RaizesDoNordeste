using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaizesDoNordeste.Domain.Core.Payments;

namespace RaizesDoNordeste.Data.EntityBuilders;

public class PaymentBuilder : BaseEntityBuilder<long, Payment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Payment> builder)
    {

        builder.ToTable("payment");
        
        builder.Property(x => x.Total)
            .HasColumnName("total").IsRequired();
        
        builder.Property(x => x.TotalPaid)
            .HasColumnName("total_paid").IsRequired();

        builder.Property(x => x.TotalDiscount)
            .HasColumnName("total_discount")
            .HasDefaultValue(0.0m)
            .IsRequired();
        
        builder.Property(x => x.PaymentMethod)
            .HasColumnName("payment_method").IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status").IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description").HasMaxLength(255);

        builder.Property(x => x.ExternalPaymentId)
            .HasColumnName("external_payment_id").HasMaxLength(100);

        builder.Navigation(X => X.PaymentOrder);

    }
}
