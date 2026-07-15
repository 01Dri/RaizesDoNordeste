using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Menus.DTO;
using RaizesDoNordeste.Domain.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("produtos")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IUseCaseHandler<CreateProductDto, ProductResponseDto> _createHandler;
        private readonly IUseCaseHandler<UpdateProductDto, ProductResponseDto> _updateHandler;
        private readonly IUseCaseHandler<DeleteProductDto, DeleteProductResponseDto> _deleteHandler;
        private readonly IUseCaseHandler<GetProductByIdQueryDto, ProductResponseDto> _getByIdHandler;
        private readonly IUseCaseHandler<ListProductsResponseDto> _listHandler;

        public ProductsController(
            IUseCaseHandler<CreateProductDto, ProductResponseDto> createHandler,
            IUseCaseHandler<UpdateProductDto, ProductResponseDto> updateHandler,
            IUseCaseHandler<DeleteProductDto, DeleteProductResponseDto> deleteHandler,
            IUseCaseHandler<GetProductByIdQueryDto, ProductResponseDto> getByIdHandler,
            IUseCaseHandler<ListProductsResponseDto> listHandler)
        {
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _getByIdHandler = getByIdHandler;
            _listHandler = listHandler;
        }

        [HttpPost]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProductDto dto, CancellationToken cancellation)
        {
            var result = await _createHandler.HandleAsync(dto, cancellation);
            if (result.IsSuccess)
            {
                return Created($"produtos/{result.Data!.Id}", result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao cadastrar produto");
            return StatusCode(errorResponse.Status, errorResponse);
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync(CancellationToken cancellation)
        {
            var result = await _listHandler.HandleAsync(cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao listar produtos");
            return StatusCode(errorResponse.Status, errorResponse);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] long id, CancellationToken cancellation)
        {
            var result = await _getByIdHandler.HandleAsync(new GetProductByIdQueryDto(id), cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao obter produto");
            return StatusCode(errorResponse.Status, errorResponse);
        }

        [HttpPut("{id:long}")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] UpdateProductDto dto, CancellationToken cancellation)
        {
            dto.Id = id;
            var result = await _updateHandler.HandleAsync(dto, cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao atualizar produto");
            return StatusCode(errorResponse.Status, errorResponse);
        }

        [HttpDelete("{id:long}")]
        [RolesAuthorize(RoleType.Manager, RoleType.Owner, RoleType.Admin)]
        public async Task<IActionResult> DeleteAsync([FromRoute] long id, CancellationToken cancellation)
        {
            var result = await _deleteHandler.HandleAsync(new DeleteProductDto(id), cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao excluir produto");
            return StatusCode(errorResponse.Status, errorResponse);
        }
    }
}
