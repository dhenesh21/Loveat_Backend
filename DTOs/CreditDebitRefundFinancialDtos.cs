namespace LovEat.API.DTOs
{
    // ── M97: Credit Notes ──────────────────────────────────────────
    public class IssueCreditNoteRequestDto
    {
        public int InvoiceId { get; set; }
        public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
        public int? LinkedRefundRequestId { get; set; }
    }

    public class CreditNoteDto
    {
        public int Id { get; set; }
        public string CreditNoteNumber { get; set; } = "";
        public int InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public string? IssuedByName { get; set; }
        public DateTime IssuedAt { get; set; }
    }

    public class CancelCreditNoteRequestDto { public int CreditNoteId { get; set; } }

    // ── M98: Debit Notes ───────────────────────────────────────────
    public class IssueDebitNoteRequestDto
    {
        public int? InvoiceId { get; set; }
        public string PartyType { get; set; } = "Customer";
        public int PartyId { get; set; }
        public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class DebitNoteDto
    {
        public int Id { get; set; }
        public string DebitNoteNumber { get; set; } = "";
        public int? InvoiceId { get; set; }
        public string PartyType { get; set; } = "";
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public string? IssuedByName { get; set; }
        public DateTime IssuedAt { get; set; }
    }

    public class CancelDebitNoteRequestDto { public int DebitNoteId { get; set; } }

    // ── M99: Refund Dashboard ──────────────────────────────────────
    public class CreateRefundRequestDto
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = "";
        public string RefundMethod { get; set; } = "Original";
    }

    public class DecideRefundRequestDto
    {
        public int RefundRequestId { get; set; }
        public bool Approve { get; set; } = true;
        public string? RejectionReason { get; set; }
    }

    public class ProcessRefundRequestDto
    {
        public int RefundRequestId { get; set; }
        public string TransactionRef { get; set; } = "";
        public bool IssueCreditNote { get; set; } = true;
    }

    public class RefundRequestDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "";
        public string RefundMethod { get; set; } = "";
        public string? TransactionRef { get; set; }
        public string? RejectionReason { get; set; }
        public int? LinkedCreditNoteId { get; set; }
        public string? ProcessedByName { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class RefundDashboardStatsDto
    {
        public int TotalRequested { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int TotalProcessed { get; set; }
        public decimal TotalAmountProcessed { get; set; }
        public decimal TotalAmountPending { get; set; }
        public double AvgProcessingHours { get; set; }
    }

    // ── M100: Financial Reports ────────────────────────────────────
    public class GenerateFinancialReportRequestDto { public string PeriodKey { get; set; } = ""; } // yyyy-MM

    public class FinancialReportDto
    {
        public string PeriodKey { get; set; } = "";
        public decimal GrossRevenue { get; set; }
        public decimal GstCollected { get; set; }
        public decimal PlatformCommissionEarned { get; set; }
        public decimal TdsDeducted { get; set; }
        public decimal TotalRefundsIssued { get; set; }
        public decimal TotalCreditNotesValue { get; set; }
        public decimal TotalDebitNotesValue { get; set; }
        public decimal NetPlatformRevenue { get; set; }
        public int InvoiceCount { get; set; }
        public int RefundCount { get; set; }
        public string? GeneratedByName { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
