using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LovEat.API.Controllers
{
    // ── M30: Quality Check ─────────────────────────────────────────
    [ApiController, Route("api/quality"), Authorize]
    public class QualityController : ControllerBase
    {
        private readonly AppDbContext _db;
        public QualityController(AppDbContext db) => _db = db;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CreateQualityCheckDto dto)
        {
            var qc = new QualityCheck
            {
                BookingId=dto.BookingId, CustomerId=UserId,
                OverallScore=dto.OverallScore, TasteScore=dto.TasteScore,
                HygieneScore=dto.HygieneScore, PresentationScore=dto.PresentationScore,
                PortionScore=dto.PortionScore, Comments=dto.Comments, WouldReorder=dto.WouldReorder,
            };
            _db.QualityChecks.Add(qc);
            await _db.SaveChangesAsync();
            return Ok(new { success=true, message="Quality check submitted", data=new QualityCheckDto { Id=qc.Id, BookingId=qc.BookingId, OverallScore=qc.OverallScore, TasteScore=qc.TasteScore, HygieneScore=qc.HygieneScore, PresentationScore=qc.PresentationScore, PortionScore=qc.PortionScore, Comments=qc.Comments, WouldReorder=qc.WouldReorder, CreatedAt=qc.CreatedAt.ToString("MMM dd, yyyy") } });
        }

        [HttpGet("admin/all"), Authorize(Roles="Admin")]
        public async Task<IActionResult> AdminAll()
        {
            var data = await _db.QualityChecks.Include(q=>q.Customer).OrderByDescending(q=>q.CreatedAt).Take(100).ToListAsync();
            return Ok(new { success=true, data=data.Select(q=>new QualityCheckDto { Id=q.Id, BookingId=q.BookingId, OverallScore=q.OverallScore, TasteScore=q.TasteScore, HygieneScore=q.HygieneScore, PresentationScore=q.PresentationScore, PortionScore=q.PortionScore, Comments=q.Comments, WouldReorder=q.WouldReorder, CreatedAt=q.CreatedAt.ToString("MMM dd, yyyy") }) });
        }
    }

    // ── M36: Safety ────────────────────────────────────────────────
    [ApiController, Route("api/safety"), Authorize]
    public class SafetyController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SafetyController(AppDbContext db) => _db = db;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("sos")]
        public async Task<IActionResult> SOS([FromBody] CreateSafetyAlertDto dto)
        {
            var alert = new SafetyAlert { UserId=UserId, AlertType=dto.AlertType, Latitude=dto.Latitude, Longitude=dto.Longitude, BookingId=dto.BookingId, Message=dto.Message };
            _db.SafetyAlerts.Add(alert);
            await _db.SaveChangesAsync();
            // In production: notify admin + emergency contacts
            return Ok(new { success=true, message="SOS alert sent to support team", data=new { alertId=alert.Id } });
        }

        [HttpPost("resolve/{id}"), Authorize(Roles="Admin")]
        public async Task<IActionResult> Resolve(int id)
        {
            var alert = await _db.SafetyAlerts.FindAsync(id);
            if (alert==null) return NotFound();
            alert.Status = "Resolved";
            alert.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { success=true });
        }

        [HttpGet("admin/alerts"), Authorize(Roles="Admin")]
        public async Task<IActionResult> AdminAlerts()
        {
            var data = await _db.SafetyAlerts.Include(a=>a.User).OrderByDescending(a=>a.CreatedAt).ToListAsync();
            return Ok(new { success=true, data });
        }
    }

    // ── M37: Grocery ───────────────────────────────────────────────
    [ApiController, Route("api/grocery"), Authorize]
    public class GroceryController : ControllerBase
    {
        private readonly AppDbContext _db;
        public GroceryController(AppDbContext db) => _db = db;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetList(int bookingId)
        {
            var list = await _db.GroceryLists.FirstOrDefaultAsync(g=>g.BookingId==bookingId);
            if (list==null) return Ok(new { success=false, message="No grocery list yet" });
            var items = JsonSerializer.Deserialize<List<GroceryItemDto>>(list.ItemsJson) ?? new();
            return Ok(new { success=true, data=new GroceryListDto { BookingId=bookingId, IsConfirmed=list.IsConfirmed, Items=items } });
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] dynamic req)
        {
            // In production: call AI service; here return smart demo items
            int bookingId = (int)(req.bookingId ?? 0);
            string cuisine = (string)(req.cuisine ?? "South Indian");
            var items = GenerateForCuisine(cuisine);
            var list = await _db.GroceryLists.FirstOrDefaultAsync(g=>g.BookingId==bookingId);
            if (list==null) { list=new GroceryList { BookingId=bookingId, ChefId=UserId }; _db.GroceryLists.Add(list); }
            list.ItemsJson = JsonSerializer.Serialize(items);
            await _db.SaveChangesAsync();
            return Ok(new { success=true, data=new GroceryListDto { BookingId=bookingId, Cuisine=cuisine, IsConfirmed=false, Items=items } });
        }

        [HttpPost("confirm/{bookingId}")]
        public async Task<IActionResult> Confirm(int bookingId)
        {
            var list = await _db.GroceryLists.FirstOrDefaultAsync(g=>g.BookingId==bookingId);
            if (list==null) return NotFound();
            list.IsConfirmed=true;
            await _db.SaveChangesAsync();
            return Ok(new { success=true });
        }

        private static List<GroceryItemDto> GenerateForCuisine(string cuisine) => cuisine switch
        {
            "Chettinad" => new() {
                new(){Name="Chicken",Quantity="1",Unit="kg",Category="Protein"},
                new(){Name="Kalpasi",Quantity="5",Unit="g",Category="Spice"},
                new(){Name="Marathi Mokku",Quantity="5",Unit="g",Category="Spice"},
                new(){Name="Coconut",Quantity="2",Unit="pieces",Category="Produce"},
                new(){Name="Shallots",Quantity="200",Unit="g",Category="Produce"},
                new(){Name="Tomatoes",Quantity="4",Unit="pieces",Category="Produce"},
                new(){Name="Red Chilli",Quantity="10",Unit="g",Category="Spice"},
            },
            "North Indian" => new() {
                new(){Name="Paneer",Quantity="500",Unit="g",Category="Dairy"},
                new(){Name="Onions",Quantity="500",Unit="g",Category="Produce"},
                new(){Name="Tomatoes",Quantity="500",Unit="g",Category="Produce"},
                new(){Name="Garam Masala",Quantity="20",Unit="g",Category="Spice"},
                new(){Name="Ghee",Quantity="100",Unit="ml",Category="Dairy"},
                new(){Name="Cashews",Quantity="50",Unit="g",Category="Nuts"},
            },
            _ => new() {
                new(){Name="Rice",Quantity="1",Unit="kg",Category="Grain"},
                new(){Name="Toor Dal",Quantity="200",Unit="g",Category="Lentil"},
                new(){Name="Tomatoes",Quantity="4",Unit="pieces",Category="Produce"},
                new(){Name="Tamarind",Quantity="50",Unit="g",Category="Condiment"},
                new(){Name="Curry Leaves",Quantity="2",Unit="sprigs",Category="Herbs"},
                new(){Name="Mustard Seeds",Quantity="10",Unit="g",Category="Spice"},
                new(){Name="Coconut Oil",Quantity="100",Unit="ml",Category="Oil"},
                new(){Name="Sambar Powder",Quantity="30",Unit="g",Category="Spice"},
            },
        };
    }

    // ── M38/M39: Meal Planning ─────────────────────────────────────
    [ApiController, Route("api/mealplan"), Authorize]
    public class MealPlanController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MealPlanController(AppDbContext db) => _db = db;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my")]
        public async Task<IActionResult> GetMy([FromQuery] string weekStart = "")
        {
            var plan = await _db.MealPlans.FirstOrDefaultAsync(p=>p.UserId==UserId && (weekStart==""||p.WeekStart==weekStart));
            if (plan==null) return Ok(new { success=false, data=(object?)null });
            return Ok(new { success=true, data=plan });
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] dynamic req)
        {
            string weekStart = (string)(req.weekStart ?? "");
            string entries   = JsonSerializer.Serialize(req.entries);
            var plan = await _db.MealPlans.FirstOrDefaultAsync(p=>p.UserId==UserId && p.WeekStart==weekStart);
            if (plan==null) { plan=new MealPlan { UserId=UserId, WeekStart=weekStart }; _db.MealPlans.Add(plan); }
            plan.EntriesJson = entries;
            plan.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { success=true, data=plan });
        }
    }

    // ── M40: Portfolio ─────────────────────────────────────────────
    [ApiController, Route("api/portfolio"), Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PortfolioController(AppDbContext db) => _db = db;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("chef/{chefProfileId}"), AllowAnonymous]
        public async Task<IActionResult> GetChef(int chefProfileId)
        {
            var items = await _db.ChefPortfolioItems.Where(p=>p.ChefProfileId==chefProfileId&&p.IsActive).OrderBy(p=>p.SortOrder).ToListAsync();
            return Ok(new { success=true, data=items.Select(p=>new ChefPortfolioItemDto { Id=p.Id, ImageUrl=p.ImageUrl, Caption=p.Caption, Cuisine=p.Cuisine, DishName=p.DishName, LikeCount=p.LikeCount, SortOrder=p.SortOrder }) });
        }

        [HttpGet("my"), Authorize(Roles="Chef")]
        public async Task<IActionResult> GetMy()
        {
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(c=>c.UserId==UserId);
            if (profile==null) return BadRequest();
            var items = await _db.ChefPortfolioItems.Where(p=>p.ChefProfileId==profile.Id).OrderBy(p=>p.SortOrder).ToListAsync();
            return Ok(new { success=true, data=items.Select(p=>new ChefPortfolioItemDto { Id=p.Id, ImageUrl=p.ImageUrl, Caption=p.Caption, Cuisine=p.Cuisine, DishName=p.DishName, LikeCount=p.LikeCount, SortOrder=p.SortOrder }) });
        }

        [HttpPost("add"), Authorize(Roles="Chef")]
        public async Task<IActionResult> Add([FromBody] AddPortfolioItemDto dto)
        {
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(c=>c.UserId==UserId);
            if (profile==null) return BadRequest();
            var item = new ChefPortfolioItem { ChefProfileId=profile.Id, ImageUrl=dto.ImageUrl, Caption=dto.Caption, Cuisine=dto.Cuisine, DishName=dto.DishName };
            _db.ChefPortfolioItems.Add(item);
            await _db.SaveChangesAsync();
            return Ok(new { success=true, data=new ChefPortfolioItemDto { Id=item.Id, ImageUrl=item.ImageUrl, Caption=item.Caption, Cuisine=item.Cuisine, DishName=item.DishName } });
        }

        [HttpDelete("{id}"), Authorize(Roles="Chef")]
        public async Task<IActionResult> Delete(int id)
        {
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(c=>c.UserId==UserId);
            var item = await _db.ChefPortfolioItems.FirstOrDefaultAsync(p=>p.Id==id&&p.ChefProfileId==profile!.Id);
            if (item==null) return NotFound();
            _db.ChefPortfolioItems.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { success=true });
        }

        [HttpPost("{id}/like"), AllowAnonymous]
        public async Task<IActionResult> Like(int id)
        {
            var item = await _db.ChefPortfolioItems.FindAsync(id);
            if (item==null) return NotFound();
            item.LikeCount++;
            await _db.SaveChangesAsync();
            return Ok(new { success=true, likeCount=item.LikeCount });
        }
    }
}
