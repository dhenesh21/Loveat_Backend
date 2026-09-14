using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;

namespace LovEat.API.Services
{
    public class ChefEarningsService
    {
        private readonly AppDbContext _db;
        private const decimal COMMISSION_RATE = 0.15m;

        public ChefEarningsService(AppDbContext db) => _db = db;

        // ── Chef's own earnings overview ───────────────────────────
        public async Task<ChefEarningsOverviewDto> GetOverviewAsync(int chefId)
        {
            var chef = await _db.Users.FindAsync(chefId);
            var bookings = await _db.Bookings
                .Where(b => b.ChefId == chefId && b.Status == "Completed")
                .OrderByDescending(b => b.ScheduledAt)
                .ToListAsync();

            var now        = DateTime.UtcNow;
            var thisMonth  = bookings.Where(b => b.ScheduledAt.Month == now.Month && b.ScheduledAt.Year == now.Year).ToList();
            var lastMonth  = bookings.Where(b => b.ScheduledAt.Month == now.AddMonths(-1).Month && b.ScheduledAt.Year == now.AddMonths(-1).Year).ToList();
            var thisWeek   = bookings.Where(b => b.ScheduledAt >= now.AddDays(-7)).ToList();

            decimal totalEarnings   = bookings.Sum(b => b.TotalAmount);
            decimal thisMonthAmt    = thisMonth.Sum(b => b.TotalAmount);
            decimal lastMonthAmt    = lastMonth.Sum(b => b.TotalAmount);
            decimal commission      = totalEarnings * COMMISSION_RATE;
            int     totalHours      = bookings.Sum(b => (b.DurationMinutes / 60));

            double momGrowth = lastMonthAmt > 0
                ? Math.Round((double)((thisMonthAmt - lastMonthAmt) / lastMonthAmt * 100), 1) : 0;

            // Daily chart — last 7 days
            var dailyChart = Enumerable.Range(0, 7).Select(i =>
            {
                var day  = now.AddDays(-6 + i);
                var dayB = bookings.Where(b => b.ScheduledAt.Date == day.Date).ToList();
                return new EarningsChartPointDto
                {
                    Label    = day.ToString("ddd"),
                    Amount   = dayB.Sum(b => b.TotalAmount),
                    Bookings = dayB.Count,
                };
            }).ToList();

            // Monthly chart — last 6 months
            var monthlyChart = Enumerable.Range(0, 6).Select(i =>
            {
                var m  = now.AddMonths(-5 + i);
                var mB = bookings.Where(b => b.ScheduledAt.Month == m.Month && b.ScheduledAt.Year == m.Year).ToList();
                return new EarningsChartPointDto
                {
                    Label    = m.ToString("MMM"),
                    Amount   = mB.Sum(b => b.TotalAmount),
                    Bookings = mB.Count,
                };
            }).ToList();

            // By booking type
            var byType = bookings.GroupBy(b => b.BookingType).Select(g => new EarningsByTypeDto
            {
                BookingType = g.Key,
                Amount      = g.Sum(b => b.TotalAmount),
                Count       = g.Count(),
                Percent     = bookings.Count > 0 ? Math.Round(g.Count() * 100.0 / bookings.Count, 1) : 0,
            }).OrderByDescending(t => t.Amount).ToList();

            // By day of week
            var dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            var byDay = dayNames.Select((d, i) =>
            {
                var dayB = bookings.Where(b => (int)b.ScheduledAt.DayOfWeek == i).ToList();
                return new EarningsByDayDto
                {
                    Day    = d,
                    Amount = dayB.Sum(b => b.TotalAmount),
                    Count  = dayB.Count,
                };
            }).ToList();

            // Top bookings
            var topBookings = bookings.OrderByDescending(b => b.TotalAmount).Take(5).Select(b => new TopBookingDto
            {
                BookingId    = b.Id,
                CustomerName = "Customer",
                BookingType  = b.BookingType,
                Amount       = b.TotalAmount,
                Date         = b.ScheduledAt.ToString("MMM dd, yyyy"),
            }).ToList();

            return new ChefEarningsOverviewDto
            {
                ChefId                = chefId,
                ChefName              = chef?.FullName ?? "",
                TotalLifetimeEarnings = totalEarnings,
                EarningsThisMonth     = thisMonthAmt,
                EarningsLastMonth     = lastMonthAmt,
                MoMGrowthPercent      = (decimal)momGrowth,
                EarningsThisWeek      = thisWeek.Sum(b => b.TotalAmount),
                AvgPerBooking         = bookings.Count > 0 ? Math.Round(totalEarnings / bookings.Count, 0) : 0,
                AvgPerHour            = totalHours > 0 ? Math.Round(totalEarnings / totalHours, 0) : 0,
                TotalBookings         = bookings.Count,
                TotalHoursWorked      = totalHours,
                PlatformCommissionPaid= commission,
                NetEarnings           = totalEarnings - commission,
                PerformanceGrade      = bookings.Count >= 50 ? "A" : bookings.Count >= 20 ? "B" : bookings.Count >= 10 ? "C" : "D",
                DailyChart            = dailyChart,
                MonthlyChart          = monthlyChart,
                ByBookingType         = byType,
                ByDayOfWeek           = byDay,
                TopBookings           = topBookings,
            };
        }

