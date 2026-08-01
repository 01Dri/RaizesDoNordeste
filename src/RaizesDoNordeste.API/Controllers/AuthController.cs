using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.Core.Login;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : RaizesDoNordesteController
    {
        private readonly IUseCaseHandler<LoginDto, LoginResponseDto> _loginHandler;
        private readonly IUseCaseHandler<RefreshRequestDto, LoginResponseDto> _refreshHandler;
        private readonly IUseCaseHandler<LogoutRequestDto, LogoutResponseDto> _logoutHandler;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUseCaseHandler<LoginDto, LoginResponseDto> loginHandler,
            IUseCaseHandler<RefreshRequestDto, LoginResponseDto> refreshHandler,
            IUseCaseHandler<LogoutRequestDto, LogoutResponseDto> logoutHandler,
            IConfiguration configuration)
        {
            _loginHandler = loginHandler;
            _refreshHandler = refreshHandler;
            _logoutHandler = logoutHandler;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto, CancellationToken cancellation)
        {
            var result = await _loginHandler.HandleAsync(dto, cancellation);
            return !result.IsSuccess ? Error("Erro ao realizar login", result) : Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshRequestDto dto, CancellationToken cancellation)
        {
            var result = await _refreshHandler.HandleAsync(dto, cancellation);
            return !result.IsSuccess ? Error("Erro ao renovar token", result) : Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequestDto dto, CancellationToken cancellation)
        {
            var result = await _logoutHandler.HandleAsync(dto, cancellation);
            return !result.IsSuccess ? Error("Erro ao realizar logout", result) : Ok(result);
        }

        [HttpGet("desenvolvedor")]
        public async Task<IActionResult> LoginDeveloperAsync(CancellationToken cancellation)
        {
            var developerCredentials = _configuration.GetSection("DeveloperCredentials");

            var email = developerCredentials["Email"];
            var password = developerCredentials["Password"];

            var result = await _loginHandler.HandleAsync(
                new LoginDto(email, password, Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a")), cancellation);

            return !result.IsSuccess ? Error("Erro ao realizar login", result) : Ok(result);

        }
    }
}
