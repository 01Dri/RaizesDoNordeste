using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        public AuthController(
            IUseCaseHandler<LoginDto, LoginResponseDto> loginHandler,
            IUseCaseHandler<RefreshRequestDto, LoginResponseDto> refreshHandler,
            IUseCaseHandler<LogoutRequestDto, LogoutResponseDto> logoutHandler)
        {
            _loginHandler = loginHandler;
            _refreshHandler = refreshHandler;
            _logoutHandler = logoutHandler;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellation)
        {
            var result = await _loginHandler.HandleAsync(dto, cancellation);
            return !result.IsSuccess ? Error("Erro ao realizar login", result) : Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto, CancellationToken cancellation)
        {
            var result = await _refreshHandler.HandleAsync(dto, cancellation);
            return !result.IsSuccess ? Error("Erro ao renovar token", result) : Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto, CancellationToken cancellation)
        {
            var result = await _logoutHandler.HandleAsync(dto, cancellation);
            return !result.IsSuccess ? Error("Erro ao realizar logout", result) : Ok(result);
        }

        [HttpGet("desenvolvedor")]
        public async Task<IActionResult> LoginDeveloper([FromServices] IHostEnvironment env, [FromServices] IConfiguration configuration, CancellationToken cancellation)
        {
            if (env.IsProduction())
            {
                return NotFound();
            }

            var developerCredentials = configuration.GetSection("DeveloperCredentials");

            var email = developerCredentials["Email"] ?? "admin@raizesdonordeste.com";
            var password = developerCredentials["Password"] ?? "somehashedpassword";

            var result = await _loginHandler.HandleAsync(
                new LoginDto(email, password, Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a")), cancellation);

            return !result.IsSuccess ? Error("Erro ao realizar login", result) : Ok(result);
        }
    }
}
