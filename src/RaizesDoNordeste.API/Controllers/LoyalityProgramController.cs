using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.UseCases;
using System.Threading;
using System.Threading.Tasks;

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
            if (result.IsSuccess)
            {
                return Created("", result.Data);
            }
            return Error("Erro no programa de fidelidade.", result);
        }

        [HttpDelete]
        public async Task<IActionResult> LeaveAsync([FromBody] LoyalityLeaveRequestDto? dto, CancellationToken cancellationToken)
        {
            dto ??= new LoyalityLeaveRequestDto();
            var result = await _leaveHandler.HandleAsync(dto, cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return Error("Erro ao sair do programa de fidelidade.", result);
        }
    }
}
