using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _svc;
        private readonly IPaymentGatewayService _gateway;
        private readonly ILogger<PaymentController> _logger;
        public PaymentController(PaymentService svc, IPaymentGatewayService gateway, ILogger<PaymentController> logger)
        {
            _svc = svc;
            _gateway = gateway;
            _logger = logger;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequestDto req)
        {
            var (success, message, data) = await _svc.InitiateAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        /// <summary>Client-side confirmation after Razorpay checkout completes — verifies the HMAC signature Razorpay's SDK returns before trusting it (see PaymentService.ConfirmAsync). The webhook below is still the more authoritative path since it comes directly from Razorpay's servers, not the client.</summary>
        [HttpPost("confirm")]
        [Authorize]
        public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequestDto req)
        {
            var (success, message) = await _svc.ConfirmAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _svc.GetAsync(UserId, id);
            if (data == null) return NotFound(new { success = false, message = "Payment not found." });
            return Ok(new { success = true, data });
        }

        [HttpGet("booking/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> ForBooking(int bookingId)
        {
            var data = await _svc.GetForBookingAsync(UserId, bookingId);
            return Ok(new { success = true, data });
        }

        /// <summary>
        /// P0 #3 hardening: real signature-verified webhook handler.
        /// Reads the raw request body (required for HMAC verification —
        /// model-bound JSON loses the exact byte sequence Razorpay signed),
        /// checks X-Razorpay-Signature against ExternalServices:PaymentGateway:WebhookSecret,
        /// and rejects anything that doesn't verify. Point your Razorpay
        /// dashboard's webhook URL at this endpoint once WebhookSecret is set.
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
            if (!_gateway.VerifyWebhookSignature(rawBody, signature))
            {
                _logger.LogWarning("Rejected payment webhook — signature verification failed or gateway not configured.");
                return Unauthorized(new { success = false, message = "Signature verification failed." });
            }

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var eventType = doc.RootElement.GetProperty("event").GetString() ?? "";
                var paymentEntity = doc.RootElement.GetProperty("payload").GetProperty("payment").GetProperty("entity");
                var gatewayPaymentId = paymentEntity.GetProperty("id").GetString();
                var gatewayOrderId = paymentEntity.TryGetProperty("order_id", out var orderIdProp) ? orderIdProp.GetString() : null;

                var (success, message) = await _svc.ProcessWebhookEventAsync(eventType, gatewayOrderId, gatewayPaymentId, rawBody);
                if (!success) _logger.LogWarning("Payment webhook processing issue: {Message}", message);
                return Ok(new { received = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse verified payment webhook payload.");
                return Ok(new { received = true }); // acknowledge receipt so the gateway doesn't retry-storm; error is logged for investigation
            }
        }
    }
}
