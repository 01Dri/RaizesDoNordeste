using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.Extensions;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Restaurants;
using RaizesDoNordeste.Domain.Core.Restaurants.DTO;
using RaizesDoNordeste.Domain.Core.Stocks;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Restaurants
{
    public sealed class CreateRestaurantUseCaseHandler : IUseCaseHandler<CreateRestaurantDto, RestaurantDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateRestaurantDto> _validator;

        public CreateRestaurantUseCaseHandler(ApplicationDbContext context, IValidator<CreateRestaurantDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result<RestaurantDto>> HandleAsync(CreateRestaurantDto parameter, CancellationToken cancellation = default)
        {
            var validation = await _validator.ValidateAsync(parameter, cancellation);
            if (validation.ContainsErrors())
            {
                return validation.ToResultFailure<RestaurantDto>();
            }

            var cnpj = new Cnpj(parameter.Cnpj);
            var cnpjExists = await _context.Restaurants
                .AnyAsync(r => r.Cnpj == cnpj, cancellation);

            if (cnpjExists)
            {
                return Result<RestaurantDto>.Failure(
                    new Error("Já existe uma unidade cadastrada com este CNPJ."),
                    HttpStatusCode.Conflict
                );
            }

            var email = new Email(parameter.Email);
            var emailExists = await _context.Restaurants
                .AnyAsync(r => r.Email == email, cancellation);

            if (emailExists)
            {
                return Result<RestaurantDto>.Failure(
                    new Error("Já existe uma unidade cadastrada com este e-mail."),
                    HttpStatusCode.Conflict
                );
            }

            var stock = new Stock
            {
                PublicId = Guid.NewGuid(),
                Items = parameter.StockItems?.Select(item => new StockIngredient
                {
                    PublicId = Guid.NewGuid(),
                    Name = item.Name,
                    Unit = item.Unit,
                    Quantity = item.Quantity
                }).ToList() ?? []
            };

            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid(),
                Name = parameter.Name,
                Description = parameter.Description,
                Phone = new Phone(parameter.Phone),
                Email = email,
                Cnpj = cnpj,
                Address = new Address(
                    parameter.AddressStreet,
                    parameter.AddressNumber,
                    parameter.AddressDistrict,
                    parameter.AddressCity,
                    parameter.AddressState,
                    parameter.AddressZipCode,
                    parameter.AddressComplement
                ),
                Stock = stock
            };

            await _context.Restaurants.AddAsync(restaurant, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var resultDto = new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Phone = restaurant.Phone.Value,
                Email = restaurant.Email.Value,
                Cnpj = restaurant.Cnpj.Value,
                AddressStreet = restaurant.Address.Street,
                AddressNumber = restaurant.Address.Number,
                AddressDistrict = restaurant.Address.District,
                AddressCity = restaurant.Address.City,
                AddressState = restaurant.Address.State,
                AddressZipCode = restaurant.Address.ZipCode,
                AddressComplement = restaurant.Address.Complement
            };

            return Result<RestaurantDto>.Success(resultDto);
        }
    }
}
