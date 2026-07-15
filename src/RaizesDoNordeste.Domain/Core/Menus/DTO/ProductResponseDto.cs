using RaizesDoNordeste.Domain.UseCases;
using System;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record ProductResponseDto : IUseCaseResponse
    {
        public long Id { get; init; }
        public Guid PublicId { get; init; }
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsAvailable { get; init; }
        public int DisplayOrder { get; init; }
        public int PreparationTimeInMinutes { get; init; }
        public bool IsFeatured { get; init; }
        public long MenuId { get; init; }
        public Error? ErrorResponse { get; set; }
    }
}
