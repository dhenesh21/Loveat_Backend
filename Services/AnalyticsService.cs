using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;

namespace LovEat.API.Services
{
    /// <summary>M27: top-level platform KPIs for AdminWeb's main analytics dashboard. Distinct from batch 28's BusinessPerformanceService (per-chef performance) — this is the single-number, whole-platform summary.</summary>
    public class AnalyticsService
    {
        private readonly AppDbContext _db;
        public AnalyticsService(AppDbContext db) => _db = db;

        public async Task<PlatformAnalyticsSummaryDto> GetSummaryAsync()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalChefs = await _db.Users.CountAsync(u => u.Role == "Chef");
            var totalCustomers = await _db.Users.CountAsync(u => u.Role == "Customer");

            var totalBookings = await _db.Bookings.CountAsync();
            var completedBookings = await _db.Bookings.CountAsync(b => b.Status == "Completed");
            var cancelledBookings = await _db.Bookings.CountAsync(b => b.Status == "Cancelled");

            var totalRevenue = await _db.Payments.Where(p => p.Status == "Success").SumAsync(p => (decimal?)p.Amount) ?? 0;

            var avgBookingValue = completedBookings == 0
                ? 0
                : await _db.Bookings.Where(b => b.Status == "Completed").AverageAsync(b => (decimal?)b.TotalAmount) ?? 0;

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var newUsers = await _db.Users.CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

            return new PlatformAnalyticsSummaryDto
            {
                TotalUsers = totalUsers,
                TotalChefs = totalChefs,
                TotalCustomers = totalCustomers,
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                TotalRevenue = totalRevenue,
                AverageBookingValue = Math.Round(avgBookingValue, 2),
                NewUsersLast30Days = newUsers,
            };
        }

        /// <summary>Booking count per calendar month, for the dashboard's trend chart. `months` is how far back to look (e.g. 6 for "6M").</summary>
        public async Task<List<MonthlyBookingTrendPointDto>> GetMonthlyTrendAsync(int months = 6)
        {
            var since = DateTime.UtcNow.AddMonths(-months + 1);
            since = new DateTime(since.Year, since.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var bookings = await _db.Bookings
                .Where(b => b.CreatedAt >= since)
                .Select(b => b.CreatedAt)
                .ToListAsync();

            var buckets = new List<MonthlyBookingTrendPointDto>();
            for (int i = 0; i < months; i++)
            {
                var monthStart = since.AddMonths(i);
                var count = bookings.Count(d => d.Year == monthStart.Year && d.Month == monthStart.Month);
                buckets.Add(new MonthlyBookingTrendPointDto { Month = monthStart.ToString("MMM"), Bookings = count });
            }

            return buckets;
        }
    }
}
