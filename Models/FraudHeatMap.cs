using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>M21: aggregated geographic risk view — how many fraud/safety flags cluster in a zone over a period. Feeds AdminWeb's heat map visualization; distinct from the per-event FraudDetectionLog (batch 23) and SafetyIncident (batch 24), which this table summarizes.</summary>
    public class FraudHeatMap
    {
        [Key] public int Id { get; set; }

        [MaxLength(100)]
        public string City { get; set; } = "";

        [MaxLength(100)]
        public string? Zone { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public int RiskScore { get; set; } = 0; // 0-100
        public int IncidentCount { get; set; } = 0;
        public int FraudFlagCount { get; set; } = 0;

        [MaxLength(20)]
        public string Period { get; set; } = "Weekly"; // Daily / Weekly / Monthly
        [MaxLength(20)]
        public string PeriodKey { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
