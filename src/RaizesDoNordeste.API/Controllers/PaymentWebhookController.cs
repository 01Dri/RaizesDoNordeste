using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Services;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("pagamento/webhook")]
    [UninterPaymentAuthorize]
    public class PaymentWebhookController : RaizesDoNordesteController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPaymentTransactionService _paymentTransactionService;

        public PaymentWebhookController(ApplicationDbContext dbContext, IPaymentTransactionService paymentTransactionService)
        {
            _dbContext = dbContext;
            _paymentTransactionService = paymentTransactionService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveNotification([FromBody] WebhookPayload dto, CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Orders
                .Include(o => o.PaymentOrder)
                .ThenInclude(po => po.Payment)
                .FirstOrDefaultAsync(o => o.PublicId == dto.OrderId, cancellationToken);

            if (order == null)
            {
                return NotFound($"Pedido {dto.OrderId} não encontrado.");
            }

            if (dto.Status != "Approved") return BadRequest($"Status de pagamento não suportado: {dto.Status}");
            var paymentOrder = order.PaymentOrder;
            var payment = paymentOrder?.Payment;

            if (payment == null)
            {
                return BadRequest("Não foi encontrado um registro de pagamento para este pedido.");
            }

            await _paymentTransactionService.ConfirmPaymentAsync(
                payment,
                order.AccountId.GetValueOrDefault(),
                order.RestaurantId,
                dto.Amount,
                dto.TransactionId,
                "Pagamento Pix aprovado via webhook.",
                cancellationToken
            );

            return Ok(new { Message = "Pagamento processado com sucesso e status atualizado." });

        }

        public class WebhookPayload
        {
            public string TransactionId { get; init; } = null!;
            public Guid OrderId { get; init; }
            public string Status { get; init; } = null!;
            public decimal Amount { get; init; }
        }
    }
}
