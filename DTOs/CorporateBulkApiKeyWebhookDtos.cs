namespace LovEat.API.DTOs
{
    // ── M129: Enterprise — Corporate Accounts ──────────────────────
    public class CreateCorporateAccountRequestDto
    {
        public string CompanyName { get; set; } = "";
        public string? BillingEmail { get; set; }
        public string? Gstin { get; set; }
        public decimal CreditLimit { get; set; } = 0;
        public int OwnerUserId { get; set; }
    }

    public class CorporateAccountDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string? BillingEmail { get; set; }
        public string? Gstin { get; set; }
        public string Status { get; set; } = "";
        public decimal CreditLimit { get; set; }
        public decimal CurrentOutstanding { get; set; }
        public string? OwnerName { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddCorporateMemberRequestDto
    {
        public int CorporateAccountId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = "Booker";
    }

    public class CorporateMemberDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
    }

    // ── M130: Enterprise — Bulk Ordering ────────────────────────────
    public class BulkOrderLineItemRequestDto
    {
        public int ChefId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateBulkOrderRequestDto
    {
        public int CorporateAccountId { get; set; }
        public int RequestedByUserId { get; set; }
        public string? City { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<BulkOrderLineItemRequestDto> LineItems { get; set; } = new();
    }

    public class BulkOrderLineItemDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? CreatedBookingId { get; set; }
    }

    public class BulkOrderRequestDto
    {
        public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public string? CompanyName { get; set; }
        public string? RequestedByName { get; set; }
        public string? City { get; set; }
        public int MealCount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal EstimatedTotal { get; set; }
        public decimal? VolumeDiscountPercent { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public List<BulkOrderLineItemDto> LineItems { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class DecideBulkOrderRequestDto
    {
        public int BulkOrderRequestId { get; set; }
        public bool Approve { get; set; } = true;
    }

    public class FulfillBulkOrderRequestDto { public int BulkOrderRequestId { get; set; } }

    // ── M131: Enterprise — API Key Management ──────────────────────
    public class CreateApiKeyRequestDto
    {
        public string Label { get; set; } = "";
        public string Scopes { get; set; } = "read";
        public int? CorporateAccountId { get; set; }
        public int RateLimitPerMinute { get; set; } = 60;
    }

    public class ApiKeyCreatedDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public string FullKey { get; set; } = ""; // shown once, at creation only
        public string KeyPrefix { get; set; } = "";
        public string Scopes { get; set; } = "";
    }

    public class ApiKeyDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public string KeyPrefix { get; set; } = "";
        public string Scopes { get; set; } = "";
        public string? CorporateAccountName { get; set; }
        public int RateLimitPerMinute { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RevokeApiKeyRequestDto { public int ApiKeyId { get; set; } }

    // ── M132: Enterprise — Webhook Management ───────────────────────
    public class CreateWebhookRequestDto
    {
        public int? CorporateAccountId { get; set; }
        public string TargetUrl { get; set; } = "";
        public string EventTypes { get; set; } = ""; // comma-separated
    }

    public class WebhookCreatedDto
    {
        public int Id { get; set; }
        public string TargetUrl { get; set; } = "";
        public string EventTypes { get; set; } = "";
        public string SigningSecret { get; set; } = ""; // shown once, at creation only
    }

    public class WebhookSubscriptionDto
    {
        public int Id { get; set; }
        public string TargetUrl { get; set; } = "";
        public string EventTypes { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TriggerWebhookEventRequestDto
    {
        public string EventType { get; set; } = "";
        public object? Payload { get; set; }
    }

    public class WebhookDeliveryLogDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = "";
        public string Status { get; set; } = "";
        public int? ResponseStatusCode { get; set; }
        public int AttemptCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}
