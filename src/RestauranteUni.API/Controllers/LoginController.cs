using Microsoft.AspNetCore.Mvc;
using RestauranteUni.Domain.Core.Login;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUseCaseHandler<LoginDto, LoginResponseDto> _handler;

        public LoginController(IUseCaseHandler<LoginDto, LoginResponseDto> handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginDto dto, CancellationToken cancellation)
        {
            var result = await _handler.HandleAsync(dto, cancellation);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            var errorResponse = result.ToErrorResponse("Login Error");
            return StatusCode(errorResponse.Status, errorResponse);

        }

    }
}
