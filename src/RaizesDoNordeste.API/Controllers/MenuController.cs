using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers;


[ApiController]
[Route("cardapio")]
[Authorize]
public class MenuController : RaizesDoNordesteController
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
        return result.IsSuccess ? Created("", result.Data) : Error("Erro ao obter o cardápio", result);
    }
}
