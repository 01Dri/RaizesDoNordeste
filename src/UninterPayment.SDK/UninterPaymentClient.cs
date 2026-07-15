using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace UninterPayment.SDK
{
    public class UninterPaymentClient : IUninterPaymentClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _workerUrl;

        public UninterPaymentClient(HttpClient? httpClient = null, string? workerUrl = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _workerUrl = workerUrl ?? "http://localhost:5200";
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Amount <= 0)
            {
                return new PaymentResult
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Status = UninterPaymentStatus.Failed,
                    ErrorMessage = "Amount must be greater than zero."
                };
            }

            if (request.Method == UninterPaymentMethod.CreditCard)
            {
                // Credit card payments are approved immediately
                return new PaymentResult
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Status = UninterPaymentStatus.Approved
                };
            }
            else if (request.Method == UninterPaymentMethod.Pix)
            {
                string transactionId = Guid.NewGuid().ToString();

                // Send the payment to the worker queue via HTTP
                var payload = new
                {
                    TransactionId = transactionId,
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    WebhookUrl = request.WebhookUrl
                };

                try
                {
                    string url = $"{_workerUrl.TrimEnd('/')}/payments";
                    var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        return new PaymentResult
                        {
                            TransactionId = transactionId,
                            Status = UninterPaymentStatus.Waiting
                        };
                    }
                    else
                    {
                        string errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                        return new PaymentResult
                        {
                            TransactionId = transactionId,
                            Status = UninterPaymentStatus.Failed,
                            ErrorMessage = $"Worker returned error: {response.StatusCode} - {errorText}"
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new PaymentResult
                    {
                        TransactionId = transactionId,
                        Status = UninterPaymentStatus.Failed,
                        ErrorMessage = $"Failed to communicate with worker: {ex.Message}"
                    };
                }
            }
            else
            {
                return new PaymentResult
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Status = UninterPaymentStatus.Failed,
                    ErrorMessage = "Unsupported payment method."
                };
            }
        }
    }
}
