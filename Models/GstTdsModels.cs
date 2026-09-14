using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M93: GST Management ────────────────────────────────────────
    // Builds on top of the M74 TaxRule (generic per-country rate lookup) and
    // the M22 Invoice.GSTPercent (hardcoded 18% India billing, unchanged).
    // This adds what's actually needed to *run* Indian GST compliance:
    // a GSTIN registry for chefs who are GST-registered vendors, an HSN/SAC
    // code catalog, and period-wise return summaries (GSTR-1 / GSTR-3B) an
    // admin can generate from the existing Invoice table and mark filed.
    public class ChefGstProfile
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(15)] public string? Gstin { get; set; } // 15-char Indian GSTIN
        [MaxLength(150)] public string? LegalBusinessName { get; set; }
        [MaxLength(60)] public string? PlaceOfSupplyState { get; set; }
        public bool IsGstRegistered { get; set; } = false; // false = unregistered small vendor, no GSTIN required
        public bool IsVerified { get; set; } = false;
        public int? VerifiedByAdminId { get; set; }
        public User? VerifiedByAdmin { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class GstHsnSacCode
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(10)] public string Code { get; set; } = ""; // e.g. "996331" (SAC for restaurant/catering services)
        [MaxLength(150)] public string Description { get; set; } = "";
        public decimal RatePercent { get; set; } = 18;
        [MaxLength(30)] public string AppliesTo { get; set; } = "Booking"; // Booking / Subscription / DeliveryFee
        public bool IsActive { get; set; } = true;
    }

    public class GstReturnPeriod
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        [MaxLength(10)] public string ReturnType { get; set; } = "GSTR1"; // GSTR1 / GSTR3B
        public int TotalInvoiceCount { get; set; }
        public decimal TotalTaxableValue { get; set; }
        public decimal TotalGstCollected { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Open"; // Open / Filed
        public string? FilingReference { get; set; }
        public int? FiledByAdminId { get; set; }
        public User? FiledByAdmin { get; set; }
        public DateTime? FiledAt { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M94: TDS Management ────────────────────────────────────────
    // Platform-as-e-commerce-operator TDS on chef payouts (Income Tax Act
    // Section 194-O: 1% on gross settlement value for most cases). Hooks
    // into the existing M42 SettlementRequest flow additively — recording a
    // deduction does not change SettlementRequest itself, so nothing already
    // relying on that table breaks.
    public class TdsRateConfig
    {
        [Key] public int Id { get; set; }
        [MaxLength(10)] public string Section { get; set; } = "194O"; // 194O / 194J / 194C
        [MaxLength(100)] public string Description { get; set; } = "";
        public decimal RatePercent { get; set; } = 1; // 194-O default: 1%
        public decimal AnnualThresholdAmount { get; set; } = 500000; // no TDS below this cumulative FY payout (individual/HUF exemption)
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TdsDeduction
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public int? SettlementRequestId { get; set; }
        public SettlementRequest? SettlementRequest { get; set; }
        [MaxLength(10)] public string Section { get; set; } = "194O";
        public decimal GrossAmount { get; set; }
        public decimal TdsRatePercent { get; set; }
        public decimal TdsAmount { get; set; }
        public decimal NetPayableAmount { get; set; }
        [MaxLength(9)] public string FinancialYear { get; set; } = ""; // e.g. "2026-27"
        [MaxLength(2)] public string Quarter { get; set; } = ""; // Q1-Q4
        public DateTime DeductedAt { get; set; } = DateTime.UtcNow;
    }

    public class TdsReturn
    {
        [Key] public int Id { get; set; }
        [MaxLength(9)] public string FinancialYear { get; set; } = "";
        [MaxLength(2)] public string Quarter { get; set; } = "";
        public int TotalDeducteeCount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalTdsDeducted { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Open"; // Open / Filed
        public string? FilingReference { get; set; } // Form 26Q acknowledgement number
        public int? FiledByAdminId { get; set; }
        public User? FiledByAdmin { get; set; }
        public DateTime? FiledAt { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class TdsCertificate
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(9)] public string FinancialYear { get; set; } = "";
        [MaxLength(2)] public string Quarter { get; set; } = "";
        [MaxLength(30)] public string CertificateNumber { get; set; } = ""; // Form 16A reference
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalTdsAmount { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
