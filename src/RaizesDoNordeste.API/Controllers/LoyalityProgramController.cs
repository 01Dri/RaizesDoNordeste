using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("loyality")]
    [Authorize]
    public class LoyalityProgramController : BaseController
    {
        private readonly IUseCaseHandler<LoyalityJoinRequestDto, LoyalityJoinResponseDto> _joinHandler;
        private readonly IUseCaseHandler<LoyalityLeaveRequestDto, LoyalityLeaveResponseDto> _leaveHandler;

        public LoyalityProgramController(
            IUseCaseHandler<LoyalityJoinRequestDto, LoyalityJoinResponseDto> joinHandler,
            IUseCaseHandler<LoyalityLeaveRequestDto, LoyalityLeaveResponseDto> leaveHandler)
        {
            _joinHandler = joinHandler;
            _leaveHandler = leaveHandler;
        }

        [HttpPost]
        [RolesAuthorize(RoleType.Manager)]
        public async Task<IActionResult> JoinAsync([FromBody] LoyalityJoinRequestDto dto, CancellationToken cancellationToken)
        {
            var result = await _joinHandler.HandleAsync(dto, cancellationToken);
            return result.IsSuccess ? Created("", result.Data) : Error("Erro no programa de fidelidade.", result);
        }

        [HttpDelete]
        public async Task<IActionResult> LeaveAsync([FromBody] LoyalityLeaveRequestDto? dto, CancellationToken cancellationToken)
        {
            dto ??= new LoyalityLeaveRequestDto();
            var result = await _leaveHandler.HandleAsync(dto, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : Error("Erro ao sair do programa de fidelidade.", result);
        }
    }
}
