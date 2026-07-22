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
        private readonly IUseCaseHandler<LoyalityJoinRequestDto, LoyalityJoinResponseDto> _handler;

        public LoyalityProgramController(IUseCaseHandler<LoyalityJoinRequestDto, LoyalityJoinResponseDto> handler)
        {
            _handler = handler;
        }

        [HttpPost]
        [RolesAuthorize(RoleType.Manager)]
        public async Task<IActionResult> JoinAsync([FromBody] LoyalityJoinRequestDto dto, CancellationToken cancellationToken)
        {
            var result = await _handler.HandleAsync(dto, cancellationToken);
            if (result.IsSuccess)
            {
                return Created("", result.Data);
            }
            return Error("Erro no programa de fidelidade.", result);
        }
    }
}
