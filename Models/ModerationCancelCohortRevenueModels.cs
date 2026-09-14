using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M117: AI Suite — Content Moderation ────────────────────────
    // Keyword/pattern-based flagging (profanity, spam, contact-info leakage
    // attempts) over free-text fields — chef bios, portfolio descriptions,
    // review comments. Same honest heuristic framing as the rest of the AI
    // Suite: a curated blocklist, not a trained classifier.
    public class ModerationFlag
    {
        [Key] public int Id { get; set; }
        [MaxLength(20)] public string SourceType { get; set; } = "ChefBio"; // ChefBio / Review / ChatMessage / Portfolio
        public int SourceId { get; set; }
        [MaxLength(20)] public string FlagType { get; set; } = "Profanity"; // Profanity / Spam / ContactInfoLeak
        [MaxLength(300)] public string MatchedTerms { get; set; } = "";
        [MaxLength(20)] public string Status { get; set; } = "Flagged"; // Flagged / Reviewed / Cleared / ActionTaken
        public int? ReviewedByAdminId { get; set; }
        public User? ReviewedByAdmin { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime FlaggedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M118: AI Suite — Cancellation Risk Prediction ──────────────
    // Per-booking heuristic score at creation/check time, based on the
    // customer's and chef's historical cancellation rates — lets Support
    // proactively confirm high-risk bookings before they fall through.
    public class CancellationRiskScore
    {
        [Key] public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public decimal CustomerCancellationRate { get; set; } // 0-100, historical
        public decimal ChefCancellationRate { get; set; } // 0-100, historical
        public decimal RiskScore { get; set; } // 0-100 composite
        [MaxLength(20)] public string RiskLevel { get; set; } = "Low"; // Low / Medium / High
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M119: Analytics — Cohort Retention ─────────────────────────
    // Monthly signup-cohort retention: for customers who signed up in a
    // given month, what % booked again in month 0 (signup month), month 1,
    // month 2, etc. Snapshotted per cohort+offset so the grid doesn't need
    // recomputing from raw bookings on every dashboard load.
    public class CohortRetentionSnapshot
    {
        [Key] public int Id { get; set; }
        [MaxLength(7)] public string CohortMonth { get; set; } = ""; // yyyy-MM signup month
        public int MonthOffset { get; set; } // 0, 1, 2, 3...
        public int CohortSize { get; set; } // customers who signed up that month
        public int ActiveCount { get; set; } // of those, how many booked in CohortMonth+MonthOffset
        public decimal RetentionPercent { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M120: Analytics — Revenue Analytics ────────────────────────
    // Granular revenue breakdown snapshots (by city, by category, daily
    // trend) — deeper than the M100 FinancialReportSnapshot's single
    // platform-wide monthly total, for the analytics dashboard's charts.
    public class RevenueAnalyticsSnapshot
    {
        [Key] public int Id { get; set; }
        [MaxLength(10)] public string DateKey { get; set; } = ""; // yyyy-MM-dd
        [MaxLength(60)] public string? City { get; set; } // null = all-cities daily total row
        [MaxLength(60)] public string? Category { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
