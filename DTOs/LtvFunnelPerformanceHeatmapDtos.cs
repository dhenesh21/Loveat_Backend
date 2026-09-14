namespace LovEat.API.DTOs
{
    // ── M121: Analytics — Customer Lifetime Value ──────────────────
    public class ComputeLtvRequestDto { public int CustomerId { get; set; } }

    public class CustomerLtvDto
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpend { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TenureDays { get; set; }
        public decimal EstimatedAnnualValue { get; set; }
        public string Segment { get; set; } = "";
        public DateTime ComputedAt { get; set; }
    }

    public class LtvLeaderboardEntryDto
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal TotalSpend { get; set; }
        public string Segment { get; set; } = "";
    }

    // ── M122: Analytics — Booking Funnel ───────────────────────────
    public class GenerateFunnelRequestDto { public string PeriodKey { get; set; } = ""; } // yyyy-MM

    public class BookingFunnelDto
    {
        public string PeriodKey { get; set; } = "";
        public int CreatedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int StartedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public decimal AcceptanceRate { get; set; }
        public decimal StartRate { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal OverallConversionRate { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    // ── M123: Analytics — Chef Performance ─────────────────────────
    public class GenerateChefPerformanceRequestDto { public string PeriodKey { get; set; } = ""; } // yyyy-MM applies to all chefs

    public class ChefPerformanceAnalyticsDto
    {
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public string PeriodKey { get; set; } = "";
        public int BookingCount { get; set; }
        public decimal Earnings { get; set; }
        public decimal AverageRating { get; set; }
        public int CancelledCount { get; set; }
        public decimal CancellationRate { get; set; }
    }

    public class ChefPerformanceTrendDto
    {
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public List<ChefPerformanceAnalyticsDto> Periods { get; set; } = new();
    }

    // ── M124: Analytics — Geographic Heatmap ───────────────────────
    public class GenerateHeatmapRequestDto { public string PeriodKey { get; set; } = ""; } // yyyy-MM

    public class HeatmapCellDto
    {
        public decimal GridLatitude { get; set; }
        public decimal GridLongitude { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class HeatmapDto
    {
        public string PeriodKey { get; set; } = "";
        public List<HeatmapCellDto> Cells { get; set; } = new();
    }
}
