namespace LovEat.API.DTOs
{
    // ── M125: Analytics — Custom Report Builder ────────────────────
    public class CreateReportDefinitionRequestDto
    {
        public string Name { get; set; } = "";
        public string Metric { get; set; } = "Revenue";
        public string GroupBy { get; set; } = "Day";
        public string DateRangeType { get; set; } = "Last30Days";
        public DateTime? CustomFrom { get; set; }
        public DateTime? CustomTo { get; set; }
    }

    public class ReportDefinitionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Metric { get; set; } = "";
        public string GroupBy { get; set; } = "";
        public string DateRangeType { get; set; } = "";
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReportResultRowDto
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
    }

    public class ReportResultDto
    {
        public int ReportDefinitionId { get; set; }
        public string Name { get; set; } = "";
        public string Metric { get; set; } = "";
        public List<ReportResultRowDto> Rows { get; set; } = new();
        public decimal Total { get; set; }
    }

    // ── M126: Analytics — Scheduled Report Delivery ────────────────
    public class CreateScheduledReportRequestDto
    {
        public int ReportDefinitionId { get; set; }
        public string Frequency { get; set; } = "Weekly";
        public string RecipientEmails { get; set; } = "";
    }

    public class ScheduledReportDto
    {
        public int Id { get; set; }
        public int ReportDefinitionId { get; set; }
        public string? ReportName { get; set; }
        public string Frequency { get; set; } = "";
        public string RecipientEmails { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime NextRunDue { get; set; }
    }

    public class RunScheduledReportRequestDto { public int ScheduledReportId { get; set; } }

    public class ScheduledReportRunDto
    {
        public string Status { get; set; } = "";
        public string? ResultSummary { get; set; }
        public DateTime RunAt { get; set; }
    }

    // ── M127: Analytics — A/B Test Analytics ───────────────────────
    public class CreateExperimentRequestDto
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string VariantAName { get; set; } = "Control";
        public string VariantBName { get; set; } = "Treatment";
    }

    public class ExperimentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string VariantAName { get; set; } = "";
        public string VariantBName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    public class UpdateExperimentStatusRequestDto
    {
        public int ExperimentId { get; set; }
        public string Status { get; set; } = ""; // Running / Completed
    }

    public class GetVariantRequestDto { public int ExperimentId { get; set; } public int UserId { get; set; } }

    public class RecordExperimentEventRequestDto
    {
        public int ExperimentId { get; set; }
        public int UserId { get; set; }
        public string EventType { get; set; } = "Conversion";
        public decimal? Value { get; set; }
    }

    public class ExperimentVariantResultDto
    {
        public string Variant { get; set; } = "";
        public int Exposures { get; set; }
        public int Conversions { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ExperimentResultsDto
    {
        public int ExperimentId { get; set; }
        public string Name { get; set; } = "";
        public List<ExperimentVariantResultDto> Variants { get; set; } = new();
        public decimal? LiftPercent { get; set; } // (VariantB rate - VariantA rate) / VariantA rate * 100
    }

    // ── M128: Analytics — Data Export ──────────────────────────────
    public class CreateExportJobRequestDto
    {
        public string ExportType { get; set; } = "Bookings";
        public string Format { get; set; } = "JSON";
        public DateTime DateRangeFrom { get; set; }
        public DateTime DateRangeTo { get; set; }
    }

    public class DataExportJobDto
    {
        public int Id { get; set; }
        public string ExportType { get; set; } = "";
        public string Format { get; set; } = "";
        public DateTime DateRangeFrom { get; set; }
        public DateTime DateRangeTo { get; set; }
        public string Status { get; set; } = "";
        public int RecordCount { get; set; }
        public string? ResultPreview { get; set; }
        public string? RequestedByName { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
