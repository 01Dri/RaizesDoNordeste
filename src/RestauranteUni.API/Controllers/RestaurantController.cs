using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestauranteUni.Data;
using RestauranteUni.Domain.Restaurants.Menus.DTO;
using RestauranteUni.Domain.UseCases;
using RestauranteUni.Domain.Users;

namespace RestauranteUni.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class RestaurantController : ControllerBase
    {

        private readonly IUseCaseHandler<MenuResponseDto> _handler;
        public RestaurantController(IUseCaseHandler<MenuResponseDto> handler)
        {
            _handler = handler;
        }

        [HttpGet]
        [Route("user/menu")]
        public async Task<IActionResult> GetUserRestaurantMenu(CancellationToken cancellation)
        {
            var result = await _handler.HandleAsync(cancellation);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            var errorResponse = result.ToErrorResponse("Test");
            return StatusCode(errorResponse.Status, errorResponse);
        }
    }
}
