using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M121: Analytics — Customer Lifetime Value ──────────────────
    public class CustomerLtvSnapshot
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpend { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TenureDays { get; set; }
        public decimal EstimatedAnnualValue { get; set; } // (TotalSpend / TenureDays) * 365, tenure-adjusted
        [MaxLength(20)] public string Segment { get; set; } = "Bronze"; // Platinum / Gold / Silver / Bronze
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M122: Analytics — Booking Funnel ───────────────────────────
    // Uses the Booking lifecycle itself (no separate event-tracking table
    // exists on the platform yet) as the funnel: Created -> Accepted ->
    // Started -> Completed, with Cancelled as the drop-off exit at any stage.
    public class BookingFunnelSnapshot
    {
        [Key] public int Id { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        public int CreatedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int StartedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public decimal AcceptanceRate { get; set; } // Accepted / Created
        public decimal StartRate { get; set; } // Started / Accepted
        public decimal CompletionRate { get; set; } // Completed / Started
        public decimal OverallConversionRate { get; set; } // Completed / Created
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M123: Analytics — Chef Performance ─────────────────────────
    public class ChefPerformanceAnalyticsSnapshot
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        public int BookingCount { get; set; }
        public decimal Earnings { get; set; }
        public decimal AverageRating { get; set; }
        public int CancelledCount { get; set; }
        public decimal CancellationRate { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M124: Analytics — Geographic Heatmap ───────────────────────
    // Coarse lat/lng grid-cell aggregation (rounded to ~1.1km cells at the
    // equator, 2 decimal places) so the map doesn't need to plot every raw
    // booking point — cheap to render, still shows real density patterns.
    public class GeoHeatmapCell
    {
        [Key] public int Id { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        public decimal GridLatitude { get; set; } // rounded to 2 decimals
        public decimal GridLongitude { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
