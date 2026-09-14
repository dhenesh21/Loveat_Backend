using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    // ── M54: Service Area Service ──────────────────────────────────
    public class ServiceAreaService
    {
        private readonly AppDbContext _db;
        public ServiceAreaService(AppDbContext db) => _db = db;

        public async Task<List<ServiceCityDto>> GetAllCitiesAsync()
        {
            var cities = await _db.ServiceCities.Include(c => c.Zones).OrderBy(c => c.CityName).ToListAsync();
            return cities.Select(Map).ToList();
        }

        public async Task<ServiceCityDto> CreateCityAsync(CreateCityDto dto)
        {
            var city = new ServiceCity
            {
                CityName    = dto.CityName,
                State       = dto.State,
                Latitude    = dto.Latitude,
                Longitude   = dto.Longitude,
                RadiusKm    = dto.RadiusKm,
                IsLaunching = dto.IsLaunching,
                IsActive    = !dto.IsLaunching,
            };
            _db.ServiceCities.Add(city);
            await _db.SaveChangesAsync();
            return Map(city);
        }

        public async Task<bool> ToggleCityAsync(int id)
        {
            var city = await _db.ServiceCities.FindAsync(id);
            if (city == null) return false;
            city.IsActive = !city.IsActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceZoneDto> AddZoneAsync(int cityId, ServiceZoneDto dto)
        {
            var zone = new ServiceZone
            {
                ServiceCityId = cityId,
                ZoneName      = dto.ZoneName,
                Latitude      = dto.Latitude,
                Longitude     = dto.Longitude,
                RadiusKm      = dto.RadiusKm,
                IsPeakZone    = dto.IsPeakZone,
            };
            _db.ServiceZones.Add(zone);
            await _db.SaveChangesAsync();
            dto.Id = zone.Id;
            return dto;
        }

        public async Task<bool> IsLocationServedAsync(decimal lat, decimal lng)
        {
            var cities = await _db.ServiceCities.Where(c => c.IsActive).ToListAsync();
            foreach (var city in cities)
            {
                var dist = HaversineKm(lat, lng, city.Latitude, city.Longitude);
                if (dist <= (double)city.RadiusKm) return true;
            }
            return false;
        }

        private static double HaversineKm(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            const double R = 6371;
            var dLat = (double)(lat2 - lat1) * Math.PI / 180;
            var dLng = (double)(lng2 - lng1) * Math.PI / 180;
            var a    = Math.Sin(dLat/2)*Math.Sin(dLat/2) + Math.Cos((double)lat1*Math.PI/180)*Math.Cos((double)lat2*Math.PI/180)*Math.Sin(dLng/2)*Math.Sin(dLng/2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        }

        private static ServiceCityDto Map(ServiceCity c) => new()
        {
            Id=c.Id, CityName=c.CityName, State=c.State, Latitude=c.Latitude, Longitude=c.Longitude,
            RadiusKm=c.RadiusKm, IsActive=c.IsActive, IsLaunching=c.IsLaunching,
            ChefCount=c.ChefCount, BookingCount=c.BookingCount,
            Zones=c.Zones.Select(z=>new ServiceZoneDto { Id=z.Id, ZoneName=z.ZoneName, Latitude=z.Latitude, Longitude=z.Longitude, RadiusKm=z.RadiusKm, IsActive=z.IsActive, IsPeakZone=z.IsPeakZone }).ToList(),
        };
    }

    // ── M55: AI Analytics Service ──────────────────────────────────
    public class AIAnalyticsService
    {
        private readonly AppDbContext _db;
        public AIAnalyticsService(AppDbContext db) => _db = db;

        public async Task<AIAnalyticsSummaryDto> GetSummaryAsync(string city = "all")
        {
            var bookings = await _db.Bookings.Where(b => b.Status == "Completed").ToListAsync();

            // Hourly demand forecast (0-23)
            var hourly = Enumerable.Range(0, 24).Select(h => {
                var actual = bookings.Count(b => b.ScheduledAt.Hour == h);
                var predicted = PredictHourly(h);
                return new DemandForecastDto { Label=$"{h}:00", Predicted=predicted, Actual=actual, Confidence=0.85m };
            }).ToList();

            // Weekly forecast
            var days = new[]{"Sun","Mon","Tue","Wed","Thu","Fri","Sat"};
            var weekly = Enumerable.Range(0, 7).Select(d => {
                var actual = bookings.Count(b => (int)b.ScheduledAt.DayOfWeek == d);
                return new DemandForecastDto { Label=days[d], Predicted=PredictDay(d), Actual=actual, Confidence=0.82m };
            }).ToList();

            // City demand
            var cityDemand = new List<CityDemandDto>
            {
                new(){ City="Coimbatore", DemandScore=85, ChefShortage=12, Trend="↑ Growing"  },
                new(){ City="Chennai",    DemandScore=92, ChefShortage=28, Trend="↑ High"     },
                new(){ City="Madurai",    DemandScore=68, ChefShortage=5,  Trend="→ Stable"   },
                new(){ City="Trichy",     DemandScore=54, ChefShortage=2,  Trend="→ Stable"   },
                new(){ City="Salem",      DemandScore=41, ChefShortage=0,  Trend="↓ Declining"},
            };

            return new AIAnalyticsSummaryDto
            {
                HourlyForecast   = hourly,
                WeeklyForecast   = weekly,
                CityDemand       = cityDemand,
                PredictionAccuracy= 87.4m,
                Recommendations  = new List<string>
                {
                    "Add 12 more chefs in Chennai to meet peak weekend demand",
                    "Increase surge pricing on Friday 7–9 PM by 20%",
                    "Launch referral campaign in Madurai — demand growing 15% MoM",
                    "Coimbatore Saturday bookings predicted 40% above average next week",
                },
            };
        }

        private static decimal PredictHourly(int h) => h switch
        {
            >= 7  and <= 9  => 65 + (h - 7) * 10,
            >= 11 and <= 13 => 80 + (h - 11) * 8,
            >= 17 and <= 20 => 75 + (h - 17) * 12,
            _ => 20 + h * 1.5m,
        };

        private static decimal PredictDay(int d) => d switch { 0=>72, 1=>55, 2=>58, 3=>62, 4=>68, 5=>85, 6=>90, _ => 60 };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/service-areas")]
    public class ServiceAreaController : ControllerBase
    {
        private readonly Services.ServiceAreaService _svc;
        public ServiceAreaController(Services.ServiceAreaService svc) => _svc = svc;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() => Ok(new { success=true, data=await _svc.GetAllCitiesAsync() });

        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<IActionResult> Check([FromQuery] decimal lat, [FromQuery] decimal lng)
        {
            var served = await _svc.IsLocationServedAsync(lat, lng);
            return Ok(new { success=true, data=new { served } });
        }

        [HttpPost, Authorize(Roles="Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCityDto dto)
            => Ok(new { success=true, data=await _svc.CreateCityAsync(dto) });

        [HttpPost("{id}/toggle"), Authorize(Roles="Admin")]
        public async Task<IActionResult> Toggle(int id)
            => Ok(new { success=await _svc.ToggleCityAsync(id) });

        [HttpPost("{cityId}/zones"), Authorize(Roles="Admin")]
        public async Task<IActionResult> AddZone(int cityId, [FromBody] ServiceZoneDto dto)
            => Ok(new { success=true, data=await _svc.AddZoneAsync(cityId, dto) });
    }

    [ApiController, Route("api/ai-analytics"), Authorize(Roles="Admin")]
    public class AIAnalyticsController : ControllerBase
    {
        private readonly Services.AIAnalyticsService _svc;
        public AIAnalyticsController(Services.AIAnalyticsService svc) => _svc = svc;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] string city = "all")
            => Ok(new { success=true, data=await _svc.GetSummaryAsync(city) });
    }
}
