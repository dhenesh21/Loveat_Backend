using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LovEat.API.Services
{
    /// <summary>
    /// P0 #2/#3 hardening: real payment gateway integration, replacing the
    /// previous stub in PaymentService.InitiateAsync (which returned a fake
    /// "stub_order_{id}" and never called out to any real gateway).
    ///
    /// This talks to Razorpay's real REST API. It degrades gracefully when
    /// no keys are configured (returns a clear failure message instead of a
    /// silent fake success) so local dev without real Razorpay keys still
    /// works for wallet payments and doesn't crash for card/UPI ones — it
    /// just correctly reports "not configured" rather than pretending to
    /// succeed.
    /// </summary>
    public interface IPaymentGatewayService
    {
        Task<(bool Success, string? GatewayOrderId, string? ErrorMessage)> CreateOrderAsync(decimal amountRupees, int paymentId);
        bool VerifyWebhookSignature(string rawRequestBody, string? signatureHeader);
        bool IsConfigured { get; }
    }

    public class RazorpayGatewayService : IPaymentGatewayService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<RazorpayGatewayService> _logger;

        public RazorpayGatewayService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<RazorpayGatewayService> logger)
        {
            _http = httpClientFactory.CreateClient();
            _config = config;
            _logger = logger;
        }

        private string? KeyId => _config["ExternalServices:PaymentGateway:KeyId"];
        private string? KeySecret => _config["ExternalServices:PaymentGateway:KeySecret"];
        private string? WebhookSecret => _config["ExternalServices:PaymentGateway:WebhookSecret"];

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(KeyId) && KeyId != "REPLACE_VIA_ENV_VAR" &&
            !string.IsNullOrWhiteSpace(KeySecret) && KeySecret != "REPLACE_VIA_ENV_VAR";

        // Creates a real order via POST https://api.razorpay.com/v1/orders.
        // Amount must be sent to Razorpay in the smallest currency unit
        // (paise for INR), hence * 100. `receipt` lets us tie the gateway's
        // order back to our own Payment row for reconciliation.
        public async Task<(bool Success, string? GatewayOrderId, string? ErrorMessage)> CreateOrderAsync(decimal amountRupees, int paymentId)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Razorpay order creation skipped — ExternalServices:PaymentGateway:KeyId/KeySecret not configured.");
                return (false, null, "Payment gateway is not configured on this server. Set ExternalServices:PaymentGateway:KeyId and KeySecret.");
            }

            var authBytes = Encoding.UTF8.GetBytes($"{KeyId}:{KeySecret}");
            var payload = JsonSerializer.Serialize(new
            {
                amount = (long)Math.Round(amountRupees * 100),
                currency = "INR",
                receipt = $"loveat_payment_{paymentId}",
                payment_capture = 1,
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/orders")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            try
            {
                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Razorpay order creation failed ({StatusCode}): {Body}", response.StatusCode, body);
                    return (false, null, "The payment gateway rejected the order request. Please try again.");
                }

                using var doc = JsonDocument.Parse(body);
                var orderId = doc.RootElement.GetProperty("id").GetString();
                return (true, orderId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Razorpay order creation threw an exception for payment {PaymentId}", paymentId);
                return (false, null, "Could not reach the payment gateway. Please try again.");
            }
        }

        // Razorpay signs webhook payloads with HMAC-SHA256 over the raw
        // request body using the configured webhook secret, delivered in
        // the X-Razorpay-Signature header. Constant-time comparison avoids
        // a timing side-channel on the signature check.
        public bool VerifyWebhookSignature(string rawRequestBody, string? signatureHeader)
        {
            if (string.IsNullOrWhiteSpace(WebhookSecret) || WebhookSecret == "REPLACE_VIA_ENV_VAR")
            {
                _logger.LogWarning("Webhook signature check skipped — ExternalServices:PaymentGateway:WebhookSecret not configured. Rejecting webhook.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(signatureHeader)) return false;

            var computedHash = Convert.ToHexString(new HMACSHA256(Encoding.UTF8.GetBytes(WebhookSecret)).ComputeHash(Encoding.UTF8.GetBytes(rawRequestBody))).ToLowerInvariant();
            var providedHash = signatureHeader.Trim().ToLowerInvariant();

            var computedBytes = Encoding.UTF8.GetBytes(computedHash);
            var providedBytes = Encoding.UTF8.GetBytes(providedHash);
            if (computedBytes.Length != providedBytes.Length) return false;
            return CryptographicOperations.FixedTimeEquals(computedBytes, providedBytes);
        }
    }
}
