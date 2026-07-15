using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UninterPayment.Worker
{
    public class PaymentQueueItem
    {
        public string TransactionId { get; set; } = null!;
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? WebhookUrl { get; set; }
    }

    public class PaymentQueue
    {
        private readonly Channel<PaymentQueueItem> _channel;

        public PaymentQueue()
        {
            _channel = Channel.CreateUnbounded<PaymentQueueItem>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(PaymentQueueItem item, CancellationToken cancellationToken = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            await _channel.Writer.WriteAsync(item, cancellationToken);
        }

        public async ValueTask<PaymentQueueItem> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
