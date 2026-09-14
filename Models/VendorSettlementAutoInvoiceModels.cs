using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M95: Vendor Settlement ─────────────────────────────────────
    // A Finance-level batching layer on top of the existing M42
    // SettlementRequest flow (chef withdrawal requests) and M94 TDS. Finance
    // groups a period's approved payouts into a SettlementBatch, gets
    // sign-off, then processes the whole batch at once (marking the
    // underlying SettlementRequests Completed via the existing
    // SettlementService — this table does not replace SettlementRequest).
    public class SettlementBatch
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string BatchNumber { get; set; } = "";
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Draft"; // Draft / Approved / Processed / Failed
        public int VendorCount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalTdsAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public int CreatedByAdminId { get; set; }
        public User? CreatedByAdmin { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public User? ApprovedByAdmin { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        [MaxLength(60)] public string? BankFileReference { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<SettlementBatchItem> Items { get; set; } = new List<SettlementBatchItem>();
    }

    public class SettlementBatchItem
    {
        [Key] public int Id { get; set; }
        public int SettlementBatchId { get; set; }
        public SettlementBatch? SettlementBatch { get; set; }
        public int SettlementRequestId { get; set; }
        public SettlementRequest? SettlementRequest { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TdsAmount { get; set; }
        public decimal NetAmount { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Included"; // Included / Excluded / Failed
        public string? FailureReason { get; set; }
    }

    // Blocks a vendor's future payout batches (fraud review, disputed
    // booking, KYC pending) without touching their SettlementRequest history.
    public class VendorSettlementHold
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(200)] public string Reason { get; set; } = "";
        public int HeldByAdminId { get; set; }
        public User? HeldByAdmin { get; set; }
        public DateTime HeldAt { get; set; } = DateTime.UtcNow;
        public int? ReleasedByAdminId { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ── M96: Automatic Invoice ─────────────────────────────────────
    // Automation layer over the existing M43 InvoiceService.GenerateInvoiceAsync
    // (which stays manual/on-demand). This adds: a config toggle for which
    // booking status triggers auto-generation, an idempotent per-booking
    // trigger safe to call from a completion webhook or a backfill sweep,
    // a run log for observability, and delivery tracking once an invoice
    // exists.
    public class AutoInvoiceConfig
    {
        [Key] public int Id { get; set; }
        public bool IsEnabled { get; set; } = true;
        [MaxLength(20)] public string TriggerBookingStatus { get; set; } = "Completed";
        public bool AutoSendToCustomer { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AutoInvoiceRunLog
    {
        [Key] public int Id { get; set; }
        public int BookingId { get; set; }
        [MaxLength(20)] public string Result { get; set; } = "Generated"; // Generated / AlreadyExists / Skipped / Failed
        public int? InvoiceId { get; set; }
        public string? Detail { get; set; }
        public DateTime RunAt { get; set; } = DateTime.UtcNow;
    }

    public class InvoiceDeliveryLog
    {
        [Key] public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        [MaxLength(20)] public string Channel { get; set; } = "Email"; // Email / SMS / InApp
        [MaxLength(20)] public string Status { get; set; } = "Sent"; // Sent / Failed
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
