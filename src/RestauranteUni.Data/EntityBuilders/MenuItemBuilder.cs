using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography;
using RestauranteUni.Domain.Core.Menus;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class MenuItemBuilder : BaseEntityBuilder<long, MenuItem>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<MenuItem> builder)
        {
            builder.ToTable("menu_item");

            builder.HasIndex(x => x.PublicId)
                .IsUnique();

            builder.Property(x => x.PublicId)
                .HasColumnName("public_id")
                .IsRequired();


            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.Price)
                .HasColumnName("price")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(500);

            builder.Property(x => x.IsAvailable)
                .HasColumnName("is_available")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.DisplayOrder)
                .HasColumnName("display_order")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.PreparationTimeInMinutes)
                .HasColumnName("preparation_time_in_minutes")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.IsFeatured)
                .HasColumnName("is_featured")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.MenuId)
                .HasColumnName("menu_id")
                .IsRequired();

            builder.HasOne(x => x.Menu)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.MenuId)
                .OnDelete(DeleteBehavior.Cascade);



            
            builder.Navigation(x => x.Menu).AutoInclude();
            builder.Navigation(x => x.Ingredients);

            // Índices para melhorar performance
            builder.HasIndex(x => x.MenuId);
            builder.HasIndex(x => x.IsAvailable);
            builder.HasIndex(x => x.IsFeatured);
            builder.HasIndex(x => new { x.MenuId, x.DisplayOrder });

            builder.HasData(
                new MenuItem
                {
                    Id = 1,
                    PublicId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c9b"),
                    Title = "X-Burger",
                    Description = "Hambúrguer artesanal com queijo e salada",
                    Price = 29.90m,
                    ImageUrl = "/images/xburger.jpg",
                    IsAvailable = true,
                    DisplayOrder = 1,
                    PreparationTimeInMinutes = 15,
                    IsFeatured = true,
                    MenuId = 1,
                    CreatedAt = new DateTime(2026, 1, 1),
                    Active = true
                },
                new MenuItem
                {
                    Id = 2,
                    PublicId = Guid.Parse("2a88024d-2618-4e25-87f5-35217f7a7c9b"),
                    Title = "Pizza Calabresa",
                    Description = "Pizza tradicional de calabresa",
                    Price = 59.90m,
                    ImageUrl = "/images/calabresa.jpg",
                    IsAvailable = true,
                    DisplayOrder = 2,
                    PreparationTimeInMinutes = 25,
                    IsFeatured = true,
                    MenuId = 1,
                    CreatedAt = new DateTime(2026, 1, 1),
                    Active = true
                },
                new MenuItem
                {
                    Id = 3,
                    PublicId = Guid.Parse("7a88024d-2618-4e25-87f5-35217f7a7c9b"),
                    Title = "Coca-Cola 350ml",
                    Description = "Refrigerante lata",
                    Price = 6.50m,
                    ImageUrl = "/images/coca350.jpg",
                    IsAvailable = true,
                    DisplayOrder = 3,
                    PreparationTimeInMinutes = 1,
                    IsFeatured = false,
                    MenuId = 1,
                    CreatedAt = new DateTime(2026, 1, 1),
                    Active = true
                }
            );
        }
    }
}
