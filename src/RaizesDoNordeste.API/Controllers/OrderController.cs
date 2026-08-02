using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Orders.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers;

[ApiController]
[Route("pedido")]
[Authorize]
public class OrderController : RaizesDoNordesteController   
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

        if (!result.IsSuccess)
            return Error("Falha ao criar o pedido", result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data.Id },
            result.Data);
    }
    
    [HttpPut]
    [Route("status/{id:guid}")]
    [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
    public async Task<IActionResult> ChangeStatus([FromRoute] Guid id, [FromBody] ChangeOrderStatusDto dto, CancellationToken cancellationToken)
    {
        dto.OrderId = id;
        var result = await _changeStatusHandler.HandleAsync(dto, cancellationToken);
        return result.IsSuccess ? Ok(result) : Error("Falha ao alterar o status do pedido", result);
    }

    [HttpGet]
    [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
    public async Task<IActionResult> Get(
        [FromQuery] OrderStatus? status,
        [FromQuery(Name = "canalPedido")] OrderChannel? canalPedido,
        CancellationToken cancellationToken)
    {
        var queryDto = new ListOrdersQueryDto(status, canalPedido);
        var result = await _listOrdersHandler.HandleAsync(queryDto, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : Error("Falha ao obter lista de pedidos", result);
    }

    [HttpGet("{id:guid}")]
    [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _getOrderByIdHandler.HandleAsync(new GetOrderByIdQueryDto(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : Error("Falha ao obter detalhes do pedido", result);
    }
}
