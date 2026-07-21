using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.Core.Loyalit.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers
{

    [ApiController]
    [Route("loyality")]
    [Authorize]
    public class LoyalityProgramController : BaseController
    {

        private readonly IUseCaseHandler<LoyalityJoinResponseDto> _handler;

        public LoyalityProgramController(IUseCaseHandler<LoyalityJoinResponseDto> handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> JoinAsync(CancellationToken cancellationToken)
        {
            var result = await _handler.HandleAsync(cancellationToken);
            if (result.IsSuccess)
            {
                return Created("", result.Data);
            }
            return Error("Erro no programa de fidelidade.", result);

        }
    }
}
