using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using LovEat.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LovEat.API.Tests.Services
{
    // Minimal fake IHttpClientFactory so RazorpayGatewayService can be
    // constructed without a real network stack — these tests only exercise
    // the signature-verification and IsConfigured logic, which are pure
    // functions of config + input, not network calls.
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new HttpClient();
    }

    public class RazorpayGatewayServiceTests
    {
        private const string TestWebhookSecret = "test-webhook-secret-value";

        private static RazorpayGatewayService BuildService(string? webhookSecret = TestWebhookSecret, string? keyId = "rzp_test_key", string? keySecret = "rzp_test_secret")
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ExternalServices:PaymentGateway:KeyId"] = keyId,
                    ["ExternalServices:PaymentGateway:KeySecret"] = keySecret,
                    ["ExternalServices:PaymentGateway:WebhookSecret"] = webhookSecret,
                })
                .Build();
            return new RazorpayGatewayService(new FakeHttpClientFactory(), config, NullLogger<RazorpayGatewayService>.Instance);
        }

        private static string ComputeValidSignature(string secret, string body)
            => Convert.ToHexString(new HMACSHA256(Encoding.UTF8.GetBytes(secret)).ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();

        [Fact]
        public void VerifyWebhookSignature_ReturnsTrue_ForCorrectSignature()
        {
            var svc = BuildService();
            var body = "{\"event\":\"payment.captured\",\"payload\":{}}";
            var validSignature = ComputeValidSignature(TestWebhookSecret, body);

            Assert.True(svc.VerifyWebhookSignature(body, validSignature));
        }

        [Fact]
        public void VerifyWebhookSignature_ReturnsFalse_ForTamperedBody()
        {
            var svc = BuildService();
            var originalBody = "{\"event\":\"payment.captured\",\"amount\":100}";
            var validSignature = ComputeValidSignature(TestWebhookSecret, originalBody);

            // Attacker changes the amount after the signature was computed —
            // this is exactly the attack signature verification exists to stop.
            var tamperedBody = "{\"event\":\"payment.captured\",\"amount\":999999}";

            Assert.False(svc.VerifyWebhookSignature(tamperedBody, validSignature));
        }

        [Fact]
        public void VerifyWebhookSignature_ReturnsFalse_ForWrongSecret()
        {
            var svc = BuildService();
            var body = "{\"event\":\"payment.captured\"}";
            var signatureFromDifferentSecret = ComputeValidSignature("a-completely-different-secret", body);

            Assert.False(svc.VerifyWebhookSignature(body, signatureFromDifferentSecret));
        }

        [Fact]
        public void VerifyWebhookSignature_ReturnsFalse_WhenSignatureHeaderMissing()
        {
            var svc = BuildService();
            Assert.False(svc.VerifyWebhookSignature("{}", null));
            Assert.False(svc.VerifyWebhookSignature("{}", ""));
        }

        [Fact]
        public void VerifyWebhookSignature_ReturnsFalse_WhenWebhookSecretNotConfigured()
        {
            // Unconfigured must fail closed (reject), never fail open (accept
            // anything) — an unconfigured server should not silently trust
            // unverified webhook calls.
            var svc = BuildService(webhookSecret: null);
            var body = "{}";
            Assert.False(svc.VerifyWebhookSignature(body, "any-signature-at-all"));
        }

        [Fact]
        public void VerifyWebhookSignature_ReturnsFalse_WhenWebhookSecretIsPlaceholder()
        {
            var svc = BuildService(webhookSecret: "REPLACE_VIA_ENV_VAR");
            Assert.False(svc.VerifyWebhookSignature("{}", "any-signature"));
        }

        [Fact]
        public void IsConfigured_ReturnsFalse_WhenKeysAreMissing()
        {
            var svc = BuildService(keyId: null, keySecret: null);
            Assert.False(svc.IsConfigured);
        }

        [Fact]
        public void IsConfigured_ReturnsFalse_WhenKeysArePlaceholders()
        {
            var svc = BuildService(keyId: "REPLACE_VIA_ENV_VAR", keySecret: "REPLACE_VIA_ENV_VAR");
            Assert.False(svc.IsConfigured);
        }

        [Fact]
        public void IsConfigured_ReturnsTrue_WhenRealKeysPresent()
        {
            var svc = BuildService();
            Assert.True(svc.IsConfigured);
        }

        [Fact]
        public async Task CreateOrderAsync_FailsCleanly_WhenNotConfigured_InsteadOfFakingSuccess()
        {
            var svc = BuildService(keyId: null, keySecret: null);
            var (success, orderId, error) = await svc.CreateOrderAsync(500m, paymentId: 1);

            Assert.False(success);
            Assert.Null(orderId);
            Assert.NotNull(error);
        }
    }
}
