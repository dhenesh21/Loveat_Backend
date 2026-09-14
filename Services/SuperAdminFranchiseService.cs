using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // M83: Super Admin Dashboard Service
    // ══════════════════════════════════════════════════════════════
    public class SuperAdminDashboardService
    {
        private readonly AppDbContext _db;
        public SuperAdminDashboardService(AppDbContext db) => _db = db;

        public async Task<SuperAdminDashboardDto> GetDashboardAsync()
        {
            var totalRevenue = await _db.Payments.Where(p => p.Status == "Success").SumAsync(p => (decimal?)p.Amount) ?? 0;
            var activeUsers = await _db.Users.CountAsync(u => u.IsActive);
            var activeChefs = await _db.ChefProfiles.CountAsync(p => p.IsAvailable);
            var liveBookings = await _db.Bookings.CountAsync(b => b.Status == "Accepted" || b.Status == "InProgress");

            var recentMetrics = await _db.PlatformHealthMetrics
                .OrderByDescending(m => m.RecordedAt)
                .Take(10)
                .ToListAsync();

            var errorMetrics = recentMetrics.Where(m => m.MetricName == "ErrorRate").ToList();
            var healthScore = errorMetrics.Count == 0
                ? 100
                : Math.Clamp(100 - (int)Math.Round(errorMetrics.Average(m => m.MetricValue) * 10), 0, 100);

            return new SuperAdminDashboardDto
            {
                TotalRevenue = totalRevenue,
                ActiveUsers = activeUsers,
                ActiveChefs = activeChefs,
                LiveBookings = liveBookings,
                PlatformHealthScore = healthScore,
                RecentMetrics = recentMetrics.Select(m => new PlatformHealthMetricDto
                {
                    MetricName = m.MetricName, MetricValue = m.MetricValue, RecordedAt = m.RecordedAt,
                }).ToList(),
            };
        }

        public async Task RecordMetricAsync(string metricName, decimal value)
        {
            _db.PlatformHealthMetrics.Add(new PlatformHealthMetric { MetricName = metricName, MetricValue = value });
            await _db.SaveChangesAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // M84: Franchise Management Service
    // ══════════════════════════════════════════════════════════════
    public class FranchiseService
    {
        private readonly AppDbContext _db;
        public FranchiseService(AppDbContext db) => _db = db;

        public async Task<FranchiseDto> CreateAsync(CreateFranchiseRequestDto req)
        {
            var franchise = new Franchise
            {
                FranchiseeName = req.FranchiseeName,
                ContactEmail = req.ContactEmail,
                ContactPhone = req.ContactPhone,
                RegionOrCity = req.RegionOrCity,
                RevenueSharePercent = req.RevenueSharePercent,
            };
            _db.Franchises.Add(franchise);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(franchise);
        }

        public async Task<List<FranchiseDto>> GetAllAsync()
        {
            var franchises = await _db.Franchises.OrderByDescending(f => f.CreatedAt).ToListAsync();
            var result = new List<FranchiseDto>();
            foreach (var f in franchises) result.Add(await ToDtoAsync(f));
            return result;
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(int id, string status)
        {
            var franchise = await _db.Franchises.FindAsync(id);
            if (franchise == null) return (false, "Franchise not found.");
            franchise.Status = status;
            await _db.SaveChangesAsync();
            return (true, $"Franchise marked {status}.");
        }

        public async Task<(bool Success, string Message)> UpsertSettingAsync(int franchiseId, UpsertFranchiseSettingRequestDto req)
        {
            var franchise = await _db.Franchises.FindAsync(franchiseId);
            if (franchise == null) return (false, "Franchise not found.");

            var setting = await _db.FranchiseSettings.FirstOrDefaultAsync(s => s.FranchiseId == franchiseId && s.SettingKey == req.SettingKey);
            if (setting == null)
            {
                setting = new FranchiseSettings { FranchiseId = franchiseId, SettingKey = req.SettingKey };
                _db.FranchiseSettings.Add(setting);
            }
            setting.SettingValue = req.SettingValue;
            setting.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Setting saved.");
        }

        public async Task<List<FranchiseSettingDto>> GetSettingsAsync(int franchiseId)
        {
            var settings = await _db.FranchiseSettings.Where(s => s.FranchiseId == franchiseId).ToListAsync();
            return settings.Select(s => new FranchiseSettingDto { SettingKey = s.SettingKey, SettingValue = s.SettingValue, UpdatedAt = s.UpdatedAt }).ToList();
        }

        /// <summary>Revenue share is estimated from successful payments for bookings whose chef is registered in the franchise's region — ChefProfile.City is the closest existing signal for "which region a booking belongs to" (Booking itself only has a free-text Address, not a structured city).</summary>
        private async Task<FranchiseDto> ToDtoAsync(Franchise f)
        {
            var chefIdsInRegion = await _db.ChefProfiles.Where(p => p.City == f.RegionOrCity).Select(p => p.UserId).ToListAsync();

            var regionRevenue = chefIdsInRegion.Count == 0 ? 0 : await _db.Bookings
                .Where(b => chefIdsInRegion.Contains(b.ChefId) && b.PaymentStatus == "Paid")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            return new FranchiseDto
            {
                Id = f.Id,
                FranchiseeName = f.FranchiseeName,
                ContactEmail = f.ContactEmail,
                ContactPhone = f.ContactPhone,
                RegionOrCity = f.RegionOrCity,
                RevenueSharePercent = f.RevenueSharePercent,
                Status = f.Status,
                EstimatedRevenueShareAmount = Math.Round(regionRevenue * f.RevenueSharePercent / 100m, 2),
                CreatedAt = f.CreatedAt,
            };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/super-admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class SuperAdminDashboardController : ControllerBase
    {
        private readonly Services.SuperAdminDashboardService _svc;
        public SuperAdminDashboardController(Services.SuperAdminDashboardService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _svc.GetDashboardAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("metrics")]
        [AllowAnonymous]
        public async Task<IActionResult> RecordMetric([FromBody] RecordHealthMetricRequestDto req)
        {
            await _svc.RecordMetricAsync(req.MetricName, req.MetricValue);
            return Ok(new { success = true });
        }
    }

    [ApiController]
    [Route("api/franchises")]
    [Authorize(Roles = "Admin")]
    public class FranchiseController : ControllerBase
    {
        private readonly Services.FranchiseService _svc;
        public FranchiseController(Services.FranchiseService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFranchiseRequestDto req)
        {
            var data = await _svc.CreateAsync(req);
            return Ok(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _svc.GetAllAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateFranchiseStatusRequestDto req)
        {
            var (success, message) = await _svc.UpdateStatusAsync(id, req.Status);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("{id}/settings")]
        public async Task<IActionResult> UpsertSetting(int id, [FromBody] UpsertFranchiseSettingRequestDto req)
        {
            var (success, message) = await _svc.UpsertSettingAsync(id, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("{id}/settings")]
        public async Task<IActionResult> GetSettings(int id)
        {
            var data = await _svc.GetSettingsAsync(id);
            return Ok(new { success = true, data });
        }
    }
}
