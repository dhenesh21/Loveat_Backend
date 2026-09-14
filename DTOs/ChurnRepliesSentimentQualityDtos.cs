namespace LovEat.API.DTOs
{
    // Note: M113 (Churn Prediction) DTOs already exist — see ChurnVoiceDtos.cs
    // (ChurnRiskDto, etc.), part of the earlier M71 module. Nothing new needed here.

    // ── M114: AI Suite — Suggested Replies ─────────────────────────
    public class CreateReplyTemplateRequestDto
    {
        public string Category { get; set; } = "General";
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string? Keywords { get; set; }
    }

    public class ReplyTemplateDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string? Keywords { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class SuggestRepliesRequestDto
    {
        public int TicketId { get; set; }
        public int Take { get; set; } = 3;
    }

    public class UseTemplateRequestDto { public int TemplateId { get; set; } }

    // ── M115: AI Suite — Sentiment Analysis ────────────────────────
    public class AnalyzeSentimentRequestDto
    {
        public string SourceType { get; set; } = "Review"; // Review / Feedback
        public int SourceId { get; set; }
        public string Text { get; set; } = "";
    }

    public class SentimentScoreDto
    {
        public string SourceType { get; set; } = "";
        public int SourceId { get; set; }
        public decimal Score { get; set; }
        public string Label { get; set; } = "";
        public string? MatchedKeywords { get; set; }
        public DateTime AnalyzedAt { get; set; }
    }

    public class SentimentSummaryDto
    {
        public int TotalAnalyzed { get; set; }
        public int Positive { get; set; }
        public int Neutral { get; set; }
        public int Negative { get; set; }
        public double AverageScore { get; set; }
    }

    // ── M116: AI Suite — Chef Quality Scoring ──────────────────────
    public class ComputeChefQualityRequestDto { public int ChefId { get; set; } }

    public class ChefQualityScoreDto
    {
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public decimal RatingComponent { get; set; }
        public decimal CompletionComponent { get; set; }
        public decimal CancellationPenalty { get; set; }
        public decimal ResponsivenessComponent { get; set; }
        public decimal OverallScore { get; set; }
        public string Tier { get; set; } = "";
        public DateTime ComputedAt { get; set; }
    }

    public class ChefQualityLeaderboardEntryDto
    {
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public decimal OverallScore { get; set; }
        public string Tier { get; set; } = "";
    }
}
