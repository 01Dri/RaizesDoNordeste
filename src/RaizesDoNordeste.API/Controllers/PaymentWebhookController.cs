using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.API.Attributes;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Ingredients.Enums;
using RaizesDoNordeste.Domain.Core.Payments;
using RaizesDoNordeste.Domain.Services;
using System;
using System.Threading.Tasks;

namespace RaizesDoNordeste.API.Controllers
{
    [ApiController]
    [Route("pagamento/webhook")]
    [UninterPaymentAuthorize]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoyalityProgramService _loyalityProgramService;

        public PaymentWebhookController(ApplicationDbContext dbContext, ILoyalityProgramService loyalityProgramService)
        {
            _dbContext = dbContext;
            _loyalityProgramService = loyalityProgramService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveNotification([FromBody] WebhookPayload dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid payload.");
            }

            var order = await _dbContext.Orders
                .Include(o => o.PaymentOrder)
                .ThenInclude(po => po.Payment)
                .FirstOrDefaultAsync(o => o.PublicId == dto.OrderId);

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

                // Update existing waiting payment
                payment.Status = PaymentStatus.Paid;
                payment.TotalPaid = dto.Amount;
                payment.ExternalPaymentId = dto.TransactionId;
                payment.Description = "Pagamento Pix aprovado via webhook.";

                // Earn loyalty points upon confirmation of Pix payment
                await _loyalityProgramService.EarnPointsAsync(
                    dto.Amount,
                    order.AccountId.GetValueOrDefault(),
                    order.RestaurantId
                );

                await _dbContext.SaveChangesAsync();
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
