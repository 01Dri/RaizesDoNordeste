using System;
using System.Threading;
using System.Threading.Tasks;

namespace UninterPayment.SDK
{
    public enum UninterPaymentMethod
    {
        CreditCard,
        Pix
    }

    public enum UninterPaymentStatus
    {
        Approved,
        Waiting,
        Failed
    }

    public class PaymentRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public UninterPaymentMethod Method { get; set; }
        public string? CardNumber { get; set; }
        public string? PixKey { get; set; }
        public string? WebhookUrl { get; set; }
    }

    public class PaymentResult
    {
        public string TransactionId { get; set; } = null!;
        public UninterPaymentStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IUninterPaymentClient
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    }
}
