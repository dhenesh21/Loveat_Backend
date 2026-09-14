using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M97: Credit Notes ──────────────────────────────────────────
    // Formal GST-compliant document reducing what a customer owes/was
    // charged against an existing Invoice (refund, overcharge correction,
    // service-not-rendered adjustment). References Invoice (M43) and
    // optionally a RefundRequest (M99) it was issued to settle.
    public class CreditNote
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string CreditNoteNumber { get; set; } = "";
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        [MaxLength(200)] public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Issued"; // Issued / Cancelled
        public int? LinkedRefundRequestId { get; set; }
        public int IssuedByAdminId { get; set; }
        public User? IssuedByAdmin { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M98: Debit Notes ───────────────────────────────────────────
    // Formal document for an additional amount owed — either by a customer
    // (undercharged booking) or charged against a chef/vendor (penalty,
    // correction). PartyType distinguishes who it applies to.
    public class DebitNote
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string DebitNoteNumber { get; set; } = "";
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        [MaxLength(10)] public string PartyType { get; set; } = "Customer"; // Customer / Chef
        public int PartyId { get; set; }
        public User? Party { get; set; }
        [MaxLength(200)] public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Issued"; // Issued / Cancelled
        public int IssuedByAdminId { get; set; }
        public User? IssuedByAdmin { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M99: Refund Dashboard ──────────────────────────────────────
    // A real workflow around the Refund concept that today only exists as
    // scattered status/amount fields on Booking/DisputeFraud/Payment. This
    // is the single place a refund gets requested, approved, and tracked to
    // completion, optionally issuing a CreditNote to formalize it.
    public class RefundRequest
    {
        [Key] public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public decimal Amount { get; set; }
        [MaxLength(200)] public string Reason { get; set; } = "";
        [MaxLength(20)] public string Status { get; set; } = "Requested"; // Requested / Approved / Rejected / Processed
        [MaxLength(20)] public string RefundMethod { get; set; } = "Original"; // Original / Wallet / BankTransfer
        public string? TransactionRef { get; set; }
        public string? RejectionReason { get; set; }
        public int? LinkedCreditNoteId { get; set; }
        public int? ProcessedByAdminId { get; set; }
        public User? ProcessedByAdmin { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }

    // ── M100: Financial Reports ────────────────────────────────────
    // Saved snapshots of the platform's financial position for a period, so
    // reports don't have to be recomputed from raw tables every time and
    // history stays available even as raw transactional data ages out.
    public class FinancialReportSnapshot
    {
        [Key] public int Id { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        public decimal GrossRevenue { get; set; }
        public decimal GstCollected { get; set; }
        public decimal PlatformCommissionEarned { get; set; }
        public decimal TdsDeducted { get; set; }
        public decimal TotalRefundsIssued { get; set; }
        public decimal TotalCreditNotesValue { get; set; }
        public decimal TotalDebitNotesValue { get; set; }
        public decimal NetPlatformRevenue { get; set; } // commission - refunds + debit notes - credit notes
        public int InvoiceCount { get; set; }
        public int RefundCount { get; set; }
        public int GeneratedByAdminId { get; set; }
        public User? GeneratedByAdmin { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
