namespace LovEat.API.DTOs
{
    // ── M95: Vendor Settlement ──────────────────────────────────────
    public class CreateSettlementBatchRequestDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class SettlementBatchItemDto
    {
        public int Id { get; set; }
        public int SettlementRequestId { get; set; }
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TdsAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = "";
        public string? FailureReason { get; set; }
    }

    public class SettlementBatchDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = "";
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Status { get; set; } = "";
        public int VendorCount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalTdsAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public string? CreatedByName { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? BankFileReference { get; set; }
        public List<SettlementBatchItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class ApproveSettlementBatchRequestDto { public int BatchId { get; set; } }
    public class ProcessSettlementBatchRequestDto { public int BatchId { get; set; } }

    public class PlaceVendorHoldRequestDto
    {
        public int ChefId { get; set; }
        public string Reason { get; set; } = "";
    }

    public class VendorHoldDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public string Reason { get; set; } = "";
        public string? HeldByName { get; set; }
        public DateTime HeldAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ReleasedAt { get; set; }
    }

    // ── M96: Automatic Invoice ──────────────────────────────────────
    public class UpdateAutoInvoiceConfigRequestDto
    {
        public bool IsEnabled { get; set; } = true;
        public string TriggerBookingStatus { get; set; } = "Completed";
        public bool AutoSendToCustomer { get; set; } = true;
    }

    public class AutoInvoiceConfigDto
    {
        public bool IsEnabled { get; set; }
        public string TriggerBookingStatus { get; set; } = "";
        public bool AutoSendToCustomer { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TriggerAutoInvoiceRequestDto { public int BookingId { get; set; } }

    public class AutoInvoiceRunResultDto
    {
        public int BookingId { get; set; }
        public string Result { get; set; } = "";
        public int? InvoiceId { get; set; }
        public string? Detail { get; set; }
    }

    public class BackfillAutoInvoiceResultDto
    {
        public int ScannedCount { get; set; }
        public int GeneratedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public List<AutoInvoiceRunResultDto> Results { get; set; } = new();
    }

    public class AutoInvoiceRunLogDto
    {
        public int BookingId { get; set; }
        public string Result { get; set; } = "";
        public int? InvoiceId { get; set; }
        public string? Detail { get; set; }
        public DateTime RunAt { get; set; }
    }
}
