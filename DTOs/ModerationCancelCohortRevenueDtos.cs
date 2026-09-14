namespace LovEat.API.DTOs
{
    // ── M117: AI Suite — Content Moderation ────────────────────────
    public class ScanContentRequestDto
    {
        public string SourceType { get; set; } = "ChefBio";
        public int SourceId { get; set; }
        public string Text { get; set; } = "";
    }

    public class ModerationFlagDto
    {
        public int Id { get; set; }
        public string SourceType { get; set; } = "";
        public int SourceId { get; set; }
        public string FlagType { get; set; } = "";
        public string MatchedTerms { get; set; } = "";
        public string Status { get; set; } = "";
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime FlaggedAt { get; set; }
    }

    public class DecideModerationFlagRequestDto
    {
        public int FlagId { get; set; }
        public string Status { get; set; } = "Cleared"; // Reviewed / Cleared / ActionTaken
    }

    // ── M118: AI Suite — Cancellation Risk Prediction ──────────────
    public class ComputeCancellationRiskRequestDto { public int BookingId { get; set; } }

    public class CancellationRiskDto
    {
        public int BookingId { get; set; }
        public decimal CustomerCancellationRate { get; set; }
        public decimal ChefCancellationRate { get; set; }
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = "";
        public DateTime ComputedAt { get; set; }
    }

    // ── M119: Analytics — Cohort Retention ─────────────────────────
    public class GenerateCohortRequestDto
    {
        public string CohortMonth { get; set; } = ""; // yyyy-MM
        public int MaxMonthOffset { get; set; } = 6;
    }

    public class CohortRetentionRowDto
    {
        public string CohortMonth { get; set; } = "";
        public int MonthOffset { get; set; }
        public int CohortSize { get; set; }
        public int ActiveCount { get; set; }
        public decimal RetentionPercent { get; set; }
    }

    public class CohortGridDto
    {
        public List<string> Cohorts { get; set; } = new();
        public List<CohortRetentionRowDto> Rows { get; set; } = new();
    }

    // ── M120: Analytics — Revenue Analytics ────────────────────────
    public class GenerateRevenueSnapshotRequestDto { public string DateKey { get; set; } = ""; } // yyyy-MM-dd

    public class RevenueSnapshotDto
    {
        public string DateKey { get; set; } = "";
        public string? City { get; set; }
        public string? Category { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class RevenueTrendRequestDto
    {
        public string FromDateKey { get; set; } = "";
        public string ToDateKey { get; set; } = "";
    }

    public class RevenueTrendDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public List<RevenueSnapshotDto> DailyTotals { get; set; } = new();
        public List<RevenueSnapshotDto> TopCities { get; set; } = new();
        public List<RevenueSnapshotDto> TopCategories { get; set; } = new();
    }
}
