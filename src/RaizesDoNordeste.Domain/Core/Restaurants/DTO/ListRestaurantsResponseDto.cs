using RaizesDoNordeste.Domain.UseCases;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Restaurants.DTO
{
    public class ListRestaurantsResponseDto : IUseCaseResponse
    {
        public List<RestaurantDto> Restaurants { get; set; } = [];
        public Error? ErrorResponse { get; set; }
    }
}
