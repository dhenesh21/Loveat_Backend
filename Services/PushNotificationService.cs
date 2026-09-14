using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace LovEat.API.Services
{
    /// <summary>
    /// P1 hardening: real push delivery via FCM HTTP v1 API, replacing the
    /// Console.WriteLine stub in NotificationService.PushToDeviceAsync.
    ///
    /// FCM's legacy server-key API is deprecated; the current v1 API needs
    /// an OAuth2 access token minted from a Google service-account JSON key
    /// (ExternalServices:Push:ServiceAccountJson — the whole JSON file
    /// contents, not just a key string) plus the Firebase project id.
    /// Degrades gracefully with a clear log + no-op when not configured.
    /// </summary>
    public interface IPushNotificationService
    {
        Task<(bool Success, string? ErrorMessage)> SendAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
        bool IsConfigured { get; }
    }

    public class FcmPushNotificationService : IPushNotificationService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<FcmPushNotificationService> _logger;
        private string? _cachedAccessToken;
        private DateTime _accessTokenExpiresAt = DateTime.MinValue;

        public FcmPushNotificationService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<FcmPushNotificationService> logger)
        {
            _http = httpClientFactory.CreateClient();
            _config = config;
            _logger = logger;
        }

        private string? ProjectId => _config["ExternalServices:Push:ProjectId"];
        private string? ServiceAccountJson => _config["ExternalServices:Push:ServiceAccountJson"];

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ProjectId) && ProjectId != "REPLACE_VIA_ENV_VAR" &&
            !string.IsNullOrWhiteSpace(ServiceAccountJson) && ServiceAccountJson != "REPLACE_VIA_ENV_VAR";

        public async Task<(bool Success, string? ErrorMessage)> SendAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Push send skipped for token {TokenPrefix}... — ExternalServices:Push:ProjectId/ServiceAccountJson not configured. [DEV LOG] {Title} — {Body}",
                    deviceToken.Length > 8 ? deviceToken[..8] : deviceToken, title, body);
                return (false, "Push notification provider is not configured on this server.");
            }

            var accessToken = await GetAccessTokenAsync();
            if (accessToken == null) return (false, "Could not authenticate with the push notification provider.");

            var payload = JsonSerializer.Serialize(new
            {
                message = new
                {
                    token = deviceToken,
                    notification = new { title, body },
                    data = data ?? new Dictionary<string, string>(),
                },
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://fcm.googleapis.com/v1/projects/{ProjectId}/messages:send")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var respBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("FCM push send failed ({StatusCode}): {Body}", response.StatusCode, respBody);
                    return (false, "Could not send push notification.");
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FCM push send threw an exception.");
                return (false, "Could not reach the push notification provider.");
            }
        }

        // Mints (and caches, refreshing shortly before expiry) a Google OAuth2
        // access token by signing a JWT with the service account's private key
        // — the standard "service account flow" for calling Google APIs
        // server-to-server without a user present.
        private async Task<string?> GetAccessTokenAsync()
        {
            if (_cachedAccessToken != null && DateTime.UtcNow < _accessTokenExpiresAt.AddMinutes(-2))
                return _cachedAccessToken;

            try
            {
                using var doc = JsonDocument.Parse(ServiceAccountJson!);
                var clientEmail = doc.RootElement.GetProperty("client_email").GetString();
                var privateKey = doc.RootElement.GetProperty("private_key").GetString();
                var tokenUri = doc.RootElement.TryGetProperty("token_uri", out var tu) ? tu.GetString() : "https://oauth2.googleapis.com/token";

                var now = DateTimeOffset.UtcNow;
                var header = Base64UrlEncode(JsonSerializer.Serialize(new { alg = "RS256", typ = "JWT" }));
                var claims = Base64UrlEncode(JsonSerializer.Serialize(new
                {
                    iss = clientEmail,
                    scope = "https://www.googleapis.com/auth/firebase.messaging",
                    aud = tokenUri,
                    iat = now.ToUnixTimeSeconds(),
                    exp = now.AddHours(1).ToUnixTimeSeconds(),
                }));
                var unsigned = $"{header}.{claims}";

                using var rsa = System.Security.Cryptography.RSA.Create();
                rsa.ImportFromPem(privateKey);
                var signature = rsa.SignData(Encoding.UTF8.GetBytes(unsigned), System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                var jwt = $"{unsigned}.{Base64UrlEncode(signature)}";

                using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUri)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                        ["assertion"] = jwt,
                    }),
                };
                var tokenResponse = await _http.SendAsync(tokenRequest);
                var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("FCM OAuth2 token request failed: {Body}", tokenBody);
                    return null;
                }

                using var tokenDoc = JsonDocument.Parse(tokenBody);
                _cachedAccessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
                _accessTokenExpiresAt = now.AddSeconds(tokenDoc.RootElement.GetProperty("expires_in").GetInt32()).UtcDateTime;
                return _cachedAccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mint FCM OAuth2 access token — check ServiceAccountJson is valid.");
                return null;
            }
        }

        private static string Base64UrlEncode(string input) => Base64UrlEncode(Encoding.UTF8.GetBytes(input));
        private static string Base64UrlEncode(byte[] input) => Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
