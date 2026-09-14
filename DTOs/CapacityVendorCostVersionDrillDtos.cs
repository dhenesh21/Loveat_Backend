namespace LovEat.API.DTOs
{
    public class GenerateCapacityForecastRequestDto { public string PeriodKey { get; set; } = ""; }
    public class CapacityForecastDto
    {
        public string PeriodKey { get; set; } = "";
        public int ProjectedBookingCount { get; set; }
        public int ProjectedPeakConcurrentUsers { get; set; }
        public string? ScalingRecommendation { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class LogDependencyHealthRequestDto { public string ServiceName { get; set; } = ""; public string Status { get; set; } = "Operational"; public int? ResponseTimeMs { get; set; } }
    public class DependencyHealthDto { public string ServiceName { get; set; } = ""; public string Status { get; set; } = ""; public int? ResponseTimeMs { get; set; } public DateTime CheckedAt { get; set; } }
    public class DependencyHealthOverviewDto { public List<DependencyHealthDto> Services { get; set; } = new(); public bool AllOperational { get; set; } }

    public class LogInfraCostRequestDto { public string PeriodKey { get; set; } = ""; public string ServiceCategory { get; set; } = ""; public decimal Amount { get; set; } public decimal? BudgetLimit { get; set; } }
    public class InfraCostEntryDto { public string ServiceCategory { get; set; } = ""; public decimal Amount { get; set; } public decimal? BudgetLimit { get; set; } public bool OverBudget { get; set; } }
    public class InfraCostSummaryDto { public string PeriodKey { get; set; } = ""; public decimal TotalCost { get; set; } public List<InfraCostEntryDto> ByCategory { get; set; } = new(); }

    public class CreateApiVersionRequestDto { public string Version { get; set; } = ""; }
    public class DeprecateApiVersionRequestDto { public int VersionId { get; set; } public DateTime SunsetDate { get; set; } public string? Notes { get; set; } }
    public class ApiVersionDto { public int Id { get; set; } public string Version { get; set; } = ""; public string Status { get; set; } = ""; public DateTime? DeprecationAnnouncedAt { get; set; } public DateTime? SunsetDate { get; set; } public string? Notes { get; set; } }

    public class LogDrDrillRequestDto { public string ScenarioName { get; set; } = ""; public string Outcome { get; set; } = "Pass"; public int? RecoveryTimeMinutes { get; set; } public string? Findings { get; set; } public int ConductedByAdminId { get; set; } }
    public class DrDrillLogDto { public string ScenarioName { get; set; } = ""; public string Outcome { get; set; } = ""; public int? RecoveryTimeMinutes { get; set; } public string? ConductedByName { get; set; } public DateTime ConductedAt { get; set; } }

    public class OpsDashboardSummaryDto
    {
        public int ActiveIncidents { get; set; }
        public bool BackupsHealthy { get; set; }
        public bool AllDependenciesOperational { get; set; }
        public decimal CompliancePercent { get; set; }
        public int PendingChangeRequests { get; set; }
        public string OverallHealthStatus { get; set; } = "Healthy";
    }
}
