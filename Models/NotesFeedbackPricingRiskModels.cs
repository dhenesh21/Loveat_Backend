using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M109: Internal Notes ───────────────────────────────────────
    // Generic, cross-entity internal notes — chef profiles, customer
    // accounts, bookings, anything an admin wants to leave a note on.
    // Distinct from M31 SupportMessage.IsInternal, which is scoped to a
    // single ticket's conversation thread; this is a standalone note not
    // tied to any ticket.
    public class InternalNote
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string EntityType { get; set; } = "User"; // User / Chef / Booking / Complaint / Invoice / ...
        public int EntityId { get; set; }
        public string Note { get; set; } = "";
        public bool IsPinned { get; set; } = false;
        public int CreatedByAdminId { get; set; }
        public User? CreatedByAdmin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M110: Customer Feedback Center ─────────────────────────────
    // General sentiment collection (NPS/CSAT-style surveys), distinct from
    // per-ticket satisfaction (M31 SupportTicket.Rating) and per-booking
    // reviews — this is periodic, platform-wide "how are we doing" pulse.
    public class FeedbackSurvey
    {
        [Key] public int Id { get; set; }
        [MaxLength(150)] public string Title { get; set; } = "";
        [MaxLength(20)] public string SurveyType { get; set; } = "NPS"; // NPS / CSAT / General
        [MaxLength(20)] public string TargetAudience { get; set; } = "All"; // All / Customer / Chef
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FeedbackResponse
    {
        [Key] public int Id { get; set; }
        public int FeedbackSurveyId { get; set; }
        public FeedbackSurvey? FeedbackSurvey { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int Score { get; set; } // NPS: 0-10, CSAT: 1-5
        public string? Comment { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M111: AI Suite — Smart Pricing Engine ──────────────────────
    // Heuristic (not ML-model-backed) pricing suggestions computed from the
    // M85 ServiceCity + M111... existing DemandPrediction table: compares
    // recent predicted demand against active chef supply in a city/category
    // and suggests a price multiplier. Suggestions are advisory — nothing
    // auto-applies a price change.
    public class PricingSuggestion
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string City { get; set; } = "";
        [MaxLength(60)] public string Category { get; set; } = "All";
        public decimal SuggestedMultiplier { get; set; } = 1.0m; // e.g. 1.15 = suggest +15%
        public decimal DemandScore { get; set; } // avg PredictedDemand used as basis, 0-100
        public int ActiveChefSupply { get; set; }
        [MaxLength(300)] public string Reason { get; set; } = "";
        [MaxLength(20)] public string Status { get; set; } = "Suggested"; // Suggested / Applied / Dismissed
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M112: AI Suite — Fraud Risk Scoring ────────────────────────
    // Writes into the existing M45 FraudDetectionLog table (DetectionType =
    // "AIRiskScore") rather than creating a parallel table — this is an
    // on-demand behavioral scoring engine that feeds the same fraud review
    // queue the Dispute/Fraud module already uses.
    public class RiskSignalDefinition
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string SignalKey { get; set; } = ""; // e.g. "high_refund_rate", "rapid_repeat_bookings"
        [MaxLength(150)] public string Description { get; set; } = "";
        public decimal WeightPoints { get; set; } = 10; // added to risk score when this signal fires
        public bool IsActive { get; set; } = true;
    }
}
