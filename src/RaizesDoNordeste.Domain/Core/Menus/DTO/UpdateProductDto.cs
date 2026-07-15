using RaizesDoNordeste.Domain.UseCases;
using System.Text.Json.Serialization;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record UpdateProductDto : IUseCaseRequest
    {
        [JsonIgnore]
        public long Id { get; set; }
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsAvailable { get; init; }
        public int DisplayOrder { get; init; }
        public int PreparationTimeInMinutes { get; init; }
        public bool IsFeatured { get; init; }
    }
}
