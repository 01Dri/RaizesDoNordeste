using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record CreateProductDto : IUseCaseRequest
    {
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsAvailable { get; init; } = true;
        public int DisplayOrder { get; init; }
        public int PreparationTimeInMinutes { get; init; }
        public bool IsFeatured { get; init; }
    }
}
