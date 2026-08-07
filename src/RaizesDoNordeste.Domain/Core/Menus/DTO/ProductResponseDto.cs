using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Collections.Generic;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record ProductIngredientResponseDto
    {
        public long Id { get; init; }
        public long StockIngredientId { get; init; }
        public string StockIngredientName { get; init; } = null!;
        public decimal QuantityUseToOrder { get; init; }
    }

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
        public List<ProductIngredientResponseDto> Ingredients { get; init; } = [];
        public Error? ErrorResponse { get; set; }
    }
}
