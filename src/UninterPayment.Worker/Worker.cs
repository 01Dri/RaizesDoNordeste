using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UninterPayment.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly PaymentQueue _paymentQueue;
        private readonly HttpClient _httpClient;

        public Worker(ILogger<Worker> logger, PaymentQueue paymentQueue)
        {
            _logger = logger;
            _paymentQueue = paymentQueue;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UninterPayment.Worker background job started. Waiting for Pix payments in queue...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Dequeue payment item (blocks asynchronously until an item is available)
                    var payment = await _paymentQueue.DequeueAsync(stoppingToken);

                    _logger.LogInformation("Dequeued Pix payment for Order {OrderId} (Tx: {TransactionId}, Amount: {Amount})", 
                        payment.OrderId, payment.TransactionId, payment.Amount);

                    // Simulate processing delay (e.g., waiting for Pix bank reconciliation)
                    _logger.LogInformation("Waiting 10 seconds to simulate Pix bank reconciliation...");
                    await Task.Delay(10000, stoppingToken);

                    if (!string.IsNullOrEmpty(payment.WebhookUrl))
                    {
                        var payload = new WebhookPayload
                        {
                            TransactionId = payment.TransactionId,
                            OrderId = payment.OrderId,
                            Status = "Approved",
                            Amount = payment.Amount
                        };

                        _logger.LogInformation("Notifying webhook at {WebhookUrl}...", payment.WebhookUrl);

                        try
                        {
                            var requestMessage = new HttpRequestMessage(HttpMethod.Post, payment.WebhookUrl)
                            {
                                Content = JsonContent.Create(payload)
                            };
                            requestMessage.Headers.Add("X-UninterPayment-Key", "UninterSecretWebhookToken123!");

                            var response = await _httpClient.SendAsync(requestMessage, stoppingToken);

                            if (response.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("Webhook notified successfully for Order {OrderId}.", payment.OrderId);
                            }
                            else
                            {
                                string errorText = await response.Content.ReadAsStringAsync(stoppingToken);
                                _logger.LogWarning("Webhook returned status {StatusCode}: {Error}", response.StatusCode, errorText);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to call webhook at {WebhookUrl} for Order {OrderId}", payment.WebhookUrl, payment.OrderId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No WebhookUrl provided for Order {OrderId}. Processed locally only.", payment.OrderId);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in worker execution loop.");
                }
            }
        }

        private class WebhookPayload
        {
            public string TransactionId { get; set; } = null!;
            public Guid OrderId { get; set; }
            public string Status { get; set; } = null!;
            public decimal Amount { get; set; }
        }
    }
}
