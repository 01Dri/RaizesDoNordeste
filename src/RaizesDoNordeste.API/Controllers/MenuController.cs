using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers;

[ApiController]
[Route("cardapio")]
[Authorize]
public class MenuController : RaizesDoNordesteController
{
    private readonly IUseCaseHandler<MenuResponseDto> _handler;
    private readonly IUseCaseHandler<CreateMenuDto, MenuResponseDto> _createHandler;

    public MenuController(
        IUseCaseHandler<MenuResponseDto> handler,
        IUseCaseHandler<CreateMenuDto, MenuResponseDto> createHandler)
    {
        _handler = handler;
        _createHandler = createHandler;
    }

    [HttpPost]
    [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateMenuDto dto, CancellationToken cancellation)
    {
        var result = await _createHandler.HandleAsync(dto, cancellation);
        return result.IsSuccess ? Created("/cardapio/usuario-atual", result) : Error("Erro ao criar o cardápio", result);
    }

    [HttpGet]
    [Route("usuario-atual")]
    public async Task<IActionResult> GetRestaurantMenuOfCurrentUser(CancellationToken cancellation)
    {
        var result = await _handler.HandleAsync(cancellation);
        return result.IsSuccess ? Ok(result) : Error("Erro ao obter o cardápio", result);
    }
}
