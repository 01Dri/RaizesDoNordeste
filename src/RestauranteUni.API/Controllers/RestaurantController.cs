using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestauranteUni.Domain.Core.Menus.DTO;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.API.Controllers
{

    [ApiController]
    [Route("restaurante")]
    [Authorize]
    public class RestaurantController : ControllerBase
    {

        private readonly IUseCaseHandler<MenuResponseDto> _handler;
        public RestaurantController(IUseCaseHandler<MenuResponseDto> handler)
        {
            _handler = handler;
        }
    }
}
