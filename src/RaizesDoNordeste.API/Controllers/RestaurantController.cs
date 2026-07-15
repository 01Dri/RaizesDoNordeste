using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.Core.Restaurants.DTO;
using RaizesDoNordeste.Domain.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("unidades")]
    [Authorize]
    public class RestaurantController : ControllerBase
    {
        private readonly IUseCaseHandler<ListRestaurantsResponseDto> _listHandler;

        public RestaurantController(IUseCaseHandler<ListRestaurantsResponseDto> listHandler)
        {
            _listHandler = listHandler;
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync(CancellationToken cancellation)
        {
            var result = await _listHandler.HandleAsync(cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao obter unidades");
            return StatusCode(errorResponse.Status, errorResponse);
        }
    }
}
