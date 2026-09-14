namespace LovEat.API.DTOs
{
    // ── M93: GST Management ────────────────────────────────────────
    public class UpsertChefGstProfileRequestDto
    {
        public int ChefId { get; set; }
        public string? Gstin { get; set; }
        public string? LegalBusinessName { get; set; }
        public string? PlaceOfSupplyState { get; set; }
        public bool IsGstRegistered { get; set; }
    }

    public class ChefGstProfileDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public string? Gstin { get; set; }
        public string? LegalBusinessName { get; set; }
        public string? PlaceOfSupplyState { get; set; }
        public bool IsGstRegistered { get; set; }
        public bool IsVerified { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public class VerifyChefGstRequestDto { public int ChefGstProfileId { get; set; } public bool Approve { get; set; } = true; }

    public class UpsertHsnSacCodeRequestDto
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal RatePercent { get; set; } = 18;
        public string AppliesTo { get; set; } = "Booking";
    }

    public class GstHsnSacCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal RatePercent { get; set; }
        public string AppliesTo { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class GenerateGstReturnRequestDto
    {
        public string PeriodKey { get; set; } = ""; // yyyy-MM
        public string ReturnType { get; set; } = "GSTR1";
    }

    public class MarkGstReturnFiledRequestDto
    {
        public int ReturnPeriodId { get; set; }
        public string FilingReference { get; set; } = "";
    }

    public class GstReturnPeriodDto
    {
        public int Id { get; set; }
        public string PeriodKey { get; set; } = "";
        public string ReturnType { get; set; } = "";
        public int TotalInvoiceCount { get; set; }
        public decimal TotalTaxableValue { get; set; }
        public decimal TotalGstCollected { get; set; }
        public string Status { get; set; } = "";
        public string? FilingReference { get; set; }
        public string? FiledByName { get; set; }
        public DateTime? FiledAt { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    // ── M94: TDS Management ────────────────────────────────────────
    public class UpsertTdsRateConfigRequestDto
    {
        public string Section { get; set; } = "194O";
        public string Description { get; set; } = "";
        public decimal RatePercent { get; set; } = 1;
        public decimal AnnualThresholdAmount { get; set; } = 500000;
    }

    public class TdsRateConfigDto
    {
        public int Id { get; set; }
        public string Section { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal RatePercent { get; set; }
        public decimal AnnualThresholdAmount { get; set; }
        public bool IsActive { get; set; }
    }

    public class RecordTdsDeductionRequestDto
    {
        public int ChefId { get; set; }
        public int? SettlementRequestId { get; set; }
        public decimal GrossAmount { get; set; }
        public string Section { get; set; } = "194O";
    }

    public class TdsDeductionDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public string Section { get; set; } = "";
        public decimal GrossAmount { get; set; }
        public decimal TdsRatePercent { get; set; }
        public decimal TdsAmount { get; set; }
        public decimal NetPayableAmount { get; set; }
        public string FinancialYear { get; set; } = "";
        public string Quarter { get; set; } = "";
        public DateTime DeductedAt { get; set; }
    }

    public class ChefTdsSummaryDto
    {
        public int ChefId { get; set; }
        public string FinancialYear { get; set; } = "";
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalTdsDeducted { get; set; }
        public int DeductionCount { get; set; }
        public bool ThresholdCrossed { get; set; }
    }

    public class GenerateTdsReturnRequestDto
    {
        public string FinancialYear { get; set; } = "";
        public string Quarter { get; set; } = "";
    }

    public class MarkTdsReturnFiledRequestDto
    {
        public int ReturnId { get; set; }
        public string FilingReference { get; set; } = "";
    }

    public class TdsReturnDto
    {
        public int Id { get; set; }
        public string FinancialYear { get; set; } = "";
        public string Quarter { get; set; } = "";
        public int TotalDeducteeCount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalTdsDeducted { get; set; }
        public string Status { get; set; } = "";
        public string? FilingReference { get; set; }
        public string? FiledByName { get; set; }
        public DateTime? FiledAt { get; set; }
    }

    public class IssueTdsCertificateRequestDto
    {
        public int ChefId { get; set; }
        public string FinancialYear { get; set; } = "";
        public string Quarter { get; set; } = "";
    }

    public class TdsCertificateDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public string FinancialYear { get; set; } = "";
        public string Quarter { get; set; } = "";
        public string CertificateNumber { get; set; } = "";
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalTdsAmount { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
