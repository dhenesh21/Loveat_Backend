using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // M57: Chef Performance Snapshot (cached metrics per chef per period)
    public class ChefPerformanceSnapshot
    {
        [Key] public int Id { get; set; }
        public int    ChefId              { get; set; }
        public User?  Chef                { get; set; }
        public string Period              { get; set; } = "Monthly"; // Weekly / Monthly
        public string PeriodKey           { get; set; } = "";       // 2024-12 / 2024-W51
        public int    TotalBookings       { get; set; }
        public int    CompletedBookings   { get; set; }
        public int    CancelledBookings   { get; set; }
        public decimal CompletionRate     { get; set; }
        public decimal AvgRating          { get; set; }
        public decimal TotalEarnings      { get; set; }
        public int    TotalHoursWorked    { get; set; }
        public decimal AvgResponseTimeMins{ get; set; }
        public int    RepeatCustomers     { get; set; }
        public string PerformanceGrade    { get; set; } = "B";
        public DateTime CreatedAt         { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // M56: Business Dashboard
    public class BusinessDashboardDto
    {
        // Revenue metrics
        public decimal TotalRevenue        { get; set; }
        public decimal RevenueThisMonth    { get; set; }
        public decimal RevenueLastMonth    { get; set; }
        public decimal RevenueMoMGrowth    { get; set; }
        public decimal RevenueThisYear     { get; set; }
        public decimal PlatformCommission  { get; set; }
        public decimal ChefPayouts         { get; set; }

        // Bookings
        public int     TotalBookings       { get; set; }
        public int     BookingsThisMonth   { get; set; }
        public decimal BookingMoMGrowth    { get; set; }
        public decimal AvgBookingValue     { get; set; }

        // Users
        public int     TotalUsers          { get; set; }
        public int     TotalChefs          { get; set; }
        public int     NewUsersThisMonth   { get; set; }
        public int     ActiveUsersThisMonth{ get; set; }
        public decimal UserRetentionRate   { get; set; }

        // Charts
        public List<MonthlyRevenueDto>    MonthlyRevenue    { get; set; } = new();
        public List<MonthlyRevenueDto>    MonthlyBookings   { get; set; } = new();
        public List<CityRevenueDto>       RevenueByCity     { get; set; } = new();
        public List<BookingTypeRevenueDto> RevenueByType    { get; set; } = new();
        public List<RevenueGrowthDto>     GrowthForecast    { get; set; } = new();

        // Health indicators
        public decimal GrossMargin         { get; set; }
        public decimal CustomerLTV         { get; set; }  // lifetime value
        public decimal CAC                 { get; set; }  // customer acquisition cost
        public decimal NPS                 { get; set; }  // net promoter score

        // Supply health — previously hardcoded/fabricated in the AdminWeb
        // page (72% of totalChefs, "4.6 ★", "87.4%"); now real aggregates.
        public int     ActiveChefsThisMonth{ get; set; }
        public decimal AvgChefRating       { get; set; }
        public decimal ChefCompletionRate  { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string  Month   { get; set; } = "";
        public decimal Amount  { get; set; }
        public int     Count   { get; set; }
        public decimal Growth  { get; set; }
    }

    public class CityRevenueDto
    {
        public string  City    { get; set; } = "";
        public decimal Revenue { get; set; }
        public int     Bookings{ get; set; }
        public double  Share   { get; set; }
    }

    public class BookingTypeRevenueDto
    {
        public string  Type    { get; set; } = "";
        public decimal Revenue { get; set; }
        public int     Count   { get; set; }
        public double  Share   { get; set; }
    }

    public class RevenueGrowthDto
    {
        public string  Month     { get; set; } = "";
        public decimal Actual    { get; set; }
        public decimal Forecast  { get; set; }
        public bool    IsForecast{ get; set; }
    }

    // M57: Chef Performance
    public class ChefPerformanceDto
    {
        public int     ChefId              { get; set; }
        public string  ChefName            { get; set; } = "";
        public string  City                { get; set; } = "";
        public string  Phone               { get; set; } = "";

        // Core metrics
        public int     TotalBookings       { get; set; }
        public int     CompletedBookings   { get; set; }
        public int     CancelledBookings   { get; set; }
        public decimal CompletionRate      { get; set; }
        public decimal AvgRating           { get; set; }
        public decimal TotalEarnings       { get; set; }
        public int     TotalHoursWorked    { get; set; }
        public decimal AvgResponseTimeMins { get; set; }
        public int     RepeatCustomers     { get; set; }
        public int     UniqueCustomers     { get; set; }
        public decimal RepeatRate          { get; set; }
        public string  PerformanceGrade    { get; set; } = "B";
        public string  Trend               { get; set; } = "Stable"; // Improving / Declining / Stable

        // Charts
        public List<MonthlyRevenueDto> MonthlyEarnings   { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyBookings    { get; set; } = new();

        // Rankings
        public int     EarningsRank        { get; set; }
        public int     RatingRank          { get; set; }
        public int     CompletionRank      { get; set; }
        public int     TotalChefsInCity    { get; set; }
    }

    public class ChefLeaderboardDto
    {
        public List<ChefPerformanceDto> TopByEarnings   { get; set; } = new();
        public List<ChefPerformanceDto> TopByRating     { get; set; } = new();
        public List<ChefPerformanceDto> TopByCompletion { get; set; } = new();
        public List<ChefPerformanceDto> AtRisk          { get; set; } = new();
    }
}
