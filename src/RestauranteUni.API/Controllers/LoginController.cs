using Microsoft.AspNetCore.Mvc;
using RestauranteUni.Domain.Core.Login;
using RestauranteUni.Domain.UseCases;

namespace RestauranteUni.API.Controllers
{

    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        private readonly IUseCaseHandler<LoginDto, LoginResponseDto> _handler;
        public readonly IConfiguration _configuration;

        public LoginController(IUseCaseHandler<LoginDto, LoginResponseDto> handler, IConfiguration configuration)
        {   
            _handler = handler;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginDto dto, CancellationToken cancellation)
        {
            var result = await _handler.HandleAsync(dto, cancellation);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            var errorResponse = result.ToErrorResponse("Erro ao realizar login");
            return StatusCode(errorResponse.Status, errorResponse);

        }


        [HttpGet]
        [Route("desenvolvedor")]
        public async Task<IActionResult> LoginDeveloperAsync(CancellationToken cancellation)
        {
            var developerCredentials = _configuration
                .GetSection("DeveloperCredentials");

            var email = developerCredentials["Email"];
            var password = developerCredentials["Password"];
            
            
            var result = await _handler.HandleAsync(
                new LoginDto(email, password, Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a")), cancellation);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            var errorResponse = result.ToErrorResponse("Erro ao realizar login");
            return StatusCode(errorResponse.Status, errorResponse);

        }
    }
}
