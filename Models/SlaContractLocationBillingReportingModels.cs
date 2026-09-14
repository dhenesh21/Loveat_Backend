using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M133: Enterprise — SLA Contracts ───────────────────────────
    // Contract-level commitments to a CorporateAccount (uptime guarantee,
    // response-time commitment, penalty terms) — distinct from the M107
    // SlaPolicy/SlaTracker, which is per-support-ticket response/resolution
    // timing. This is the legal/commercial layer sitting above that.
    public class SlaContract
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        [MaxLength(30)] public string ContractNumber { get; set; } = "";
        public decimal UptimeGuaranteePercent { get; set; } = 99.5m;
        public int SupportResponseTimeMinutes { get; set; } = 60;
        public decimal PenaltyPercentPerBreachHour { get; set; } = 1; // % of monthly contract value credited back per hour of breach
        public decimal MonthlyContractValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Active"; // Draft / Active / Expired / Terminated
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SlaContractBreach
    {
        [Key] public int Id { get; set; }
        public int SlaContractId { get; set; }
        public SlaContract? SlaContract { get; set; }
        [MaxLength(200)] public string Description { get; set; } = "";
        public decimal BreachHours { get; set; }
        public decimal CreditAmount { get; set; } // computed penalty credit issued
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M134: Enterprise — Multi-Location Management ──────────────
    // A CorporateAccount with several office/branch locations, each with
    // its own delivery address and default headcount — bulk orders can be
    // placed against a specific location rather than one flat address.
    public class CorporateLocation
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        [MaxLength(80)] public string LocationName { get; set; } = ""; // e.g. "HQ - Chennai", "Branch - OMR"
        [MaxLength(300)] public string Address { get; set; } = "";
        [MaxLength(60)] public string City { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int DefaultHeadcount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M135: Enterprise — Custom Billing ──────────────────────────
    // Consolidated monthly invoicing for a CorporateAccount — one invoice
    // rolling up all BulkOrderRequests (and any individual member bookings)
    // for the period, rather than the platform's normal per-booking M43
    // Invoice flow.
    public class CorporateInvoice
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        [MaxLength(30)] public string InvoiceNumber { get; set; } = "";
        [MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        public decimal Subtotal { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Issued"; // Issued / Paid / Overdue
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M136: Enterprise — Enterprise Reporting ────────────────────
    // Per-account usage/spend snapshot for the corporate customer's own
    // dashboard — how much they've spent, how many meals, which locations,
    // which employees are ordering most.
    public class EnterpriseUsageSnapshot
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        public int TotalOrders { get; set; }
        public int TotalMeals { get; set; }
        public decimal TotalSpend { get; set; }
        public int ActiveMembers { get; set; }
        public int ActiveLocations { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
