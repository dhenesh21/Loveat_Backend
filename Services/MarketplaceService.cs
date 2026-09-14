using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    public class MarketplaceService
    {
        private readonly AppDbContext _db;
        public MarketplaceService(AppDbContext db) => _db = db;

        public async Task<MarketplaceOverviewDto> GetOverviewAsync()
        {
            var services = await _db.MarketplaceServices.OrderBy(s => s.SortOrder).ToListAsync();
            var cities   = await _db.ExpansionCities.OrderBy(c => c.LaunchStatus).ToListAsync();
            var waitlist = await _db.CityWaitlists.CountAsync();

            return new MarketplaceOverviewDto
            {
                TotalServices     = services.Count,
                ActiveServices    = services.Count(s => s.IsActive && !s.IsComingSoon),
                ComingSoonServices= services.Count(s => s.IsComingSoon),
                LiveCities        = cities.Count(c => c.LaunchStatus == "Live"),
                PlannedCities     = cities.Count(c => c.LaunchStatus == "Planned" || c.LaunchStatus == "Soft Launch"),
                TotalWaitlist     = waitlist,
                Services          = services.Select(s => new MarketplaceServiceDto
                {
                    Id=s.Id, ServiceName=s.ServiceName, Description=s.Description,
                    IconName=s.IconName, BasePrice=s.BasePrice, PriceUnit=s.PriceUnit,
                    IsActive=s.IsActive, IsNew=s.IsNew, IsComingSoon=s.IsComingSoon,
                    ChefCount=0,
                }).ToList(),
                Cities = cities.Select(c => new ExpansionCityDto
                {
                    Id=c.Id, CityName=c.CityName, State=c.State, Country=c.Country,
                    Latitude=c.Latitude, Longitude=c.Longitude, LaunchStatus=c.LaunchStatus,
                    PlannedLaunch=c.PlannedLaunch?.ToString("MMM yyyy"),
                    ActualLaunch=c.ActualLaunch?.ToString("MMM dd, yyyy"),
                    TargetChefs=c.TargetChefs, CurrentChefs=c.CurrentChefs, WaitlistCount=c.WaitlistCount,
                    ReadinessPercent=c.TargetChefs>0 ? Math.Round((double)c.CurrentChefs/c.TargetChefs*100,1) : 0,
                    LaunchNotes=c.LaunchNotes,
                }).ToList(),
            };
        }

        public async Task<bool> JoinWaitlistAsync(int userId, JoinWaitlistDto dto)
        {
            var existing = await _db.CityWaitlists.AnyAsync(w => w.UserId==userId && w.CityName==dto.CityName);
            if (existing) return false;
            _db.CityWaitlists.Add(new CityWaitlist { UserId=userId, CityName=dto.CityName, UserType=dto.UserType, Phone=dto.Phone, Email=dto.Email });
            var city = await _db.ExpansionCities.FirstOrDefaultAsync(c => c.CityName==dto.CityName);
            if (city != null) city.WaitlistCount++;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCityStatusAsync(int cityId, string status, string? notes)
        {
            var city = await _db.ExpansionCities.FindAsync(cityId);
            if (city == null) return false;
            city.LaunchStatus = status;
            city.LaunchNotes  = notes ?? city.LaunchNotes;
            if (status == "Live") city.ActualLaunch = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task SeedAsync()
        {
            if (!await _db.MarketplaceServices.AnyAsync())
            {
                _db.MarketplaceServices.AddRange(
                    new Models.MarketplaceService { ServiceName="Home Cooking",       Description="Personal chef cooks fresh meals at your home", IconName="home-outline",         BasePrice=400,  PriceUnit="per hour",    IsActive=true,  SortOrder=1 },
                    new Models.MarketplaceService { ServiceName="Event Catering",     Description="Chef team for weddings, parties and corporate events", IconName="restaurant-outline",  BasePrice=150,  PriceUnit="per head",    IsActive=true,  SortOrder=2 },
                    new Models.MarketplaceService { ServiceName="Meal Prep",          Description="Weekly batch cooking to stock your fridge", IconName="fast-food-outline",  BasePrice=800,  PriceUnit="per session", IsActive=true,  IsNew=true, SortOrder=3 },
                    new Models.MarketplaceService { ServiceName="Cooking Class",      Description="Learn to cook authentic recipes with a pro chef", IconName="school-outline",      BasePrice=600,  PriceUnit="per session", IsActive=true,  IsNew=true, SortOrder=4 },
                    new Models.MarketplaceService { ServiceName="Nutrition Consult",  Description="Personalized diet and meal planning consultation", IconName="nutrition-outline",   BasePrice=500,  PriceUnit="per session", IsActive=false, IsComingSoon=true, SortOrder=5 },
                    new Models.MarketplaceService { ServiceName="Corporate Tiffin",   Description="Daily office meals on a monthly subscription", IconName="business-outline",    BasePrice=80,   PriceUnit="per meal",    IsActive=true,  SortOrder=6 },
                    new Models.MarketplaceService { ServiceName="Festive Hampers",    Description="Chef-prepared festive food hampers for gifting", IconName="gift-outline",        BasePrice=1200, PriceUnit="per hamper",  IsActive=false, IsComingSoon=true, SortOrder=7 }
                );
            }

            if (!await _db.ExpansionCities.AnyAsync())
            {
                _db.ExpansionCities.AddRange(
                    new ExpansionCity { CityName="Coimbatore", State="Tamil Nadu", Latitude=11.0168m, Longitude=76.9558m, LaunchStatus="Live",       ActualLaunch=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc),   TargetChefs=60,  CurrentChefs=48, WaitlistCount=0   },
                    new ExpansionCity { CityName="Chennai",    State="Tamil Nadu", Latitude=13.0827m, Longitude=80.2707m, LaunchStatus="Live",       ActualLaunch=new DateTime(2024,3,1,0,0,0,DateTimeKind.Utc),   TargetChefs=100, CurrentChefs=85, WaitlistCount=0   },
                    new ExpansionCity { CityName="Madurai",    State="Tamil Nadu", Latitude=9.9252m,  Longitude=78.1198m, LaunchStatus="Live",       ActualLaunch=new DateTime(2024,6,1,0,0,0,DateTimeKind.Utc),   TargetChefs=30,  CurrentChefs=22, WaitlistCount=0   },
                    new ExpansionCity { CityName="Trichy",     State="Tamil Nadu", Latitude=10.7905m, Longitude=78.7047m, LaunchStatus="Live",       ActualLaunch=new DateTime(2024,8,1,0,0,0,DateTimeKind.Utc),   TargetChefs=20,  CurrentChefs=14, WaitlistCount=0   },
                    new ExpansionCity { CityName="Bangalore",  State="Karnataka",  Latitude=12.9716m, Longitude=77.5946m, LaunchStatus="Soft Launch",PlannedLaunch=new DateTime(2025,1,1,0,0,0,DateTimeKind.Utc),  TargetChefs=80,  CurrentChefs=12, WaitlistCount=340 },
                    new ExpansionCity { CityName="Hyderabad",  State="Telangana",  Latitude=17.3850m, Longitude=78.4867m, LaunchStatus="Soft Launch",PlannedLaunch=new DateTime(2025,2,1,0,0,0,DateTimeKind.Utc),  TargetChefs=80,  CurrentChefs=8,  WaitlistCount=280 },
                    new ExpansionCity { CityName="Pune",       State="Maharashtra",Latitude=18.5204m, Longitude=73.8567m, LaunchStatus="Planned",    PlannedLaunch=new DateTime(2025,4,1,0,0,0,DateTimeKind.Utc),  TargetChefs=60,  CurrentChefs=0,  WaitlistCount=190 },
                    new ExpansionCity { CityName="Kochi",      State="Kerala",     Latitude=9.9312m,  Longitude=76.2673m, LaunchStatus="Planned",    PlannedLaunch=new DateTime(2025,3,1,0,0,0,DateTimeKind.Utc),  TargetChefs=40,  CurrentChefs=0,  WaitlistCount=150 },
                    new ExpansionCity { CityName="Mumbai",     State="Maharashtra",Latitude=19.0760m, Longitude=72.8777m, LaunchStatus="Planned",    PlannedLaunch=new DateTime(2025,6,1,0,0,0,DateTimeKind.Utc),  TargetChefs=120, CurrentChefs=0,  WaitlistCount=520 },
                    new ExpansionCity { CityName="Delhi",      State="Delhi",      Latitude=28.6139m, Longitude=77.2090m, LaunchStatus="Planned",    PlannedLaunch=new DateTime(2025,9,1,0,0,0,DateTimeKind.Utc),  TargetChefs=150, CurrentChefs=0,  WaitlistCount=680 }
                );
            }
            await _db.SaveChangesAsync();
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/marketplace")]
    public class MarketplaceController : ControllerBase
    {
        private readonly Services.MarketplaceService _svc;
        public MarketplaceController(Services.MarketplaceService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("overview"), AllowAnonymous]
        public async Task<IActionResult> Overview()
            => Ok(new { success=true, data=await _svc.GetOverviewAsync() });

        [HttpPost("waitlist"), Authorize]
        public async Task<IActionResult> JoinWaitlist([FromBody] JoinWaitlistDto dto)
        {
            var ok = await _svc.JoinWaitlistAsync(UserId, dto);
            return Ok(new { success=ok, message=ok?"You're on the waitlist! We'll notify you on launch.":"Already on waitlist for this city." });
        }

        [HttpPost("cities/{id}/status"), Authorize(Roles="Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] dynamic req)
        {
            var ok = await _svc.UpdateCityStatusAsync(id, (string)(req.status??""), (string?)(req.notes));
            return Ok(new { success=ok });
        }

        [HttpPost("seed"), Authorize(Roles="Admin")]
        public async Task<IActionResult> Seed() { await _svc.SeedAsync(); return Ok(new { success=true }); }
    }
}
