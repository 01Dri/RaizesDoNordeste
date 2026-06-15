using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Ingredients;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class IngredientBuilder : BaseEntityBuilder<long, Ingredient>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Ingredient> builder)
        {

            builder.HasIndex(x => x.PublicId)
                .IsUnique();

            builder.Property(x => x.PublicId)
                .HasColumnName("public_id")
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .HasColumnName("name").IsRequired();


            builder.Property(x => x.Unit)
                .HasColumnName("unit").IsRequired();

        }
    }
}
