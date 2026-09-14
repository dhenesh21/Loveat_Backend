using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // Note: M113 (AI Suite — Churn Prediction) is not duplicated here — an
    // earlier module (M71, see ChurnVoiceModels.cs/ChurnVoiceService.cs)
    // already provides a fuller-featured churn scoring engine (factors
    // breakdown, intervention tracking). M113's requirement is satisfied by
    // that existing ChurnPredictionService; nothing new needed in this file.

    // ── M114: AI Suite — Suggested Replies ─────────────────────────
    // Keyword/category-matched canned-response suggestions for support
    // agents working M31 SupportTicket — not generative AI, a curated
    // template bank matched by ticket category/keywords, ranked by
    // historical usage so the most-used template for a category rises up.
    public class ReplyTemplate
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string Category { get; set; } = "General"; // matches SupportTicket.Category, or "All"
        [MaxLength(100)] public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        [MaxLength(300)] public string? Keywords { get; set; } // comma-separated, matched against ticket subject/messages
        public int UsageCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M115: AI Suite — Sentiment Analysis ────────────────────────
    // Lexicon-based sentiment scoring (positive/negative word matching) over
    // Review comments and Feedback responses — not a trained NLP model,
    // documented honestly the same way as the rest of the AI Suite.
    public class SentimentScore
    {
        [Key] public int Id { get; set; }
        [MaxLength(20)] public string SourceType { get; set; } = "Review"; // Review / Feedback
        public int SourceId { get; set; }
        public decimal Score { get; set; } // -1.0 (very negative) to +1.0 (very positive)
        [MaxLength(20)] public string Label { get; set; } = "Neutral"; // Positive / Neutral / Negative
        [MaxLength(300)] public string? MatchedKeywords { get; set; }
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M116: AI Suite — Chef Quality Scoring ──────────────────────
    // Composite score from ratings, completion behavior, and cancellation
    // rate — a transparent weighted formula, computed on demand and
    // snapshotted for trend tracking (not a black-box model).
    public class ChefQualityScore
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public decimal RatingComponent { get; set; } // 0-100, from AverageRating
        public decimal CompletionComponent { get; set; } // 0-100, completed vs total bookings
        public decimal CancellationPenalty { get; set; } // 0-100 points deducted
        public decimal ResponsivenessComponent { get; set; } // 0-100, from avg first-response time proxy
        public decimal OverallScore { get; set; } // 0-100
        [MaxLength(20)] public string Tier { get; set; } = "Standard"; // Elite / Good / Standard / AtRisk
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }
}
