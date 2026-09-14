using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // Stores daily/weekly/monthly aggregated earnings snapshots per chef
    public class ChefEarningsSnapshot
    {
        [Key] public int Id { get; set; }

        public int    ChefId     { get; set; }
        public User?  Chef       { get; set; }

        public string Period     { get; set; } = "Daily";  // Daily / Weekly / Monthly
        public string PeriodKey  { get; set; } = "";       // e.g. "2024-12-22" / "2024-W51" / "2024-12"

        public decimal TotalEarnings      { get; set; }
        public decimal PlatformCommission { get; set; }    // 15%
        public decimal NetEarnings        { get; set; }    // 85%
        public int     BookingsCount      { get; set; }
        public int     HoursWorked        { get; set; }
        public decimal AvgEarningPerHour  { get; set; }
        public decimal AvgEarningPerBooking{ get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // ── Chef Earnings DTOs ─────────────────────────────────────────
    public class ChefEarningsOverviewDto
    {
        public int     ChefId              { get; set; }
        public string  ChefName            { get; set; } = "";
        public decimal TotalLifetimeEarnings{ get; set; }
        public decimal EarningsThisMonth   { get; set; }
        public decimal EarningsLastMonth   { get; set; }
        public decimal MoMGrowthPercent    { get; set; }
        public decimal EarningsThisWeek    { get; set; }
        public decimal AvgPerBooking       { get; set; }
        public decimal AvgPerHour          { get; set; }
        public int     TotalBookings       { get; set; }
        public int     TotalHoursWorked    { get; set; }
        public decimal PlatformCommissionPaid { get; set; }
        public decimal NetEarnings         { get; set; }
        public string  PerformanceGrade    { get; set; } = "A";
        public List<EarningsChartPointDto> DailyChart   { get; set; } = new();
        public List<EarningsChartPointDto> MonthlyChart { get; set; } = new();
        public List<EarningsByTypeDto>     ByBookingType{ get; set; } = new();
        public List<EarningsByDayDto>      ByDayOfWeek  { get; set; } = new();
        public List<TopBookingDto>         TopBookings  { get; set; } = new();
    }

    public class EarningsChartPointDto
    {
        public string  Label    { get; set; } = "";
        public decimal Amount   { get; set; }
        public int     Bookings { get; set; }
    }

    public class EarningsByTypeDto
    {
        public string  BookingType { get; set; } = "";
        public decimal Amount      { get; set; }
        public int     Count       { get; set; }
        public double  Percent     { get; set; }
    }

    public class EarningsByDayDto
    {
        public string  Day    { get; set; } = "";
        public decimal Amount { get; set; }
        public int     Count  { get; set; }
    }

    public class TopBookingDto
    {
        public int     BookingId    { get; set; }
        public string  CustomerName { get; set; } = "";
        public string  BookingType  { get; set; } = "";
        public decimal Amount       { get; set; }
        public string  Date         { get; set; } = "";
    }

    // Admin view of all chefs' earnings
    public class AdminChefEarningsSummaryDto
    {
        public decimal TotalPlatformRevenue   { get; set; }
        public decimal TotalChefPayouts       { get; set; }
        public decimal RevenueThisMonth       { get; set; }
        public decimal MoMGrowthPercent       { get; set; }
        public List<ChefEarningsRowDto> TopEarners    { get; set; } = new();
        public List<ChefEarningsRowDto> AllChefs      { get; set; } = new();
        public List<EarningsChartPointDto> MonthlyChart { get; set; } = new();
    }

    public class ChefEarningsRowDto
    {
        public int     ChefId         { get; set; }
        public string  ChefName       { get; set; } = "";
        public string  Phone          { get; set; } = "";
        public string  City           { get; set; } = "";
        public decimal TotalEarnings  { get; set; }
        public decimal Commission     { get; set; }
        public int     TotalBookings  { get; set; }
        public double  AvgRating      { get; set; }
        public string  Grade          { get; set; } = "";
    }
}
