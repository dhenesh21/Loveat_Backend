namespace LovEat.API.DTOs
{
    // ── M133: Enterprise — SLA Contracts ────────────────────────────
    public class CreateSlaContractRequestDto
    {
        public int CorporateAccountId { get; set; }
        public decimal UptimeGuaranteePercent { get; set; } = 99.5m;
        public int SupportResponseTimeMinutes { get; set; } = 60;
        public decimal PenaltyPercentPerBreachHour { get; set; } = 1;
        public decimal MonthlyContractValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SlaContractDto
    {
        public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public string? CompanyName { get; set; }
        public string ContractNumber { get; set; } = "";
        public decimal UptimeGuaranteePercent { get; set; }
        public int SupportResponseTimeMinutes { get; set; }
        public decimal PenaltyPercentPerBreachHour { get; set; }
        public decimal MonthlyContractValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
        public decimal TotalCreditsIssued { get; set; }
    }

    public class RecordSlaBreachRequestDto
    {
        public int SlaContractId { get; set; }
        public string Description { get; set; } = "";
        public decimal BreachHours { get; set; }
    }

    public class SlaBreachDto
    {
        public string Description { get; set; } = "";
        public decimal BreachHours { get; set; }
        public decimal CreditAmount { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    // ── M134: Enterprise — Multi-Location Management ───────────────
    public class CreateCorporateLocationRequestDto
    {
        public int CorporateAccountId { get; set; }
        public string LocationName { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int DefaultHeadcount { get; set; } = 0;
    }

    public class CorporateLocationDto
    {
        public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public string LocationName { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public int DefaultHeadcount { get; set; }
        public bool IsActive { get; set; }
    }

    // ── M135: Enterprise — Custom Billing ───────────────────────────
    public class GenerateCorporateInvoiceRequestDto
    {
        public int CorporateAccountId { get; set; }
        public string PeriodKey { get; set; } = ""; // yyyy-MM
    }

    public class CorporateInvoiceDto
    {
        public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public string? CompanyName { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public string PeriodKey { get; set; } = "";
        public decimal Subtotal { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
        public string Status { get; set; } = "";
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class MarkCorporateInvoicePaidRequestDto { public int InvoiceId { get; set; } }

    // ── M136: Enterprise — Enterprise Reporting ─────────────────────
    public class GenerateEnterpriseUsageRequestDto
    {
        public int CorporateAccountId { get; set; }
        public string PeriodKey { get; set; } = ""; // yyyy-MM
    }

    public class EnterpriseUsageDto
    {
        public int CorporateAccountId { get; set; }
        public string? CompanyName { get; set; }
        public string PeriodKey { get; set; } = "";
        public int TotalOrders { get; set; }
        public int TotalMeals { get; set; }
        public decimal TotalSpend { get; set; }
        public int ActiveMembers { get; set; }
        public int ActiveLocations { get; set; }
    }
}
