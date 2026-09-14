using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LovEat.API.Services
{
    /// <summary>
    /// P1 hardening: real SMS delivery via MSG91, replacing the previous
    /// Console.WriteLine stub in OtpService. Degrades gracefully when no
    /// API key is configured — returns a clear failure instead of silently
    /// pretending to send (the old stub's biggest problem: it always
    /// "succeeded" even though nothing was ever sent).
    /// </summary>
    public interface ISmsService
    {
        Task<(bool Success, string? ErrorMessage)> SendAsync(string phoneNumber, string message);
        bool IsConfigured { get; }
    }

    public class Msg91SmsService : ISmsService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<Msg91SmsService> _logger;

        public Msg91SmsService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<Msg91SmsService> logger)
        {
            _http = httpClientFactory.CreateClient();
            _config = config;
            _logger = logger;
        }

        private string? ApiKey => _config["ExternalServices:Sms:ApiKey"];
        private string SenderId => _config["ExternalServices:Sms:SenderId"] ?? "LOVEAT";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "REPLACE_VIA_ENV_VAR";

        // MSG91's Flow/SendSMS v5 API: POST https://control.msg91.com/api/v5/flow/
        // (or the simpler /api/v5/otp endpoint for OTP-only flows). Using the
        // generic send-SMS endpoint here so it also covers non-OTP transactional
        // texts (booking confirmations, alerts) with the same service.
        public async Task<(bool Success, string? ErrorMessage)> SendAsync(string phoneNumber, string message)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("SMS send skipped for {Phone} — ExternalServices:Sms:ApiKey not configured. [DEV LOG] Message would have been: {Message}", phoneNumber, message);
                return (false, "SMS provider is not configured on this server.");
            }

            // MSG91 expects mobile numbers without a leading '+' and with country code, e.g. 91XXXXXXXXXX.
            var normalizedPhone = phoneNumber.TrimStart('+');

            var payload = JsonSerializer.Serialize(new
            {
                sender = SenderId,
                route = "4", // transactional route
                country = "91",
                sms = new[] { new { message, to = new[] { normalizedPhone } } },
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://control.msg91.com/api/v5/flow/")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.TryAddWithoutValidation("authkey", ApiKey);

            try
            {
                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("MSG91 SMS send failed for {Phone} ({StatusCode}): {Body}", phoneNumber, response.StatusCode, body);
                    return (false, "Could not send SMS. Please try again.");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MSG91 SMS send threw an exception for {Phone}", phoneNumber);
                return (false, "Could not reach the SMS provider. Please try again.");
            }
        }
    }
}
