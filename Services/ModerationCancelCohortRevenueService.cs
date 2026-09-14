using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M117: AI Suite — Content Moderation ────────────────────────
    public class ContentModerationService
    {
        private readonly AppDbContext _db;
        // Small curated blocklist — deliberately simple/auditable, same
        // honesty convention as the rest of the AI Suite.
        private static readonly string[] ProfanityTerms = { "damn", "hell", "stupid", "idiot", "hate you" }; // placeholder mild list; real deployments would use a proper blocklist
        private static readonly string[] SpamPatterns = { "www.", "http://", "https://", "click here", "free money", "whatsapp me" };
        private static readonly string[] ContactLeakPatterns = { "@gmail", "@yahoo", "call me at", "my number is" };

        public ContentModerationService(AppDbContext db) => _db = db;

        public async Task<List<ModerationFlagDto>> ScanAsync(ScanContentRequestDto req)
        {
            var lower = (req.Text ?? "").ToLowerInvariant();
            var results = new List<ModerationFlagDto>();

            var flagged = new (string Type, string[] Terms)[] { ("Profanity", ProfanityTerms), ("Spam", SpamPatterns), ("ContactInfoLeak", ContactLeakPatterns) };
            foreach (var (type, terms) in flagged)
            {
                var hits = terms.Where(t => lower.Contains(t)).ToList();
                if (hits.Count == 0) continue;
                var flag = new ModerationFlag { SourceType = req.SourceType, SourceId = req.SourceId, FlagType = type, MatchedTerms = string.Join(", ", hits) };
                _db.ModerationFlags.Add(flag);
                results.Add(ToDto(flag));
            }
            if (results.Count > 0) await _db.SaveChangesAsync();
            return results;
        }

        public async Task<List<ModerationFlagDto>> GetAllAsync(string? status = null)
        {
            var q = _db.ModerationFlags.Include(f => f.ReviewedByAdmin).AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(f => f.Status == status);
            return (await q.OrderByDescending(f => f.FlaggedAt).ToListAsync()).Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message)> DecideAsync(int adminUserId, DecideModerationFlagRequestDto req)
        {
            var flag = await _db.ModerationFlags.FindAsync(req.FlagId);
            if (flag == null) return (false, "Flag not found.");
            flag.Status = req.Status;
            flag.ReviewedByAdminId = adminUserId;
            flag.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"Flag marked {req.Status}.");
        }

        private static ModerationFlagDto ToDto(ModerationFlag f) => new()
        {
            Id = f.Id, SourceType = f.SourceType, SourceId = f.SourceId, FlagType = f.FlagType, MatchedTerms = f.MatchedTerms,
            Status = f.Status, ReviewedByName = f.ReviewedByAdmin?.FullName, ReviewedAt = f.ReviewedAt, FlaggedAt = f.FlaggedAt,
        };
    }

    // ── M118: AI Suite — Cancellation Risk Prediction ──────────────
    public class CancellationRiskService
    {
        private readonly AppDbContext _db;
        public CancellationRiskService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CancellationRiskDto? Risk)> ComputeAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null) return (false, "Booking not found.", null);

            var custBookings = await _db.Bookings.Where(b => b.CustomerId == booking.CustomerId && b.Id != bookingId).ToListAsync();
            var chefBookings = await _db.Bookings.Where(b => b.ChefId == booking.ChefId && b.Id != bookingId).ToListAsync();

            var custRate = custBookings.Count == 0 ? 0 : Math.Round((decimal)custBookings.Count(b => b.Status == "Cancelled") / custBookings.Count * 100, 1);
            var chefRate = chefBookings.Count == 0 ? 0 : Math.Round((decimal)chefBookings.Count(b => b.Status == "Cancelled") / chefBookings.Count * 100, 1);
            var composite = Math.Clamp(custRate * 0.6m + chefRate * 0.4m, 0, 100);
            var level = composite >= 50 ? "High" : composite >= 25 ? "Medium" : "Low";

            var record = new CancellationRiskScore { BookingId = bookingId, CustomerCancellationRate = custRate, ChefCancellationRate = chefRate, RiskScore = composite, RiskLevel = level };
            _db.CancellationRiskScores.Add(record);
            await _db.SaveChangesAsync();

            return (true, "Computed.", new CancellationRiskDto { BookingId = bookingId, CustomerCancellationRate = custRate, ChefCancellationRate = chefRate, RiskScore = composite, RiskLevel = level, ComputedAt = record.ComputedAt });
        }

        public async Task<List<CancellationRiskDto>> GetHighRiskUpcomingAsync()
        {
            var upcomingBookingIds = await _db.Bookings.Where(b => (b.Status == "Pending" || b.Status == "Accepted") && b.ScheduledAt >= DateTime.UtcNow).Select(b => b.Id).ToListAsync();
            var latestScores = await _db.CancellationRiskScores.Where(s => upcomingBookingIds.Contains(s.BookingId))
                .GroupBy(s => s.BookingId).Select(g => g.OrderByDescending(s => s.ComputedAt).First())
                .Where(s => s.RiskLevel == "High").ToListAsync();
            return latestScores.Select(s => new CancellationRiskDto { BookingId = s.BookingId, CustomerCancellationRate = s.CustomerCancellationRate, ChefCancellationRate = s.ChefCancellationRate, RiskScore = s.RiskScore, RiskLevel = s.RiskLevel, ComputedAt = s.ComputedAt }).ToList();
        }
    }

    // ── M119: Analytics — Cohort Retention ─────────────────────────
    public class CohortRetentionService
    {
        private readonly AppDbContext _db;
        public CohortRetentionService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, List<CohortRetentionRowDto>? Rows)> GenerateAsync(GenerateCohortRequestDto req)
        {
            if (!DateTime.TryParseExact(req.CohortMonth + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var cohortStart))
                return (false, "CohortMonth must be in yyyy-MM format.", null);
            var cohortEnd = cohortStart.AddMonths(1);

            var cohortCustomerIds = await _db.Users.Where(u => u.CreatedAt >= cohortStart && u.CreatedAt < cohortEnd && u.Role == "Customer").Select(u => u.Id).ToListAsync();
            var cohortSize = cohortCustomerIds.Count;
            var rows = new List<CohortRetentionRowDto>();

            if (cohortSize > 0)
            {
                for (int offset = 0; offset <= req.MaxMonthOffset; offset++)
                {
                    var windowStart = cohortStart.AddMonths(offset);
                    var windowEnd = windowStart.AddMonths(1);
                    if (windowStart > DateTime.UtcNow) break;

                    var activeCount = await _db.Bookings.Where(b => cohortCustomerIds.Contains(b.CustomerId) && b.CreatedAt >= windowStart && b.CreatedAt < windowEnd).Select(b => b.CustomerId).Distinct().CountAsync();
                    var pct = Math.Round((decimal)activeCount / cohortSize * 100, 1);

                    var existing = await _db.CohortRetentionSnapshots.FirstOrDefaultAsync(s => s.CohortMonth == req.CohortMonth && s.MonthOffset == offset);
                    if (existing == null) { existing = new CohortRetentionSnapshot { CohortMonth = req.CohortMonth, MonthOffset = offset }; _db.CohortRetentionSnapshots.Add(existing); }
                    existing.CohortSize = cohortSize;
                    existing.ActiveCount = activeCount;
                    existing.RetentionPercent = pct;
                    existing.GeneratedAt = DateTime.UtcNow;

                    rows.Add(new CohortRetentionRowDto { CohortMonth = req.CohortMonth, MonthOffset = offset, CohortSize = cohortSize, ActiveCount = activeCount, RetentionPercent = pct });
                }
                await _db.SaveChangesAsync();
            }
            return (true, "Generated.", rows);
        }

        public async Task<CohortGridDto> GetGridAsync()
        {
            var snapshots = await _db.CohortRetentionSnapshots.OrderBy(s => s.CohortMonth).ThenBy(s => s.MonthOffset).ToListAsync();
            return new CohortGridDto
            {
                Cohorts = snapshots.Select(s => s.CohortMonth).Distinct().OrderBy(c => c).ToList(),
                Rows = snapshots.Select(s => new CohortRetentionRowDto { CohortMonth = s.CohortMonth, MonthOffset = s.MonthOffset, CohortSize = s.CohortSize, ActiveCount = s.ActiveCount, RetentionPercent = s.RetentionPercent }).ToList(),
            };
        }
    }

    // ── M120: Analytics — Revenue Analytics ────────────────────────
    public class RevenueAnalyticsService
    {
        private readonly AppDbContext _db;
        public RevenueAnalyticsService(AppDbContext db) => _db = db;

        // Generates the day's snapshot rows: one all-cities/all-category
        // total, one per city, one per category (Cuisine used as the
        // category proxy — Booking has no dedicated Category field).
        public async Task<(bool Success, string Message, List<RevenueSnapshotDto>? Rows)> GenerateAsync(GenerateRevenueSnapshotRequestDto req)
        {
            if (!DateTime.TryParseExact(req.DateKey, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var day))
                return (false, "DateKey must be in yyyy-MM-dd format.", null);
            var dayEnd = day.AddDays(1);

            var bookings = await _db.Bookings.Where(b => b.Status == "Completed" && b.CompletedAt >= day && b.CompletedAt < dayEnd).ToListAsync();
            var chefCities = await _db.ChefProfiles.ToDictionaryAsync(c => c.UserId, c => c.City);

            var rows = new List<RevenueAnalyticsSnapshot>
            {
                new() { DateKey = req.DateKey, City = null, Category = null, Revenue = bookings.Sum(b => b.TotalAmount), BookingCount = bookings.Count },
            };
            foreach (var g in bookings.GroupBy(b => chefCities.GetValueOrDefault(b.ChefId) ?? "Unknown"))
                rows.Add(new() { DateKey = req.DateKey, City = g.Key, Category = null, Revenue = g.Sum(b => b.TotalAmount), BookingCount = g.Count() });
            foreach (var g in bookings.GroupBy(b => b.Cuisine ?? "Unspecified"))
                rows.Add(new() { DateKey = req.DateKey, City = null, Category = g.Key, Revenue = g.Sum(b => b.TotalAmount), BookingCount = g.Count() });

            // Replace any existing rows for this date to keep regeneration idempotent.
            var existingRows = await _db.RevenueAnalyticsSnapshots.Where(s => s.DateKey == req.DateKey).ToListAsync();
            _db.RevenueAnalyticsSnapshots.RemoveRange(existingRows);
            _db.RevenueAnalyticsSnapshots.AddRange(rows);
            await _db.SaveChangesAsync();

            return (true, "Generated.", rows.Select(ToDto).ToList());
        }

        public async Task<RevenueTrendDto> GetTrendAsync(RevenueTrendRequestDto req)
        {
            var all = await _db.RevenueAnalyticsSnapshots.Where(s => string.Compare(s.DateKey, req.FromDateKey) >= 0 && string.Compare(s.DateKey, req.ToDateKey) <= 0).ToListAsync();
            var dailyTotals = all.Where(s => s.City == null && s.Category == null).OrderBy(s => s.DateKey).Select(ToDto).ToList();
            var cityTotals = all.Where(s => s.City != null).GroupBy(s => s.City).Select(g => new RevenueSnapshotDto { City = g.Key, Revenue = g.Sum(x => x.Revenue), BookingCount = g.Sum(x => x.BookingCount) }).OrderByDescending(x => x.Revenue).Take(10).ToList();
            var categoryTotals = all.Where(s => s.Category != null).GroupBy(s => s.Category).Select(g => new RevenueSnapshotDto { Category = g.Key, Revenue = g.Sum(x => x.Revenue), BookingCount = g.Sum(x => x.BookingCount) }).OrderByDescending(x => x.Revenue).Take(10).ToList();

            return new RevenueTrendDto
            {
                TotalRevenue = dailyTotals.Sum(d => d.Revenue), TotalBookings = dailyTotals.Sum(d => d.BookingCount),
                DailyTotals = dailyTotals, TopCities = cityTotals, TopCategories = categoryTotals,
            };
        }

        private static RevenueSnapshotDto ToDto(RevenueAnalyticsSnapshot s) => new() { DateKey = s.DateKey, City = s.City, Category = s.Category, Revenue = s.Revenue, BookingCount = s.BookingCount };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/content-moderation"), Authorize(Roles = "Admin")]
    public class ContentModerationController : ControllerBase
    {
        private readonly Services.ContentModerationService _svc;
        public ContentModerationController(Services.ContentModerationService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] ScanContentRequestDto req) => Ok(new { success = true, data = await _svc.ScanAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status) => Ok(new { success = true, data = await _svc.GetAllAsync(status) });

        [HttpPost("decide")]
        public async Task<IActionResult> Decide([FromBody] DecideModerationFlagRequestDto req)
        {
            var (success, message) = await _svc.DecideAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/cancellation-risk"), Authorize(Roles = "Admin")]
    public class CancellationRiskController : ControllerBase
    {
        private readonly Services.CancellationRiskService _svc;
        public CancellationRiskController(Services.CancellationRiskService svc) => _svc = svc;

        [HttpPost("compute")]
        public async Task<IActionResult> Compute([FromBody] ComputeCancellationRiskRequestDto req)
        {
            var (success, message, risk) = await _svc.ComputeAsync(req.BookingId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = risk });
        }

        [HttpGet("high-risk-upcoming")]
        public async Task<IActionResult> GetHighRiskUpcoming() => Ok(new { success = true, data = await _svc.GetHighRiskUpcomingAsync() });
    }

    [ApiController, Route("api/cohort-retention"), Authorize(Roles = "Admin")]
    public class CohortRetentionController : ControllerBase
    {
        private readonly Services.CohortRetentionService _svc;
        public CohortRetentionController(Services.CohortRetentionService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateCohortRequestDto req)
        {
            var (success, message, rows) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = rows });
        }

        [HttpGet("grid")]
        public async Task<IActionResult> GetGrid() => Ok(new { success = true, data = await _svc.GetGridAsync() });
    }

    [ApiController, Route("api/revenue-analytics"), Authorize(Roles = "Admin")]
    public class RevenueAnalyticsController : ControllerBase
    {
        private readonly Services.RevenueAnalyticsService _svc;
        public RevenueAnalyticsController(Services.RevenueAnalyticsService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRevenueSnapshotRequestDto req)
        {
            var (success, message, rows) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = rows });
        }

        [HttpGet("trend")]
        public async Task<IActionResult> GetTrend([FromQuery] string fromDateKey, [FromQuery] string toDateKey)
            => Ok(new { success = true, data = await _svc.GetTrendAsync(new RevenueTrendRequestDto { FromDateKey = fromDateKey, ToDateKey = toDateKey }) });
    }
}
