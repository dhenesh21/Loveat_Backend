using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M121: Analytics — Customer Lifetime Value ──────────────────
    public class CustomerLtvService
    {
        private readonly AppDbContext _db;
        public CustomerLtvService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CustomerLtvDto? Ltv)> ComputeAsync(int customerId)
        {
            var customer = await _db.Users.FindAsync(customerId);
            if (customer == null) return (false, "Customer not found.", null);

            var bookings = await _db.Bookings.Where(b => b.CustomerId == customerId && b.Status == "Completed").ToListAsync();
            var totalSpend = bookings.Sum(b => b.TotalAmount);
            var tenureDays = Math.Max(1, (int)(DateTime.UtcNow - customer.CreatedAt).TotalDays);
            var avgOrder = bookings.Count == 0 ? 0 : Math.Round(totalSpend / bookings.Count, 2);
            var estAnnual = Math.Round(totalSpend / tenureDays * 365, 2);

            var segment = estAnnual >= 50000 ? "Platinum" : estAnnual >= 20000 ? "Gold" : estAnnual >= 5000 ? "Silver" : "Bronze";

            var snapshot = new CustomerLtvSnapshot
            {
                CustomerId = customerId, TotalBookings = bookings.Count, TotalSpend = totalSpend, AverageOrderValue = avgOrder,
                TenureDays = tenureDays, EstimatedAnnualValue = estAnnual, Segment = segment,
            };
            _db.CustomerLtvSnapshots.Add(snapshot);
            await _db.SaveChangesAsync();

            return (true, "Computed.", new CustomerLtvDto
            {
                CustomerId = customerId, CustomerName = customer.FullName, TotalBookings = bookings.Count, TotalSpend = totalSpend,
                AverageOrderValue = avgOrder, TenureDays = tenureDays, EstimatedAnnualValue = estAnnual, Segment = segment, ComputedAt = snapshot.ComputedAt,
            });
        }

        public async Task<List<LtvLeaderboardEntryDto>> GetLeaderboardAsync(int take = 20)
        {
            var latestPerCustomer = await _db.CustomerLtvSnapshots.Include(s => s.Customer)
                .GroupBy(s => s.CustomerId).Select(g => g.OrderByDescending(s => s.ComputedAt).First())
                .OrderByDescending(s => s.TotalSpend).Take(take).ToListAsync();
            return latestPerCustomer.Select(s => new LtvLeaderboardEntryDto { CustomerId = s.CustomerId, CustomerName = s.Customer?.FullName, TotalSpend = s.TotalSpend, Segment = s.Segment }).ToList();
        }
    }

    // ── M122: Analytics — Booking Funnel ───────────────────────────
    public class BookingFunnelService
    {
        private readonly AppDbContext _db;
        public BookingFunnelService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, BookingFunnelDto? Funnel)> GenerateAsync(GenerateFunnelRequestDto req)
        {
            if (!DateTime.TryParseExact(req.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", null);
            var monthEnd = monthStart.AddMonths(1);

            var bookings = await _db.Bookings.Where(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd).ToListAsync();
            var created = bookings.Count;
            var accepted = bookings.Count(b => b.AcceptedAt != null);
            var started = bookings.Count(b => b.StartedAt != null);
            var completed = bookings.Count(b => b.Status == "Completed");
            var cancelled = bookings.Count(b => b.Status == "Cancelled");

            var snapshot = await _db.BookingFunnelSnapshots.FirstOrDefaultAsync(s => s.PeriodKey == req.PeriodKey) ?? new BookingFunnelSnapshot { PeriodKey = req.PeriodKey };
            var isNew = snapshot.Id == 0;
            snapshot.CreatedCount = created; snapshot.AcceptedCount = accepted; snapshot.StartedCount = started;
            snapshot.CompletedCount = completed; snapshot.CancelledCount = cancelled;
            snapshot.AcceptanceRate = created == 0 ? 0 : Math.Round((decimal)accepted / created * 100, 1);
            snapshot.StartRate = accepted == 0 ? 0 : Math.Round((decimal)started / accepted * 100, 1);
            snapshot.CompletionRate = started == 0 ? 0 : Math.Round((decimal)completed / started * 100, 1);
            snapshot.OverallConversionRate = created == 0 ? 0 : Math.Round((decimal)completed / created * 100, 1);
            snapshot.GeneratedAt = DateTime.UtcNow;
            if (isNew) _db.BookingFunnelSnapshots.Add(snapshot);
            await _db.SaveChangesAsync();

            return (true, "Generated.", ToDto(snapshot));
        }

        public async Task<List<BookingFunnelDto>> GetAllAsync() => (await _db.BookingFunnelSnapshots.OrderByDescending(s => s.PeriodKey).ToListAsync()).Select(ToDto).ToList();

        private static BookingFunnelDto ToDto(BookingFunnelSnapshot s) => new()
        {
            PeriodKey = s.PeriodKey, CreatedCount = s.CreatedCount, AcceptedCount = s.AcceptedCount, StartedCount = s.StartedCount,
            CompletedCount = s.CompletedCount, CancelledCount = s.CancelledCount, AcceptanceRate = s.AcceptanceRate, StartRate = s.StartRate,
            CompletionRate = s.CompletionRate, OverallConversionRate = s.OverallConversionRate, GeneratedAt = s.GeneratedAt,
        };
    }

    // ── M123: Analytics — Chef Performance ─────────────────────────
    public class ChefPerformanceAnalyticsService
    {
        private readonly AppDbContext _db;
        public ChefPerformanceAnalyticsService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, int ChefsProcessed)> GenerateAsync(GenerateChefPerformanceRequestDto req)
        {
            if (!DateTime.TryParseExact(req.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", 0);
            var monthEnd = monthStart.AddMonths(1);

            var bookings = await _db.Bookings.Where(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd).ToListAsync();
            var reviews = await _db.Reviews.Where(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd).ToListAsync();

            var chefIds = bookings.Select(b => b.ChefId).Distinct().ToList();
            foreach (var chefId in chefIds)
            {
                var chefBookings = bookings.Where(b => b.ChefId == chefId).ToList();
                var chefReviews = reviews.Where(r => r.ChefId == chefId).ToList();
                var completed = chefBookings.Count(b => b.Status == "Completed");
                var cancelled = chefBookings.Count(b => b.Status == "Cancelled");

                var snapshot = await _db.ChefPerformanceAnalyticsSnapshots.FirstOrDefaultAsync(s => s.ChefId == chefId && s.PeriodKey == req.PeriodKey) ?? new ChefPerformanceAnalyticsSnapshot { ChefId = chefId, PeriodKey = req.PeriodKey };
                var isNew = snapshot.Id == 0;
                snapshot.BookingCount = chefBookings.Count;
                snapshot.Earnings = chefBookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount);
                snapshot.AverageRating = chefReviews.Count == 0 ? 0 : Math.Round((decimal)chefReviews.Average(r => r.Rating), 2);
                snapshot.CancelledCount = cancelled;
                snapshot.CancellationRate = chefBookings.Count == 0 ? 0 : Math.Round((decimal)cancelled / chefBookings.Count * 100, 1);
                snapshot.GeneratedAt = DateTime.UtcNow;
                if (isNew) _db.ChefPerformanceAnalyticsSnapshots.Add(snapshot);
            }
            await _db.SaveChangesAsync();
            return (true, "Generated.", chefIds.Count);
        }

        public async Task<ChefPerformanceTrendDto?> GetTrendAsync(int chefId, int months = 6)
        {
            var chef = await _db.Users.FindAsync(chefId);
            if (chef == null) return null;
            var periods = await _db.ChefPerformanceAnalyticsSnapshots.Where(s => s.ChefId == chefId).OrderByDescending(s => s.PeriodKey).Take(months).ToListAsync();
            return new ChefPerformanceTrendDto
            {
                ChefId = chefId, ChefName = chef.FullName,
                Periods = periods.OrderBy(s => s.PeriodKey).Select(s => new ChefPerformanceAnalyticsDto
                {
                    ChefId = s.ChefId, ChefName = chef.FullName, PeriodKey = s.PeriodKey, BookingCount = s.BookingCount,
                    Earnings = s.Earnings, AverageRating = s.AverageRating, CancelledCount = s.CancelledCount, CancellationRate = s.CancellationRate,
                }).ToList(),
            };
        }

        public async Task<List<ChefPerformanceAnalyticsDto>> GetLeaderboardAsync(string periodKey, int take = 20)
        {
            var rows = await _db.ChefPerformanceAnalyticsSnapshots.Include(s => s.Chef).Where(s => s.PeriodKey == periodKey).OrderByDescending(s => s.Earnings).Take(take).ToListAsync();
            return rows.Select(s => new ChefPerformanceAnalyticsDto { ChefId = s.ChefId, ChefName = s.Chef?.FullName, PeriodKey = s.PeriodKey, BookingCount = s.BookingCount, Earnings = s.Earnings, AverageRating = s.AverageRating, CancelledCount = s.CancelledCount, CancellationRate = s.CancellationRate }).ToList();
        }
    }

    // ── M124: Analytics — Geographic Heatmap ───────────────────────
    public class GeoHeatmapService
    {
        private readonly AppDbContext _db;
        public GeoHeatmapService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, HeatmapDto? Heatmap)> GenerateAsync(GenerateHeatmapRequestDto req)
        {
            if (!DateTime.TryParseExact(req.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", null);
            var monthEnd = monthStart.AddMonths(1);

            var bookings = await _db.Bookings.Where(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd && b.Latitude != null && b.Longitude != null).ToListAsync();
            var grouped = bookings.GroupBy(b => (Lat: Math.Round(b.Latitude!.Value, 2), Lng: Math.Round(b.Longitude!.Value, 2)));

            var existing = await _db.GeoHeatmapCells.Where(c => c.PeriodKey == req.PeriodKey).ToListAsync();
            _db.GeoHeatmapCells.RemoveRange(existing);

            var cells = new List<GeoHeatmapCell>();
            foreach (var g in grouped)
                cells.Add(new GeoHeatmapCell { PeriodKey = req.PeriodKey, GridLatitude = g.Key.Lat, GridLongitude = g.Key.Lng, BookingCount = g.Count(), TotalRevenue = g.Sum(b => b.TotalAmount) });
            _db.GeoHeatmapCells.AddRange(cells);
            await _db.SaveChangesAsync();

            return (true, "Generated.", new HeatmapDto { PeriodKey = req.PeriodKey, Cells = cells.Select(ToDto).ToList() });
        }

        public async Task<HeatmapDto> GetAsync(string periodKey)
        {
            var cells = await _db.GeoHeatmapCells.Where(c => c.PeriodKey == periodKey).ToListAsync();
            return new HeatmapDto { PeriodKey = periodKey, Cells = cells.Select(ToDto).ToList() };
        }

        private static HeatmapCellDto ToDto(GeoHeatmapCell c) => new() { GridLatitude = c.GridLatitude, GridLongitude = c.GridLongitude, BookingCount = c.BookingCount, TotalRevenue = c.TotalRevenue };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/customer-ltv"), Authorize(Roles = "Admin")]
    public class CustomerLtvController : ControllerBase
    {
        private readonly Services.CustomerLtvService _svc;
        public CustomerLtvController(Services.CustomerLtvService svc) => _svc = svc;

        [HttpPost("compute")]
        public async Task<IActionResult> Compute([FromBody] ComputeLtvRequestDto req)
        {
            var (success, message, ltv) = await _svc.ComputeAsync(req.CustomerId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = ltv });
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int take = 20) => Ok(new { success = true, data = await _svc.GetLeaderboardAsync(take) });
    }

    [ApiController, Route("api/booking-funnel"), Authorize(Roles = "Admin")]
    public class BookingFunnelController : ControllerBase
    {
        private readonly Services.BookingFunnelService _svc;
        public BookingFunnelController(Services.BookingFunnelService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateFunnelRequestDto req)
        {
            var (success, message, funnel) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = funnel });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }

    [ApiController, Route("api/chef-performance-analytics"), Authorize(Roles = "Admin")]
    public class ChefPerformanceAnalyticsController : ControllerBase
    {
        private readonly Services.ChefPerformanceAnalyticsService _svc;
        public ChefPerformanceAnalyticsController(Services.ChefPerformanceAnalyticsService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateChefPerformanceRequestDto req)
        {
            var (success, message, count) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, chefsProcessed = count });
        }

        [HttpGet("{chefId}/trend")]
        public async Task<IActionResult> GetTrend(int chefId, [FromQuery] int months = 6)
        {
            var trend = await _svc.GetTrendAsync(chefId, months);
            if (trend == null) return NotFound(new { success = false, message = "Chef not found." });
            return Ok(new { success = true, data = trend });
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] string periodKey, [FromQuery] int take = 20) => Ok(new { success = true, data = await _svc.GetLeaderboardAsync(periodKey, take) });
    }

    [ApiController, Route("api/geo-heatmap"), Authorize(Roles = "Admin")]
    public class GeoHeatmapController : ControllerBase
    {
        private readonly Services.GeoHeatmapService _svc;
        public GeoHeatmapController(Services.GeoHeatmapService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateHeatmapRequestDto req)
        {
            var (success, message, heatmap) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = heatmap });
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string periodKey) => Ok(new { success = true, data = await _svc.GetAsync(periodKey) });
    }
}
