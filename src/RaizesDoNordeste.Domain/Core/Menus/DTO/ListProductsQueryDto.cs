using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public class ListProductsQueryDto : IUseCaseRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;

        public ListProductsQueryDto() { }

        public ListProductsQueryDto(int page = 1, int limit = 10)
        {
            Page = page < 1 ? 1 : page;
            Limit = limit < 1 ? 10 : (limit > 100 ? 100 : limit);
        }
    }
}
