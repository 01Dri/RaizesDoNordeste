using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Restaurants.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("unidades")]
    [Authorize]
    public class RestaurantController : RaizesDoNordesteController
    {
        private readonly IUseCaseHandler<ListRestaurantsResponseDto> _listHandler;

        public RestaurantController(IUseCaseHandler<ListRestaurantsResponseDto> listHandler)
        {
            _listHandler = listHandler;
        }

        [HttpGet]
        [RolesAuthorize(RoleType.Admin)]
        public async Task<IActionResult> Get(CancellationToken cancellation)
        {
            var result = await _listHandler.HandleAsync(cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao obter unidades", result);
        }
    }
}
