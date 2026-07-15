using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record DeleteProductDto(long Id) : IUseCaseRequest;
}
