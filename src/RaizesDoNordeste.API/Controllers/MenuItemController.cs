using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("cardapio/itens")]
    [Route("produtos")]
    [Authorize]
    public class MenuItemController : RaizesDoNordesteController
    {
        private readonly IUseCaseHandler<CreateProductDto, ProductResponseDto> _createHandler;
        private readonly IUseCaseHandler<UpdateProductDto, ProductResponseDto> _updateHandler;
        private readonly IUseCaseHandler<DeleteProductDto, DeleteProductResponseDto> _deleteHandler;
        private readonly IUseCaseHandler<GetProductByIdQueryDto, ProductResponseDto> _getByIdHandler;
        private readonly IUseCaseHandler<ListProductsQueryDto, ListProductsResponseDto> _listHandler;
        private readonly IUseCaseHandler<AddMenuItemIngredientDto, AddMenuItemIngredientResponseDto> _addIngredientHandler;

        public MenuItemController(
            IUseCaseHandler<CreateProductDto, ProductResponseDto> createHandler,
            IUseCaseHandler<UpdateProductDto, ProductResponseDto> updateHandler,
            IUseCaseHandler<DeleteProductDto, DeleteProductResponseDto> deleteHandler,
            IUseCaseHandler<GetProductByIdQueryDto, ProductResponseDto> getByIdHandler,
            IUseCaseHandler<ListProductsQueryDto, ListProductsResponseDto> listHandler,
            IUseCaseHandler<AddMenuItemIngredientDto, AddMenuItemIngredientResponseDto> addIngredientHandler)
        {
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _getByIdHandler = getByIdHandler;
            _listHandler = listHandler;
            _addIngredientHandler = addIngredientHandler;
        }

        [HttpPost("{publicMenuItemId:guid}/ingredientes")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> AddIngredientByPublicId([FromRoute] Guid publicMenuItemId, [FromBody] AddMenuItemIngredientDto dto, CancellationToken cancellation)
        {
            dto.PublicMenuItemId = publicMenuItemId;
            var result = await _addIngredientHandler.HandleAsync(dto, cancellation);
            return result.IsSuccess ? Created($"/cardapio/itens/{publicMenuItemId}/ingredientes/{result.Data?.Id}", result) : Error("Erro ao vincular ingrediente ao item do cardápio", result);
        }

        [HttpPost("{menuItemId:long}/ingredientes")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> AddIngredient([FromRoute] long menuItemId, [FromBody] AddMenuItemIngredientDto dto, CancellationToken cancellation)
        {
            dto.MenuItemId = menuItemId;
            var result = await _addIngredientHandler.HandleAsync(dto, cancellation);
            return result.IsSuccess ? Created($"/cardapio/itens/{menuItemId}/ingredientes/{result.Data?.Id}", result) : Error("Erro ao vincular ingrediente ao item do cardápio", result);
        }

        [HttpPost]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellation)
        {
            var result = await _createHandler.HandleAsync(dto, cancellation);
            if (!result.IsSuccess)
            {
                return Error("Erro ao cadastrar item no cardápio", result);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.Id },
                result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellation = default)
        {
            var result = await _listHandler.HandleAsync(new ListProductsQueryDto(page, limit), cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao listar itens do cardápio", result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken cancellation)
        {
            var result = await _getByIdHandler.HandleAsync(new GetProductByIdQueryDto(id), cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao obter item do cardápio", result);
        }

        [HttpPut("{id:long}")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateProductDto dto, CancellationToken cancellation)
        {
            dto.Id = id;
            var result = await _updateHandler.HandleAsync(dto, cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao atualizar item do cardápio", result);
        }

        [HttpDelete("{id:long}")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken cancellation)
        {
            var result = await _deleteHandler.HandleAsync(new DeleteProductDto(id), cancellation);
            return result.IsSuccess ? Ok(result) : Error("Erro ao excluir item do cardápio", result);
        }
    }
}
