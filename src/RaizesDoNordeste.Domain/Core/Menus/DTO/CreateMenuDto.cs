using System;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.Domain.Core.Menus.DTO
{
    public class CreateMenuDto : IUseCaseRequest
    {
        public string Name { get; set; } = null!;
        public Guid? RestaurantId { get; set; }
    }
}
