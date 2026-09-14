using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    public class CapacityForecast
    {
        [Key] public int Id { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = "";
        public int ProjectedBookingCount { get; set; }
        public int ProjectedPeakConcurrentUsers { get; set; }
        [MaxLength(300)] public string? ScalingRecommendation { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class DependencyHealthCheck
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string ServiceName { get; set; } = "";
        [MaxLength(20)] public string Status { get; set; } = "Unknown";
        public int? ResponseTimeMs { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class InfraCostEntry
    {
        [Key] public int Id { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = "";
        [MaxLength(60)] public string ServiceCategory { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal? BudgetLimit { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ApiVersionRecord
    {
        [Key] public int Id { get; set; }
        [MaxLength(20)] public string Version { get; set; } = "";
        [MaxLength(20)] public string Status { get; set; } = "Active";
        public DateTime? DeprecationAnnouncedAt { get; set; }
        public DateTime? SunsetDate { get; set; }
        public string? Notes { get; set; }
    }

    public class DrDrillLog
    {
        [Key] public int Id { get; set; }
        [MaxLength(120)] public string ScenarioName { get; set; } = "";
        [MaxLength(20)] public string Outcome { get; set; } = "Pass";
        public int? RecoveryTimeMinutes { get; set; }
        public string? Findings { get; set; }
        public int ConductedByAdminId { get; set; }
        public User? ConductedByAdmin { get; set; }
        public DateTime ConductedAt { get; set; } = DateTime.UtcNow;
    }
}
