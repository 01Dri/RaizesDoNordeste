using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteUni.Domain.Core.Restaurants;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Data.EntityBuilders
{
    internal sealed class RestaurantBuilder : BaseEntityBuilder<Guid, Restaurant>
    {
            private static readonly Guid RestauranteUniversitarioId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");
        private static readonly Guid CantinaCentralId = Guid.Parse("f02884ad-1725-4fcb-9bb6-cbf0b8f5fef6");
        private static readonly Guid BistroDoCampusId = Guid.Parse("be0b1f01-0d0f-43e6-9575-b1e117ad62cb");
        private static readonly DateTime SeedCreatedAt = new(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        protected override void ConfigureEntity(EntityTypeBuilder<Restaurant> builder)
        {
            builder.ToTable("restaurants");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasConversion(
                    email => email.Value,
                    value => new Email(value))
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasConversion(
                    phone => phone.Value,
                    value => new Phone(value))
                .HasColumnName("phone")
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(x => x.Cnpj)
                .HasConversion(
                    cnpj => cnpj.Value,
                    value => new Cnpj(value))
                .HasColumnName("cnpj")
                .HasMaxLength(14)
                .IsRequired();

            builder.OwnsOne(x => x.Address, address =>
            {
                address.Property(x => x.Street)
                    .HasColumnName("address_street")
                    .HasMaxLength(150)
                    .IsRequired();

                address.Property(x => x.Number)
                    .HasColumnName("address_number")
                    .HasMaxLength(20)
                    .IsRequired();

                address.Property(x => x.District)
                    .HasColumnName("address_district")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.City)
                    .HasColumnName("address_city")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.State)
                    .HasColumnName("address_state")
                    .HasMaxLength(2)
                    .IsRequired();

                address.Property(x => x.ZipCode)
                    .HasColumnName("address_zip_code")
                    .HasMaxLength(8)
                    .IsRequired();

                address.Property(x => x.Complement)
                    .HasColumnName("address_complement")
                    .HasMaxLength(150);
            });

            builder.Navigation(x => x.Address)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .HasDatabaseName("ix_restaurants_email")
                .IsUnique();

            builder.HasIndex(x => x.Cnpj)
                .HasDatabaseName("ix_restaurants_cnpj")
                .IsUnique();

            builder.Navigation(x => x.Menu);
            builder.Navigation(x => x.Stock);


            SeedRestaurants(builder);
        }

        private static void SeedRestaurants(EntityTypeBuilder<Restaurant> builder)
        {
            builder.HasData(
                new Restaurant
                {
                    Id = RestauranteUniversitarioId,
                    Name = "Restaurante Universitario",
                    Description = "Restaurante principal do campus com refeicoes acessiveis para estudantes.",
                    Email = new Email("ru@restauranteuni.com"),
                    Phone = new Phone("1133334444"),
                    Cnpj = new Cnpj("12345678000195"),
                    CreatedAt = SeedCreatedAt,
                    Active = true
                },
                new Restaurant
                {
                    Id = CantinaCentralId,
                    Name = "Cantina Central",
                    Description = "Cantina com lanches, bebidas e opcoes rapidas entre as aulas.",
                    Email = new Email("cantina@restauranteuni.com"),
                    Phone = new Phone("11988887777"),
                    Cnpj = new Cnpj("11222333000181"),
                    CreatedAt = SeedCreatedAt,
                    Active = true
                },
                new Restaurant
                {
                    Id = BistroDoCampusId,
                    Name = "Bistro do Campus",
                    Description = "Espaco com pratos executivos e opcoes vegetarianas.",
                    Email = new Email("bistro@restauranteuni.com"),
                    Phone = new Phone("1130302020"),
                    Cnpj = new Cnpj("98765432000198"),
                    CreatedAt = SeedCreatedAt,
                    Active = true
                });

            builder.OwnsOne(x => x.Address)
                .HasData(
                    new
                    {
                        RestaurantId = RestauranteUniversitarioId,
                        Street = "Avenida Universitaria",
                        Number = "1000",
                        District = "Centro",
                        City = "Sao Paulo",
                        State = "SP",
                        ZipCode = "01001000",
                        Complement = "Bloco A"
                    },
                    new
                    {
                        RestaurantId = CantinaCentralId,
                        Street = "Rua dos Estudantes",
                        Number = "250",
                        District = "Vila Academica",
                        City = "Sao Paulo",
                        State = "SP",
                        ZipCode = "01002000",
                        Complement = "Praca de alimentacao"
                    },
                    new
                    {
                        RestaurantId = BistroDoCampusId,
                        Street = "Alameda das Ciencias",
                        Number = "45",
                        District = "Campus Norte",
                        City = "Sao Paulo",
                        State = "SP",
                        ZipCode = "01003000",
                        Complement = "Predio 2"
                    });
        }
    }
}
