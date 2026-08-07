using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Stocks.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("estoque")]
    [Authorize]
    public class StockController : RaizesDoNordesteController
    {
        private readonly IUseCaseHandler<StockMovementRequestDto, StockMovementResponseDto> _movementHandler;
        private readonly IUseCaseHandler<GetStockByRestaurantQueryDto, StockResponseDto> _getByRestaurantHandler;
        private readonly IUseCaseHandler<StockResponseDto> _getCurrentUserStockHandler;
        private readonly IUseCaseHandler<AddStockIngredientDto, StockIngredientResponseDto> _addIngredientHandler;

        public StockController(
            IUseCaseHandler<StockMovementRequestDto, StockMovementResponseDto> movementHandler,
            IUseCaseHandler<GetStockByRestaurantQueryDto, StockResponseDto> getByRestaurantHandler,
            IUseCaseHandler<StockResponseDto> getCurrentUserStockHandler,
            IUseCaseHandler<AddStockIngredientDto, StockIngredientResponseDto> addIngredientHandler)
        {
            _movementHandler = movementHandler;
            _getByRestaurantHandler = getByRestaurantHandler;
            _getCurrentUserStockHandler = getCurrentUserStockHandler;
            _addIngredientHandler = addIngredientHandler;
        }

        [HttpPost("ingredientes")]
        [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> AddIngredient([FromBody] AddStockIngredientDto dto, CancellationToken cancellation)
        {
            var result = await _addIngredientHandler.HandleAsync(dto, cancellation);
            return result.IsSuccess ? Created($"/estoque", result) : Error("Erro ao cadastrar ingrediente no estoque", result);
        }

        [HttpPost("movimentacao")]
        [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> Move([FromBody] StockMovementRequestDto dto, CancellationToken cancellation)
        {
            var result = await _movementHandler.HandleAsync(dto, cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao registrar movimentação de estoque", result);
        }

        [HttpGet]
        [RolesAuthorize(RoleType.Professional, RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> GetStock(CancellationToken cancellation)
        {
            var result = await _getCurrentUserStockHandler.HandleAsync(cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao consultar estoque", result);
        }

        [HttpGet("unidade/{restaurantId:guid}")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> GetByRestaurant([FromRoute] Guid restaurantId, CancellationToken cancellation)
        {
            var result = await _getByRestaurantHandler.HandleAsync(new GetStockByRestaurantQueryDto(restaurantId), cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao consultar estoque da unidade", result);
        }
    }
}
