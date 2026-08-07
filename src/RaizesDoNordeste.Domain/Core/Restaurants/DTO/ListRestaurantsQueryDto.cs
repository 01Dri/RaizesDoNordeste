using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Restaurants.DTO
{
    public class ListRestaurantsQueryDto : IUseCaseRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;

        public ListRestaurantsQueryDto() { }

        public ListRestaurantsQueryDto(int page = 1, int limit = 10)
        {
            Page = page < 1 ? 1 : page;
            Limit = limit < 1 ? 10 : (limit > 100 ? 100 : limit);
        }
    }
}
