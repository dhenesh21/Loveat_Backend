using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class CityManagementService
    {
        private readonly AppDbContext _db;
        public CityManagementService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message)> AssignManagerAsync(AssignCityManagerRequestDto req)
        {
            var city = await _db.ServiceCities.FindAsync(req.ServiceCityId);
            if (city == null) return (false, "Service city not found.");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId && u.Role == "Admin");
            if (user == null) return (false, "User not found or is not an admin.");
            var existing = await _db.CityManagerAssignments.Where(a => a.ServiceCityId == req.ServiceCityId && a.IsActive).ToListAsync();
            foreach (var e in existing) e.IsActive = false;
            _db.CityManagerAssignments.Add(new CityManagerAssignment { ServiceCityId = req.ServiceCityId, UserId = req.UserId });
            await _db.SaveChangesAsync();
            return (true, $"{user.FullName} assigned as manager for {city.CityName}.");
        }

        public async Task<List<CityManagerDto>> GetManagersAsync()
        {
            var assignments = await _db.CityManagerAssignments.Include(a => a.ServiceCity).Include(a => a.User).Where(a => a.IsActive).ToListAsync();
            return assignments.Select(a => new CityManagerDto { ServiceCityId = a.ServiceCityId, CityName = a.ServiceCity?.CityName ?? "", UserId = a.UserId, ManagerName = a.User?.FullName ?? "", AssignedAt = a.AssignedAt }).ToList();
        }

        public async Task<List<CityPerformanceDto>> GetPerformanceAsync(int? serviceCityId = null)
        {
            var cities = serviceCityId.HasValue ? await _db.ServiceCities.Where(c => c.Id == serviceCityId).ToListAsync() : await _db.ServiceCities.Where(c => c.IsActive).ToListAsync();
            var result = new List<CityPerformanceDto>();
            foreach (var city in cities) result.Add(await RecomputeAsync(city));
            return result;
        }

        private async Task<CityPerformanceDto> RecomputeAsync(ServiceCity city)
        {
            var chefIds = await _db.ChefProfiles.Where(p => p.City == city.CityName).Select(p => p.UserId).ToListAsync();
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var bookings = chefIds.Count == 0 ? new List<Booking>() : await _db.Bookings.Where(b => chefIds.Contains(b.ChefId) && b.CreatedAt >= monthStart).ToListAsync();
            var revenue = bookings.Where(b => b.PaymentStatus == "Paid").Sum(b => b.TotalAmount);
            var activeChefs = await _db.ChefProfiles.CountAsync(p => p.City == city.CityName && p.IsAvailable);
            var avgRating = chefIds.Count == 0 ? 0 : await _db.ChefProfiles.Where(p => p.City == city.CityName).AverageAsync(p => (decimal?)p.AverageRating) ?? 0;
            var periodKey = monthStart.ToString("yyyy-MM");
            var snapshot = await _db.CityPerformanceSnapshots.FirstOrDefaultAsync(s => s.ServiceCityId == city.Id && s.PeriodKey == periodKey);
            if (snapshot == null) { snapshot = new CityPerformanceSnapshot { ServiceCityId = city.Id, PeriodKey = periodKey }; _db.CityPerformanceSnapshots.Add(snapshot); }
            snapshot.TotalBookings = bookings.Count;
            snapshot.TotalRevenue = revenue;
            snapshot.ActiveChefs = activeChefs;
            snapshot.AverageRating = Math.Round(avgRating, 2);
            snapshot.ComputedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return new CityPerformanceDto { ServiceCityId = city.Id, CityName = city.CityName, TotalBookings = bookings.Count, TotalRevenue = revenue, ActiveChefs = activeChefs, AverageRating = snapshot.AverageRating, PeriodKey = periodKey };
        }
    }

    public class RegionalManagerService
    {
        private readonly AppDbContext _db;
        public RegionalManagerService(AppDbContext db) => _db = db;

        public async Task<RegionDto> CreateRegionAsync(CreateRegionRequestDto req)
        {
            var region = new Region { Name = req.Name, ManagerUserId = req.ManagerUserId };
            _db.Regions.Add(region);
            await _db.SaveChangesAsync();
            foreach (var cityId in req.ServiceCityIds.Distinct()) _db.RegionCities.Add(new RegionCity { RegionId = region.Id, ServiceCityId = cityId });
            await _db.SaveChangesAsync();
            return await ToDtoAsync(region.Id);
        }

        public async Task<List<RegionDto>> GetAllAsync()
        {
            var regions = await _db.Regions.OrderBy(r => r.Name).ToListAsync();
            var result = new List<RegionDto>();
            foreach (var r in regions) result.Add(await ToDtoAsync(r.Id));
            return result;
        }

        private async Task<RegionDto> ToDtoAsync(int regionId)
        {
            var region = await _db.Regions.Include(r => r.Manager).FirstAsync(r => r.Id == regionId);
            var cityIds = await _db.RegionCities.Where(rc => rc.RegionId == regionId).Select(rc => rc.ServiceCityId).ToListAsync();
            var cities = await _db.ServiceCities.Where(c => cityIds.Contains(c.Id)).ToListAsync();
            return new RegionDto { Id = region.Id, Name = region.Name, ManagerName = region.Manager?.FullName, Cities = cities.Select(c => c.CityName).ToList(), TotalChefs = cities.Sum(c => c.ChefCount), TotalBookings = cities.Sum(c => c.BookingCount) };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/city-management"), Authorize(Roles = "Admin")]
    public class CityManagementController : ControllerBase
    {
        private readonly Services.CityManagementService _svc;
        public CityManagementController(Services.CityManagementService svc) => _svc = svc;

        [HttpPost("managers")]
        public async Task<IActionResult> AssignManager([FromBody] AssignCityManagerRequestDto req)
        {
            var (success, message) = await _svc.AssignManagerAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers() => Ok(new { success = true, data = await _svc.GetManagersAsync() });

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformance([FromQuery] int? serviceCityId) => Ok(new { success = true, data = await _svc.GetPerformanceAsync(serviceCityId) });
    }

    [ApiController, Route("api/regions"), Authorize(Roles = "Admin")]
    public class RegionController : ControllerBase
    {
        private readonly Services.RegionalManagerService _svc;
        public RegionController(Services.RegionalManagerService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRegionRequestDto req) => Ok(new { success = true, data = await _svc.CreateRegionAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }
}
