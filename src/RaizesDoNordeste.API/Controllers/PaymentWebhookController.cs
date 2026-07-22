using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("pagamento/webhook")]
    [UninterPaymentAuthorize]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPaymentTransactionService _paymentTransactionService;

        public PaymentWebhookController(ApplicationDbContext dbContext, IPaymentTransactionService paymentTransactionService)
        {
            _dbContext = dbContext;
            _paymentTransactionService = paymentTransactionService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveNotification([FromBody] WebhookPayload dto, CancellationToken cancellationToken)
        {
            if (dto == null)
            {
                return BadRequest("Invalid payload.");
            }

            var order = await _dbContext.Orders
                .Include(o => o.PaymentOrder)
                .ThenInclude(po => po.Payment)
                .FirstOrDefaultAsync(o => o.PublicId == dto.OrderId, cancellationToken);

            if (order == null)
            {
                return NotFound($"Order {dto.OrderId} not found.");
            }

            if (dto.Status == "Approved")
            {
                var paymentOrder = order.PaymentOrder;
                Payment? payment = paymentOrder?.Payment;

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

            return BadRequest($"Unsupported payment status: {dto.Status}");
        }

        public class WebhookPayload
        {
            public string TransactionId { get; set; } = null!;
            public Guid OrderId { get; set; }
            public string Status { get; set; } = null!;
            public decimal Amount { get; set; }
        }
    }
}
