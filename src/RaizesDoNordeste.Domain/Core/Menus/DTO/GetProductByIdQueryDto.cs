using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public record GetProductByIdQueryDto(long Id) : IUseCaseRequest;
}
