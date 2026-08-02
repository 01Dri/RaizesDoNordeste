using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Domain.Core.Payments.DTO;
using RaizesDoNordeste.Domain.UseCases;

namespace RaizesDoNordeste.API.Controllers;


[ApiController]
[Route("pagamento")]
[Authorize]
public class PaymentController : RaizesDoNordesteController
{
    private readonly IUseCaseHandler<PaymentRequestDto, PaymentResponseDto> _paymentHandler;

    public PaymentController(IUseCaseHandler<PaymentRequestDto, PaymentResponseDto> paymentHandler)
    {
        _paymentHandler = paymentHandler;
    }

    [HttpPost("pedido/{orderId}")]
    public async Task<IActionResult> Pay([FromRoute] Guid orderId, [FromBody] PaymentRequestDto dto, CancellationToken cancellationToken)
    {
        dto.OrderId = orderId;
        var result = await _paymentHandler.HandleAsync(dto, cancellationToken);
        return result.IsSuccess ? Created(string.Empty, result) : Error("Falha ao processar o pagamento", result);
    }

}
