using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.Core.Payments.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers;


[ApiController]
[Route("pagamento")]
[Authorize]
public class PaymentController : BaseController
{
    private readonly IUseCaseHandler<PaymentRequestDto, PaymentResponseDto> _paymentHandler;

    public PaymentController(IUseCaseHandler<PaymentRequestDto, PaymentResponseDto> paymentHandler)
    {
        _paymentHandler = paymentHandler;
    }

    [HttpPost("Order/{orderId}")]
    public async Task<IActionResult> PayAsync([FromRoute] Guid orderId, [FromBody] PaymentRequestDto dto, CancellationToken cancellationToken)
    {
        dto.OrderId = orderId;
        var result = await _paymentHandler.HandleAsync(dto, cancellationToken);
        if (result.IsSuccess)
        {
            return Created("", result);
        }
        return Error("Falha ao processar o pagamento", result);
    }

}
