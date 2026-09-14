using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M125: Analytics — Custom Report Builder ────────────────────
    // Ad-hoc report definitions an admin can save and re-run: pick a metric,
    // a grouping dimension, and a date range. Runs directly against Booking
    // (the source of truth already used by every other Analytics module in
    // this phase) rather than needing a separate data warehouse.
    public class ReportDefinition
    {
        [Key] public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(30)] public string Metric { get; set; } = "Revenue"; // Revenue / BookingCount / NewCustomers / CancellationCount
        [MaxLength(20)] public string GroupBy { get; set; } = "Day"; // Day / Week / Month / City / Category
        [MaxLength(20)] public string DateRangeType { get; set; } = "Last30Days"; // Last7Days / Last30Days / Last90Days / Custom
        public DateTime? CustomFrom { get; set; }
        public DateTime? CustomTo { get; set; }
        public int CreatedByAdminId { get; set; }
        public User? CreatedByAdmin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M126: Analytics — Scheduled Report Delivery ────────────────
    public class ScheduledReport
    {
        [Key] public int Id { get; set; }
        public int ReportDefinitionId { get; set; }
        public ReportDefinition? ReportDefinition { get; set; }
        [MaxLength(20)] public string Frequency { get; set; } = "Weekly"; // Daily / Weekly / Monthly
        [MaxLength(500)] public string RecipientEmails { get; set; } = ""; // comma-separated
        public bool IsActive { get; set; } = true;
        public DateTime? LastRunAt { get; set; }
        public DateTime NextRunDue { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ScheduledReportRun
    {
        [Key] public int Id { get; set; }
        public int ScheduledReportId { get; set; }
        public ScheduledReport? ScheduledReport { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Success"; // Success / Failed
        public string? ResultSummary { get; set; }
        public DateTime RunAt { get; set; } = DateTime.UtcNow;
    }

    // ── M127: Analytics — A/B Test Analytics ───────────────────────
    // General-purpose experiment framework — distinct from the M90
    // PromoCampaignVariant A/B system (which is specific to promo banners).
    // Deterministic hash-bucketed assignment (same pattern as the M91
    // FeatureFlag rollout bucketing) so a user's variant is stable.
    public class ExperimentDefinition
    {
        [Key] public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; } = "";
        public string? Description { get; set; }
        [MaxLength(30)] public string VariantAName { get; set; } = "Control";
        [MaxLength(30)] public string VariantBName { get; set; } = "Treatment";
        [MaxLength(20)] public string Status { get; set; } = "Draft"; // Draft / Running / Completed
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ExperimentEvent
    {
        [Key] public int Id { get; set; }
        public int ExperimentId { get; set; }
        public ExperimentDefinition? Experiment { get; set; }
        public int UserId { get; set; }
        [MaxLength(30)] public string Variant { get; set; } = "";
        [MaxLength(20)] public string EventType { get; set; } = "Conversion"; // Exposure / Conversion
        public decimal? Value { get; set; } // optional revenue/value attached to a conversion
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M128: Analytics — Data Export ──────────────────────────────
    // Synchronous export jobs (no background worker infra on this platform
    // yet) — suitable for moderate result sizes; ResultPreview stores a
    // capped JSON sample plus the true RecordCount so large exports are
    // still summarized honestly even though the full payload isn't stored.
    public class DataExportJob
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string ExportType { get; set; } = "Bookings"; // Bookings / Users / Invoices / Reviews
        [MaxLength(10)] public string Format { get; set; } = "JSON"; // JSON / CSV
        public DateTime DateRangeFrom { get; set; }
        public DateTime DateRangeTo { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Completed"; // Completed / Failed (processed synchronously)
        public int RecordCount { get; set; }
        public string? ResultPreview { get; set; } // capped JSON/CSV sample (first N rows)
        public int RequestedByAdminId { get; set; }
        public User? RequestedByAdmin { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
