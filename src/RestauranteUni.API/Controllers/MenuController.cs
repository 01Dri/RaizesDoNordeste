using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestauranteUni.Domain.Core.Menus.DTO;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.API.Controllers;


[ApiController]
[Route("cardapio")]
[Authorize]

public class MenuController : ControllerBase
{
    private readonly IUseCaseHandler<MenuResponseDto> _handler;
    public MenuController(IUseCaseHandler<MenuResponseDto> handler)
    {
        _handler = handler;
    }
    [HttpGet]
    [Route("usuario-atual")]
    public async Task<IActionResult> GetRestaurantMenuOfCurrentUser(CancellationToken cancellation)
    {
        var result = await _handler.HandleAsync(cancellation);
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        var errorResponse = result.ToErrorResponse("Erro ao obter o cardápio");
        return StatusCode(errorResponse.Status, errorResponse);
    }
}