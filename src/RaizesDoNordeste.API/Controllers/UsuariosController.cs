using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.Core.Accounts.DTO;
using RaizesDoNordeste.Domain.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUseCaseHandler<CreateAccountDto, CreateAccountUseCaseResponseDto> _createAccountHandler;
        private readonly IUseCaseHandler<UserProfileResponseDto> _getProfileHandler;

        public UsuariosController(
            IUseCaseHandler<CreateAccountDto, CreateAccountUseCaseResponseDto> createAccountHandler,
            IUseCaseHandler<UserProfileResponseDto> getProfileHandler)
        {
            _createAccountHandler = createAccountHandler;
            _getProfileHandler = getProfileHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateAccountDto dto, CancellationToken cancellation)
        {
            var result = await _createAccountHandler.HandleAsync(dto, cancellation);
            if (result.IsSuccess)
            {
                return Created("", result.Data);
            }

            return BadRequest(result.Validations);
        }

        [HttpGet("perfil")]
        [Authorize]
        public async Task<IActionResult> GetProfileAsync(CancellationToken cancellation)
        {
            var result = await _getProfileHandler.HandleAsync(cancellation);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var errorResponse = result.ToErrorResponse("Erro ao obter perfil do usuário");
            return StatusCode(errorResponse.Status, errorResponse);
        }
    }
}
