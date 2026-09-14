using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M129: Enterprise — Corporate Accounts ──────────────────────
    // A B2B billing entity: a company that books catering/meals for its
    // office/events, with multiple individual User accounts authorized to
    // book against it (paid centrally rather than per-employee).
    public class CorporateAccount
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(120)] public string CompanyName { get; set; } = "";
        [MaxLength(100)] public string? BillingEmail { get; set; }
        [MaxLength(15)] public string? Gstin { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Active"; // Active / Suspended / Closed
        public decimal CreditLimit { get; set; } = 0; // 0 = pay-as-you-go, no credit extended
        public decimal CurrentOutstanding { get; set; } = 0;
        public int OwnerUserId { get; set; } // primary admin contact for the account
        public User? OwnerUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CorporateAccountMember
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [MaxLength(20)] public string Role { get; set; } = "Booker"; // Booker / Approver / Owner
        public bool IsActive { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M130: Enterprise — Bulk Ordering ───────────────────────────
    // One request for many meals at once (office lunch, event catering) —
    // a batch of individual Bookings gets created once the request is
    // approved, rather than the customer placing dozens of separate orders.
    public class BulkOrderRequest
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        public int RequestedByUserId { get; set; }
        public User? RequestedByUser { get; set; }
        [MaxLength(60)] public string? City { get; set; }
        public int MealCount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal EstimatedTotal { get; set; }
        public decimal? VolumeDiscountPercent { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "PendingApproval"; // PendingApproval / Approved / Rejected / Fulfilled
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
    }

    public class BulkOrderLineItem
    {
        [Key] public int Id { get; set; }
        public int BulkOrderRequestId { get; set; }
        public BulkOrderRequest? BulkOrderRequest { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? CreatedBookingId { get; set; } // set once fulfilled and the real Booking exists
    }

    // ── M131: Enterprise — API Key Management ──────────────────────
    // Lets enterprise partners integrate directly (e.g. a corporate HR
    // portal auto-booking team lunches) via scoped, rate-limited keys
    // instead of a logged-in user session.
    public class ApiKey
    {
        [Key] public int Id { get; set; }
        [MaxLength(120)] public string Label { get; set; } = "";
        [MaxLength(64)] public string KeyPrefix { get; set; } = ""; // first 8 chars shown in UI, e.g. "lk_live_a1b2c3d4"
        [MaxLength(128)] public string KeyHash { get; set; } = ""; // SHA-256 of the full key — full key is shown once at creation, never stored
        [MaxLength(300)] public string Scopes { get; set; } = "read"; // comma-separated: read / bookings:write / webhooks:manage
        public int? CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        public int RateLimitPerMinute { get; set; } = 60;
        public bool IsActive { get; set; } = true;
        public int CreatedByAdminId { get; set; }
        public User? CreatedByAdmin { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
    }

    // ── M132: Enterprise — Webhook Management ──────────────────────
    public class WebhookSubscription
    {
        [Key] public int Id { get; set; }
        public int? CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        [MaxLength(500)] public string TargetUrl { get; set; } = "";
        [MaxLength(300)] public string EventTypes { get; set; } = ""; // comma-separated: booking.completed / invoice.generated / refund.processed
        [MaxLength(64)] public string SigningSecret { get; set; } = ""; // used to HMAC-sign delivered payloads
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WebhookDeliveryLog
    {
        [Key] public int Id { get; set; }
        public int WebhookSubscriptionId { get; set; }
        public WebhookSubscription? WebhookSubscription { get; set; }
        [MaxLength(60)] public string EventType { get; set; } = "";
        public string Payload { get; set; } = "{}";
        [MaxLength(20)] public string Status { get; set; } = "Pending"; // Pending / Delivered / Failed
        public int? ResponseStatusCode { get; set; }
        public int AttemptCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
    }
}
