using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestauranteUni.Domain.Core.Orders.DTO;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.API.Controllers;

[ApiController]
[Route("pedido")]
[Authorize]
public class OrderController : BaseController   
{
    private readonly IUseCaseHandler<CreateOrderDto, OrderResponseDto> _createOrderHandler;
    private readonly IUseCaseHandler<ChangeOrderStatusDto, OrderStatusChangeResponseDto> _changeStatusHandler;

    public OrderController
    (
        IUseCaseHandler<CreateOrderDto, OrderResponseDto> createOrderHandler,
        IUseCaseHandler<ChangeOrderStatusDto, OrderStatusChangeResponseDto> changeStatusHandler
    )
    {
        _createOrderHandler = createOrderHandler;
        _changeStatusHandler = changeStatusHandler;
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
    [Authorize]
    public async Task<IActionResult> ChangeStatus(ChangeOrderStatusDto dto, CancellationToken cancellationToken)
    {
        var result = await _changeStatusHandler.HandleAsync(dto, cancellationToken);
        if (result.IsSuccess)
        {
            return Created("", result.Data);
        }
        return Error("Falha ao alterar o status do pedido", result);
    }
}