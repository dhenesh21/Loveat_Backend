using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>M23/M55: logs every AI-driven recommendation/prediction call (chef recommendations, demand prediction, dietary matching scores) so results can be audited and the model's accuracy tracked over time. AIRecommendationService and AIAnalyticsService (Phase 4/6) both write here.</summary>
    public class AIAnalytics
    {
        [Key] public int Id { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(50)]
        public string AnalyticsType { get; set; } = ""; // ChefRecommendation / DemandPrediction / DietaryMatch / ChurnRisk

        /// <summary>JSON blob of whatever was fed to the model — kept generic since each analytics type has a different shape.</summary>
        public string? InputDataJson { get; set; }

        /// <summary>JSON blob of the model's output/scores.</summary>
        public string? OutputDataJson { get; set; }

        [MaxLength(30)]
        public string? ModelVersion { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
