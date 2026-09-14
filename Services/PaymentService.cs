using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// M14: payment processing. Real Razorpay/Stripe integration is infra
    /// gap G1 — for card/UPI/net-banking this currently issues a stub
    /// "gateway order" and ConfirmAsync accepts any transaction ref in
    /// dev/test mode, so the booking → payment → commission chain can be
    /// built and tested end-to-end before real gateway keys exist. Wallet
    /// payments are real right now (they go through WalletService, which
    /// actually moves the in-app balance).
    /// </summary>
    public class PaymentService
    {
        private readonly AppDbContext _db;
        private readonly WalletService _wallet;
        private readonly CommissionService _commission;
        private readonly NotificationService _notifications;
        private readonly IPaymentGatewayService _gateway;
        private readonly IConfiguration _config;

        public PaymentService(AppDbContext db, WalletService wallet, CommissionService commission, NotificationService notifications, IPaymentGatewayService gateway, IConfiguration config)
        {
            _db = db;
            _wallet = wallet;
            _commission = commission;
            _notifications = notifications;
            _gateway = gateway;
            _config = config;
        }

        public async Task<(bool Success, string Message, InitiatePaymentResponseDto? Data)> InitiateAsync(int userId, InitiatePaymentRequestDto req)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == req.BookingId && b.CustomerId == userId);
            if (booking == null) return (false, "Booking not found.", null);
            if (booking.PaymentStatus == "Paid") return (false, "This booking is already paid.", null);

            var payment = new Payment
            {
                BookingId = booking.Id,
                UserId = userId,
                Amount = booking.TotalAmount,
                Method = req.Method,
                Status = "Pending",
                Gateway = req.Method == "Wallet" ? "Wallet" : "Razorpay",
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Wallet payments settle immediately — no external gateway involved.
            if (req.Method == "Wallet")
            {
                var (ok, message) = await _wallet.AdjustAsync(userId, booking.TotalAmount, "Debit", "BookingPayment", booking.Id);
                if (!ok) return (false, message, null);

                await MarkPaidAsync(payment.Id, $"WALLET-{payment.Id}");
                return (true, "Paid from wallet.", new InitiatePaymentResponseDto
                {
                    PaymentId = payment.Id,
                    Amount = payment.Amount,
                    Gateway = "Wallet",
                    IsStub = false,
                });
            }

            // P0 #2/#3 hardening: real Razorpay order creation, replacing the
            // previous stub that returned a fake "stub_order_{id}" without
            // ever calling a real gateway.
            var (gatewayOk, gatewayOrderId, gatewayError) = await _gateway.CreateOrderAsync(payment.Amount, payment.Id);
            if (!gatewayOk)
            {
                payment.Status = "Failed";
                payment.FailureReason = gatewayError;
                await _db.SaveChangesAsync();
                return (false, gatewayError ?? "Could not initiate payment with the gateway.", null);
            }

            payment.GatewayTransactionRef = gatewayOrderId; // holds the order id until the payment id replaces it on confirmation
            await _db.SaveChangesAsync();

            return (true, "Payment initiated.", new InitiatePaymentResponseDto
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Gateway = payment.Gateway,
                GatewayOrderId = gatewayOrderId,
                GatewayKeyId = _config["ExternalServices:PaymentGateway:KeyId"],
                IsStub = !_gateway.IsConfigured,
            });
        }

        // Verifies the client-side Razorpay signature (HMAC-SHA256 of
        // "{order_id}|{payment_id}" using key_secret) before trusting that
        // the payment actually succeeded — replaces the old ConfirmAsync,
        // which marked payments paid on the client's word alone with no
        // verification at all. The webhook handler below is the more
        // authoritative confirmation path; this covers the immediate
        // client-side "checkout succeeded" callback Razorpay's SDK gives.
        public async Task<(bool Success, string Message)> ConfirmAsync(int userId, ConfirmPaymentRequestDto req)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == req.PaymentId && p.UserId == userId);
            if (payment == null) return (false, "Payment not found.");
            if (payment.Status == "Success") return (false, "Payment already confirmed.");

            var keySecret = _config["ExternalServices:PaymentGateway:KeySecret"];
            if (string.IsNullOrWhiteSpace(keySecret) || keySecret == "REPLACE_VIA_ENV_VAR")
                return (false, "Payment gateway is not configured on this server.");

            var expectedPayload = $"{req.GatewayOrderId}|{req.GatewayPaymentId}";
            var computedSignature = Convert.ToHexString(new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(keySecret)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(expectedPayload))).ToLowerInvariant();
            if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(computedSignature), System.Text.Encoding.UTF8.GetBytes(req.GatewaySignature.Trim().ToLowerInvariant())))
                return (false, "Payment signature verification failed — this payment cannot be trusted as genuine.");

            await MarkPaidAsync(payment.Id, req.GatewayPaymentId);
            return (true, "Payment confirmed.");
        }

        private async Task MarkPaidAsync(int paymentId, string gatewayRef)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return;

            payment.Status = "Success";
            payment.GatewayTransactionRef = gatewayRef;
            payment.CompletedAt = DateTime.UtcNow;

            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == payment.BookingId);
            if (booking != null)
            {
                booking.PaymentStatus = "Paid";
                booking.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            if (booking != null)
            {
                // Batch 21's CommissionService is already real — record the platform/chef split now that money has actually moved.
                await _commission.RecordCommissionAsync(booking.Id, booking.ChefId, booking.TotalAmount);

                await _notifications.CreateAsync(booking.CustomerId, "Payment successful",
                    $"Your payment of ₹{payment.Amount} was received.", "Payment");
                await _notifications.CreateAsync(booking.ChefId, "Booking paid",
                    $"Payment received for booking #{booking.Id}.", "Payment");
            }
        }

        // Called by the verified webhook handler in PaymentController — the
        // authoritative confirmation path, since it comes directly from
        // Razorpay's signed callback rather than the client. Looks the
        // payment up by the gateway order id stored on Initiate (before the
        // payment id exists), so it works even if the client-side Confirm
        // call never happened (app closed, network drop, etc.).
        public async Task<(bool Success, string Message)> ProcessWebhookEventAsync(string eventType, string? gatewayOrderId, string? gatewayPaymentId, string rawPayloadJson)
        {
            if (string.IsNullOrWhiteSpace(gatewayOrderId)) return (false, "Webhook payload missing order id.");
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.GatewayTransactionRef == gatewayOrderId);
            if (payment == null) return (false, $"No payment found for gateway order {gatewayOrderId}.");

            payment.GatewayResponseJson = rawPayloadJson;

            if (eventType is "payment.captured" or "payment.authorized")
            {
                if (payment.Status != "Success") await MarkPaidAsync(payment.Id, gatewayPaymentId ?? gatewayOrderId);
                return (true, "Payment marked successful from webhook.");
            }
            if (eventType == "payment.failed")
            {
                payment.Status = "Failed";
                payment.FailureReason = "Gateway reported payment.failed via webhook.";
                await _db.SaveChangesAsync();
                return (true, "Payment marked failed from webhook.");
            }

            await _db.SaveChangesAsync(); // still persist GatewayResponseJson even for events we don't act on
            return (true, $"Webhook event '{eventType}' received, no state change needed.");
        }

        public async Task<PaymentDto?> GetAsync(int userId, int paymentId)
        {
            var p = await _db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId && x.UserId == userId);
            return p == null ? null : ToDto(p);
        }

        public async Task<List<PaymentDto>> GetForBookingAsync(int userId, int bookingId)
        {
            var payments = await _db.Payments.Where(p => p.BookingId == bookingId && p.UserId == userId).ToListAsync();
            return payments.Select(ToDto).ToList();
        }

        private static PaymentDto ToDto(Payment p) => new()
        {
            Id = p.Id,
            BookingId = p.BookingId,
            Amount = p.Amount,
            Method = p.Method,
            Status = p.Status,
            Gateway = p.Gateway,
            GatewayTransactionRef = p.GatewayTransactionRef,
            CreatedAt = p.CreatedAt,
            CompletedAt = p.CompletedAt,
        };
    }
}
