using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.API.Controllers;

[ApiController]
[Route("pedidos")]
[Route("pedido")]
[Authorize]
public class OrderController : BaseController   
{
    private readonly IUseCaseHandler<CreateOrderDto, OrderResponseDto> _createOrderHandler;
    private readonly IUseCaseHandler<ChangeOrderStatusDto, OrderStatusChangeResponseDto> _changeStatusHandler;
    private readonly IUseCaseHandler<GetOrderByIdQueryDto, OrderResponseDto> _getOrderByIdHandler;
    private readonly IUseCaseHandler<ListOrdersQueryDto, ListOrdersResponseDto> _listOrdersHandler;

    public OrderController
    (
        IUseCaseHandler<CreateOrderDto, OrderResponseDto> createOrderHandler,
        IUseCaseHandler<ChangeOrderStatusDto, OrderStatusChangeResponseDto> changeStatusHandler,
        IUseCaseHandler<GetOrderByIdQueryDto, OrderResponseDto> getOrderByIdHandler,
        IUseCaseHandler<ListOrdersQueryDto, ListOrdersResponseDto> listOrdersHandler
    )
    {
        _createOrderHandler = createOrderHandler;
        _changeStatusHandler = changeStatusHandler;
        _getOrderByIdHandler = getOrderByIdHandler;
        _listOrdersHandler = listOrdersHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        var result = await _createOrderHandler.HandleAsync(dto, cancellationToken);
        if (result.IsSuccess)
        {
            return Created("", result.Data);
        }
        return Error("Falha ao criar o pedido", result);
    }
    
    [HttpPut]
    [Route("status")]
    [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
    public async Task<IActionResult> ChangeStatus(ChangeOrderStatusDto dto, CancellationToken cancellationToken)
    {
        var result = await _changeStatusHandler.HandleAsync(dto, cancellationToken);
        if (result.IsSuccess)
        {
            return Created("", result.Data);
        }
        return Error("Falha ao alterar o status do pedido", result);
    }

    [HttpGet]
    [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
    public async Task<IActionResult> List(
        [FromQuery] OrderStatus? status,
        [FromQuery(Name = "canalPedido")] OrderChannel? canalPedido,
        CancellationToken cancellationToken)
    {
        var queryDto = new ListOrdersQueryDto(status, canalPedido);
        var result = await _listOrdersHandler.HandleAsync(queryDto, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        return Error("Falha ao obter lista de pedidos", result);
    }

    [HttpGet("{id:guid}")]
    [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _getOrderByIdHandler.HandleAsync(new GetOrderByIdQueryDto(id), cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        return Error("Falha ao obter detalhes do pedido", result);
    }
}
