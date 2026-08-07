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
        private readonly IUseCaseHandler<ListRestaurantsQueryDto, ListRestaurantsResponseDto> _listHandler;
        private readonly IUseCaseHandler<CreateRestaurantDto, RestaurantDto> _createHandler;

        public RestaurantController(
            IUseCaseHandler<ListRestaurantsQueryDto, ListRestaurantsResponseDto> listHandler,
            IUseCaseHandler<CreateRestaurantDto, RestaurantDto> createHandler)
        {
            _listHandler = listHandler;
            _createHandler = createHandler;
        }

        [HttpGet]
        [RolesAuthorize(RoleType.Admin, RoleType.Owner)]
        public async Task<IActionResult> Get(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellation = default)
        {
            var result = await _listHandler.HandleAsync(new ListRestaurantsQueryDto(page, limit), cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao obter unidades", result);
        }

        [HttpPost]
        [RolesAuthorize(RoleType.Admin, RoleType.Owner)]
        public async Task<IActionResult> Create([FromBody] CreateRestaurantDto dto, CancellationToken cancellation)
        {
            var result = await _createHandler.HandleAsync(dto, cancellation);
            return result.IsSuccess ? Created($"/unidades/{result.Data?.Id}", result) : Error("Erro ao cadastrar unidade", result);
        }
    }
}