        // ── Admin: all chefs earnings summary ─────────────────────
        public async Task<AdminChefEarningsSummaryDto> GetAdminSummaryAsync()
        {
            var bookings = await _db.Bookings
                .Include(b => b.Chef)
                .Where(b => b.Status == "Completed")
                .ToListAsync();
            var chefCityLookup = await _db.ChefProfiles.ToDictionaryAsync(p => p.UserId, p => p.City);

            var now           = DateTime.UtcNow;
            decimal totalRev  = bookings.Sum(b => b.TotalAmount);
            decimal thisMonth = bookings.Where(b => b.ScheduledAt.Month == now.Month && b.ScheduledAt.Year == now.Year).Sum(b => b.TotalAmount);
            decimal lastMonth = bookings.Where(b => b.ScheduledAt.Month == now.AddMonths(-1).Month).Sum(b => b.TotalAmount);
            double  momGrowth = lastMonth > 0 ? Math.Round((double)((thisMonth - lastMonth) / lastMonth * 100), 1) : 0;

            // Per chef stats
            var chefGroups = bookings.GroupBy(b => b.ChefId).Select(g =>
            {
                var earnings = g.Sum(b => b.TotalAmount);
                return new ChefEarningsRowDto
                {
                    ChefId        = g.Key,
                    ChefName      = g.First().Chef?.FullName ?? "Chef",
                    Phone         = g.First().Chef?.PhoneNumber ?? "",
                    City          = chefCityLookup.GetValueOrDefault(g.Key) ?? "",
                    TotalEarnings = earnings,
                    Commission    = earnings * COMMISSION_RATE,
                    TotalBookings = g.Count(),
                    AvgRating     = 4.5,
                    Grade         = g.Count() >= 50 ? "A" : g.Count() >= 20 ? "B" : "C",
                };
            }).OrderByDescending(c => c.TotalEarnings).ToList();

            // Monthly chart last 6 months
            var monthlyChart = Enumerable.Range(0, 6).Select(i =>
            {
                var m  = now.AddMonths(-5 + i);
                var mB = bookings.Where(b => b.ScheduledAt.Month == m.Month && b.ScheduledAt.Year == m.Year).ToList();
                return new EarningsChartPointDto
                {
                    Label    = m.ToString("MMM"),
                    Amount   = mB.Sum(b => b.TotalAmount),
                    Bookings = mB.Count,
                };
            }).ToList();

            return new AdminChefEarningsSummaryDto
            {
                TotalPlatformRevenue = totalRev * COMMISSION_RATE,
                TotalChefPayouts     = totalRev * (1 - COMMISSION_RATE),
                RevenueThisMonth     = thisMonth,
                MoMGrowthPercent     = (decimal)momGrowth,
                TopEarners           = chefGroups.Take(10).ToList(),
                AllChefs             = chefGroups,
                MonthlyChart         = monthlyChart,
            };
        }
    }
}
