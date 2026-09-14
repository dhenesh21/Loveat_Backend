namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M83: Super Admin Dashboard
    // ══════════════════════════════════════════════════════════════
    public class SuperAdminDashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public int ActiveUsers { get; set; }
        public int ActiveChefs { get; set; }
        public int LiveBookings { get; set; } // currently Accepted or InProgress
        public int PlatformHealthScore { get; set; } // 0-100, derived from recent metrics
        public List<PlatformHealthMetricDto> RecentMetrics { get; set; } = new();
    }

    public class PlatformHealthMetricDto
    {
        public string MetricName { get; set; } = "";
        public decimal MetricValue { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    public class RecordHealthMetricRequestDto
    {
        public string MetricName { get; set; } = "";
        public decimal MetricValue { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M84: Franchise Management
    // ══════════════════════════════════════════════════════════════
    public class CreateFranchiseRequestDto
    {
        public string FranchiseeName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string? ContactPhone { get; set; }
        public string RegionOrCity { get; set; } = "";
        public decimal RevenueSharePercent { get; set; } = 20;
    }

    public class FranchiseDto
    {
        public int Id { get; set; }
        public string FranchiseeName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string? ContactPhone { get; set; }
        public string RegionOrCity { get; set; } = "";
        public decimal RevenueSharePercent { get; set; }
        public string Status { get; set; } = "";
        public decimal EstimatedRevenueShareAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateFranchiseStatusRequestDto
    {
        public string Status { get; set; } = "";
    }

    public class UpsertFranchiseSettingRequestDto
    {
        public string SettingKey { get; set; } = "";
        public string SettingValue { get; set; } = "";
    }

    public class FranchiseSettingDto
    {
        public string SettingKey { get; set; } = "";
        public string SettingValue { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }
}
