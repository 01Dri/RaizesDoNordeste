using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Restaurants.DTO;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Application.UseCases.Restaurants
{
    public sealed class ListRestaurantsUseCase : IUseCaseHandler<ListRestaurantsQueryDto, ListRestaurantsResponseDto>, IUseCaseHandler<ListRestaurantsResponseDto>
    {
        private readonly ApplicationDbContext _context;

        public ListRestaurantsUseCase(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Result<ListRestaurantsResponseDto>> HandleAsync(CancellationToken cancellation = default)
            => HandleAsync(new ListRestaurantsQueryDto(1, 10), cancellation);

        public async Task<Result<ListRestaurantsResponseDto>> HandleAsync(ListRestaurantsQueryDto query, CancellationToken cancellation = default)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var limit = query.Limit < 1 ? 10 : (query.Limit > 100 ? 100 : query.Limit);

            var totalItems = await _context.Restaurants.CountAsync(cancellation);

            var restaurants = await _context.Restaurants
                .OrderBy(r => r.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
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
                Restaurants = restaurants,
                Page = page,
                Limit = limit,
                TotalItems = totalItems
            });
        }
    }
}
