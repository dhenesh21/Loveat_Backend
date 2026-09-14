using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M83: Super Admin Dashboard
    //
    // The dashboard itself is mostly a read/aggregation layer over data
    // that already exists (Users, Bookings, Payments — see
    // AnalyticsService from Phase 6 for the platform-KPI precedent this
    // extends). This table adds only what doesn't exist anywhere yet:
    // point-in-time platform health snapshots, so "Platform Health" on the
    // dashboard has real history to show instead of only a live number.
    // ══════════════════════════════════════════════════════════════
    public class PlatformHealthMetric
    {
        [Key] public int Id { get; set; }

        [MaxLength(50)]
        public string MetricName { get; set; } = ""; // ActiveBookings / ErrorRate / AvgResponseTimeMs / etc.

        public decimal MetricValue { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M84: Franchise Management
    // ══════════════════════════════════════════════════════════════
    public class Franchise
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FranchiseeName { get; set; } = "";

        [MaxLength(100)]
        public string ContactEmail { get; set; } = "";

        [MaxLength(15)]
        public string? ContactPhone { get; set; }

        [MaxLength(100)]
        public string RegionOrCity { get; set; } = "";

        /// <summary>Percentage of net platform revenue in this region the franchisee earns.</summary>
        public decimal RevenueSharePercent { get; set; } = 20;

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active / Suspended / Terminated

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FranchiseSettings
    {
        [Key] public int Id { get; set; }

        public int FranchiseId { get; set; }
        public Franchise? Franchise { get; set; }

        [Required, MaxLength(100)]
        public string SettingKey { get; set; } = "";

        [MaxLength(1000)]
        public string SettingValue { get; set; } = "";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
