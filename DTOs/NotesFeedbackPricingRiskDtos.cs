namespace LovEat.API.DTOs
{
    // ── M109: Internal Notes ───────────────────────────────────────
    public class CreateInternalNoteRequestDto
    {
        public string EntityType { get; set; } = "User";
        public int EntityId { get; set; }
        public string Note { get; set; } = "";
        public bool IsPinned { get; set; } = false;
    }

    public class UpdateInternalNoteRequestDto
    {
        public int NoteId { get; set; }
        public string? Note { get; set; }
        public bool? IsPinned { get; set; }
    }

    public class InternalNoteDto
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = "";
        public int EntityId { get; set; }
        public string Note { get; set; } = "";
        public bool IsPinned { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // ── M110: Customer Feedback Center ─────────────────────────────
    public class CreateFeedbackSurveyRequestDto
    {
        public string Title { get; set; } = "";
        public string SurveyType { get; set; } = "NPS";
        public string TargetAudience { get; set; } = "All";
    }

    public class FeedbackSurveyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string SurveyType { get; set; } = "";
        public string TargetAudience { get; set; } = "";
        public bool IsActive { get; set; }
        public int ResponseCount { get; set; }
        public double AverageScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SubmitFeedbackRequestDto
    {
        public int SurveyId { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
    }

    public class FeedbackSurveyStatsDto
    {
        public int SurveyId { get; set; }
        public string SurveyType { get; set; } = "";
        public int ResponseCount { get; set; }
        public double AverageScore { get; set; }
        public double? NpsScore { get; set; } // only meaningful when SurveyType == "NPS"
        public int Promoters { get; set; }
        public int Passives { get; set; }
        public int Detractors { get; set; }
    }

    // ── M111: AI Suite — Smart Pricing Engine ──────────────────────
    public class GeneratePricingSuggestionsRequestDto
    {
        public string? City { get; set; } // null = all cities with demand data
    }

    public class PricingSuggestionDto
    {
        public int Id { get; set; }
        public string City { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal SuggestedMultiplier { get; set; }
        public decimal DemandScore { get; set; }
        public int ActiveChefSupply { get; set; }
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime GeneratedAt { get; set; }
    }

    public class DecidePricingSuggestionRequestDto
    {
        public int SuggestionId { get; set; }
        public bool Apply { get; set; } = true;
    }

    // ── M112: AI Suite — Fraud Risk Scoring ─────────────────────────
    public class UpsertRiskSignalRequestDto
    {
        public string SignalKey { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal WeightPoints { get; set; } = 10;
    }

    public class RiskSignalDto
    {
        public int Id { get; set; }
        public string SignalKey { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal WeightPoints { get; set; }
        public bool IsActive { get; set; }
    }

    public class ComputeRiskScoreRequestDto { public int UserId { get; set; } }

    public class RiskScoreResultDto
    {
        public int UserId { get; set; }
        public decimal RiskScore { get; set; } // 0-100
        public string Severity { get; set; } = ""; // Low / Medium / High / Critical
        public List<string> TriggeredSignals { get; set; } = new();
    }
}
