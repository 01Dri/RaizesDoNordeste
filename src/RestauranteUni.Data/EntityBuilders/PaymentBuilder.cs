using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Payments;

namespace RestauranteUni.Data.EntityBuilders;

public class PaymentBuilder : BaseEntityBuilder<long, Payment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Payment> builder)
    {

        builder.ToTable("payment");
        
        builder.Property(x => x.Total)
            .HasColumnName("total").IsRequired();
        
        builder.Property(x => x.TotalPaid)
            .HasColumnName("total_paid").IsRequired();
        
        builder.Property(x => x.PaymentMethod)
            .HasColumnName("payment_method").IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status").IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description").HasMaxLength(255);

        builder.Navigation(X => X.PaymentOrder);

    }
}