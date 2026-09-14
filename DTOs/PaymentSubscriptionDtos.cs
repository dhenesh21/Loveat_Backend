namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M14: Payment
    // (Subscription (M19) DTOs to be added here in Phase 6.)
    // ══════════════════════════════════════════════════════════════
    public class InitiatePaymentRequestDto
    {
        public int BookingId { get; set; }
        public string Method { get; set; } = "UPI"; // UPI / Card / NetBanking / Wallet
    }

    // Note: the webhook is now handled by reading the raw request body
    // directly in PaymentController.Webhook (needed for HMAC signature
    // verification) rather than a model-bound DTO — see
    // PaymentGatewayService.VerifyWebhookSignature.

    public class InitiatePaymentResponseDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Gateway { get; set; } = "Razorpay";

        /// <summary>Real Razorpay order ID (from a live CreateOrderAsync call) the client SDK opens checkout with. Null if the gateway isn't configured or the request was a Wallet payment (settles instantly, no gateway order needed).</summary>
        public string? GatewayOrderId { get; set; }

        /// <summary>The Razorpay key_id (public, safe to expose) the client SDK needs to initialize checkout — not the secret.</summary>
        public string? GatewayKeyId { get; set; }

        public bool IsStub { get; set; } = true;
    }

    // Standard Razorpay client-side verification payload: after the user
    // completes checkout, the client SDK returns these three values. The
    // server recomputes HMAC-SHA256(orderId + "|" + paymentId, key_secret)
    // and compares it to the signature — this is what actually proves the
    // payment happened, not just trusting whatever the client claims.
    public class ConfirmPaymentRequestDto
    {
        public int PaymentId { get; set; }
        public string GatewayOrderId { get; set; } = "";
        public string GatewayPaymentId { get; set; } = "";
        public string GatewaySignature { get; set; } = "";
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public string Status { get; set; } = "";
        public string Gateway { get; set; } = "";
        public string? GatewayTransactionRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// TODO (G1): shape of a Razorpay/Stripe webhook payload once wired.
    /// PaymentController.Webhook currently accepts this loosely and does NOT
    /// verify a signature yet — signature verification against
    /// ExternalServices:PaymentGateway:WebhookSecret must be added before
    /// this endpoint is exposed to the real internet.
    /// </summary>
    // ══════════════════════════════════════════════════════════════
    // M19: Subscription (customer weekly/monthly meal plans)
    // ══════════════════════════════════════════════════════════════
    public class SubscriptionPlanDto
    {
        public string PlanName { get; set; } = "";
        public int MealsPerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public List<string> Features { get; set; } = new();
    }

    public class CreateSubscriptionRequestDto
    {
        public string PlanName { get; set; } = "";
    }

    public class SubscriptionDto
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = "";
        public int MealsPerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public string Status { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        // M182: Dedicated/Same-Chef Monthly Assignment
        public int? PreferredChefId { get; set; }
        public string? PreferredChefName { get; set; }
        public bool AutoAssignSameChef { get; set; }
        public string FallbackPolicy { get; set; } = "NotifyCustomer";
    }
}
