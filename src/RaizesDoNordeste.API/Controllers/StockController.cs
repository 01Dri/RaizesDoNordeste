using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.UseCases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("estoque")]
    [Authorize]
    public class StockController : ControllerBase
    {
        private readonly IUseCaseHandler<StockMovementRequestDto, StockMovementResponseDto> _movementHandler;
        private readonly IUseCaseHandler<GetStockByRestaurantQueryDto, StockResponseDto> _getByRestaurantHandler;
        private readonly IUseCaseHandler<StockResponseDto> _getCurrentUserStockHandler;

        public StockController(
            IUseCaseHandler<StockMovementRequestDto, StockMovementResponseDto> movementHandler,
            IUseCaseHandler<GetStockByRestaurantQueryDto, StockResponseDto> getByRestaurantHandler,
            IUseCaseHandler<StockResponseDto> getCurrentUserStockHandler)
        {
            _movementHandler = movementHandler;
            _getByRestaurantHandler = getByRestaurantHandler;
            _getCurrentUserStockHandler = getCurrentUserStockHandler;
        }

        [HttpPost("movimentacao")]
        [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> MoveAsync([FromBody] StockMovementRequestDto dto, CancellationToken cancellation)
        {
            var result = await _movementHandler.HandleAsync(dto, cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao registrar movimentação de estoque");
            return StatusCode(errorResponse.Status, errorResponse);
        }

        [HttpGet]
        [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> GetCurrentStockAsync(CancellationToken cancellation)
        {
            var result = await _getCurrentUserStockHandler.HandleAsync(cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao consultar estoque");
            return StatusCode(errorResponse.Status, errorResponse);
        }

        [HttpGet("unidade/{restaurantId:guid}")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> GetByRestaurantAsync([FromRoute] Guid restaurantId, CancellationToken cancellation)
        {
            var result = await _getByRestaurantHandler.HandleAsync(new GetStockByRestaurantQueryDto(restaurantId), cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao consultar estoque da unidade");
            return StatusCode(errorResponse.Status, errorResponse);
        }
    }
}
