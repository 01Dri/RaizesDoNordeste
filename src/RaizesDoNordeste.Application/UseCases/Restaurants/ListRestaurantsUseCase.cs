using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Restaurants.DTO;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.UseCases.Restaurants
{
    public sealed class ListRestaurantsUseCase : IUseCaseHandler<ListRestaurantsResponseDto>
    {
        private readonly ApplicationDbContext _context;

        public ListRestaurantsUseCase(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ListRestaurantsResponseDto>> HandleAsync(CancellationToken cancellation = default)
        {
            var restaurants = await _context.Restaurants
                .Where(r => r.Active)
                .Select(r => new RestaurantDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Phone = r.Phone.Value,
                    Email = r.Email.Value,
                    Cnpj = r.Cnpj.Value,
                    AddressStreet = r.Address.Street,
                    AddressNumber = r.Address.Number,
                    AddressDistrict = r.Address.District,
                    AddressCity = r.Address.City,
                    AddressState = r.Address.State,
                    AddressZipCode = r.Address.ZipCode,
                    AddressComplement = r.Address.Complement
                })
                .ToListAsync(cancellation);

            return Result<ListRestaurantsResponseDto>.Success(new ListRestaurantsResponseDto
            {
                Restaurants = restaurants
            });
        }
    }
}
