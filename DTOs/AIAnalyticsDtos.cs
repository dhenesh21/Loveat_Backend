namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M23: AI chef recommendations for a customer
    // ══════════════════════════════════════════════════════════════
    public class ChefRecommendationDto
    {
        public int ChefProfileId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
        public List<string> Cuisines { get; set; } = new();
        public decimal HourlyRate { get; set; }
        public decimal AverageRating { get; set; }
        public string Reason { get; set; } = ""; // e.g. "Matches your dietary preference" / "Highly rated near you"
    }

    // ══════════════════════════════════════════════════════════════
    // M27: platform-wide analytics dashboard (distinct from batch 28's
    // per-chef BusinessPerformanceService — this is the top-level summary)
    // ══════════════════════════════════════════════════════════════
    public class PlatformAnalyticsSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalChefs { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
        public int NewUsersLast30Days { get; set; }
    }

    public class MonthlyBookingTrendPointDto
    {
        public string Month { get; set; } = "";
        public int Bookings { get; set; }
    }
}
