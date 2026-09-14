using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class BillSplitService
    {
        private readonly AppDbContext _db;
        public BillSplitService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, BillSplitDto? Split)> CreateAsync(CreateBillSplitRequestDto req)
        {
            var booking = await _db.Bookings.FindAsync(req.BookingId);
            if (booking == null) return (false, "Booking not found.", null);
            if (req.Participants.Count == 0) return (false, "At least one participant is required.", null);

            var split = new BillSplit { BookingId = req.BookingId, InitiatedByCustomerId = req.InitiatedByCustomerId, TotalAmount = booking.TotalAmount, SplitMethod = req.SplitMethod };
            _db.BillSplits.Add(split);
            await _db.SaveChangesAsync();

            var shareAmount = req.SplitMethod == "Equal" ? Math.Round(booking.TotalAmount / req.Participants.Count, 2) : 0;
            foreach (var p in req.Participants)
            {
                _db.BillSplitParticipants.Add(new BillSplitParticipant
                {
                    BillSplitId = split.Id, CustomerId = p.CustomerId,
                    ShareAmount = req.SplitMethod == "Equal" ? shareAmount : (p.ShareAmount ?? 0),
                });
            }
            await _db.SaveChangesAsync();

            return (true, "Bill split created.", await ToDtoAsync(split));
        }

        public async Task<(bool Success, string Message)> MarkPaidAsync(int participantId)
        {
            var participant = await _db.BillSplitParticipants.FindAsync(participantId);
            if (participant == null) return (false, "Participant not found.");
            participant.PaymentStatus = "Paid";
            participant.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var split = await _db.BillSplits.FindAsync(participant.BillSplitId);
            if (split != null)
            {
                var allParticipants = await _db.BillSplitParticipants.Where(p => p.BillSplitId == split.Id).ToListAsync();
                split.Status = allParticipants.All(p => p.PaymentStatus == "Paid") ? "Completed" : "PartiallyPaid";
                await _db.SaveChangesAsync();
            }
            return (true, "Marked paid.");
        }

        public async Task<BillSplitDto?> GetAsync(int billSplitId)
        {
            var split = await _db.BillSplits.FindAsync(billSplitId);
            return split == null ? null : await ToDtoAsync(split);
        }

        private async Task<BillSplitDto> ToDtoAsync(BillSplit s)
        {
            var participants = await _db.BillSplitParticipants.Include(p => p.Customer).Where(p => p.BillSplitId == s.Id).ToListAsync();
            return new BillSplitDto
            {
                Id = s.Id, BookingId = s.BookingId, TotalAmount = s.TotalAmount, SplitMethod = s.SplitMethod, Status = s.Status,
                Participants = participants.Select(p => new BillSplitParticipantDto { Id = p.Id, CustomerId = p.CustomerId, CustomerName = p.Customer?.FullName, ShareAmount = p.ShareAmount, PaymentStatus = p.PaymentStatus }).ToList(),
            };
        }
    }

    public class MealScheduleService
    {
        private readonly AppDbContext _db;
        public MealScheduleService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, MealScheduleDto? Schedule)> CreateAsync(CreateMealScheduleRequestDto req)
        {
            if (!TimeSpan.TryParse(req.PreferredTime, out var time)) return (false, "PreferredTime must be in HH:mm format.", null);
            var schedule = new MealSchedule { CustomerId = req.CustomerId, ChefId = req.ChefId, ScheduleName = req.ScheduleName, DayOfWeek = req.DayOfWeek, PreferredTime = time, EstimatedAmount = req.EstimatedAmount };
            _db.MealSchedules.Add(schedule);
            await _db.SaveChangesAsync();
            return (true, "Meal schedule created.", await ToDtoAsync(schedule));
        }

        public async Task<List<MealScheduleDto>> GetForCustomerAsync(int customerId)
        {
            var schedules = await _db.MealSchedules.Where(s => s.CustomerId == customerId).ToListAsync();
            var result = new List<MealScheduleDto>();
            foreach (var s in schedules) result.Add(await ToDtoAsync(s));
            return result;
        }

        public async Task<(bool Success, string Message)> ToggleAsync(ToggleMealScheduleRequestDto req)
        {
            var schedule = await _db.MealSchedules.FindAsync(req.ScheduleId);
            if (schedule == null) return (false, "Schedule not found.");
            schedule.IsActive = req.IsActive;
            await _db.SaveChangesAsync();
            return (true, req.IsActive ? "Schedule activated." : "Schedule paused.");
        }

        public async Task<GenerateDueBookingsResultDto> GenerateDueBookingsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var todayDow = (int)DateTime.UtcNow.DayOfWeek;
            var due = await _db.MealSchedules.Where(s => s.IsActive && s.DayOfWeek == todayDow && (s.LastGeneratedAt == null || s.LastGeneratedAt.Value.Date < today)).ToListAsync();

            var generated = 0;
            foreach (var schedule in due)
            {
                _db.Bookings.Add(new Booking
                {
                    CustomerId = schedule.CustomerId, ChefId = schedule.ChefId, TotalAmount = schedule.EstimatedAmount, BaseAmount = schedule.EstimatedAmount,
                    ScheduledAt = today.Add(schedule.PreferredTime), Status = "Pending",
                });
                schedule.LastGeneratedAt = DateTime.UtcNow;
                generated++;
            }
            if (generated > 0) await _db.SaveChangesAsync();
            return new GenerateDueBookingsResultDto { GeneratedCount = generated };
        }

        private async Task<MealScheduleDto> ToDtoAsync(MealSchedule s)
        {
            var chef = await _db.Users.FindAsync(s.ChefId);
            return new MealScheduleDto { Id = s.Id, ScheduleName = s.ScheduleName, ChefName = chef?.FullName, DayOfWeek = s.DayOfWeek, PreferredTime = s.PreferredTime.ToString(@"hh\:mm"), EstimatedAmount = s.EstimatedAmount, IsActive = s.IsActive, LastGeneratedAt = s.LastGeneratedAt };
        }
    }

    public class AccessibilityService
    {
        private readonly AppDbContext _db;
        public AccessibilityService(AppDbContext db) => _db = db;

        public async Task<AccessibilitySettingsDto> GetOrCreateAsync(int customerId)
        {
            var settings = await _db.AccessibilitySettings.FirstOrDefaultAsync(a => a.CustomerId == customerId);
            if (settings == null) { settings = new AccessibilitySettings { CustomerId = customerId }; _db.AccessibilitySettings.Add(settings); await _db.SaveChangesAsync(); }
            return ToDto(settings);
        }

        public async Task<AccessibilitySettingsDto> UpdateAsync(UpdateAccessibilitySettingsRequestDto req)
        {
            var settings = await _db.AccessibilitySettings.FirstOrDefaultAsync(a => a.CustomerId == req.CustomerId);
            if (settings == null) { settings = new AccessibilitySettings { CustomerId = req.CustomerId }; _db.AccessibilitySettings.Add(settings); }
            settings.FontSize = req.FontSize; settings.HighContrastMode = req.HighContrastMode; settings.ScreenReaderOptimized = req.ScreenReaderOptimized;
            settings.ReduceMotion = req.ReduceMotion; settings.VoiceGuidanceEnabled = req.VoiceGuidanceEnabled; settings.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(settings);
        }

        private static AccessibilitySettingsDto ToDto(AccessibilitySettings a) => new()
        {
            FontSize = a.FontSize, HighContrastMode = a.HighContrastMode, ScreenReaderOptimized = a.ScreenReaderOptimized,
            ReduceMotion = a.ReduceMotion, VoiceGuidanceEnabled = a.VoiceGuidanceEnabled,
        };
    }

    public class FaqService
    {
        private readonly AppDbContext _db;
        public FaqService(AppDbContext db) => _db = db;

        public async Task<List<FaqCategoryDto>> GetCategoriesAsync()
        {
            var categories = await _db.FaqCategories.OrderBy(c => c.SortOrder).ToListAsync();
            var result = new List<FaqCategoryDto>();
            foreach (var c in categories)
                result.Add(new FaqCategoryDto { Id = c.Id, Name = c.Name, IconName = c.IconName, ArticleCount = await _db.FaqArticles.CountAsync(a => a.FaqCategoryId == c.Id && a.IsPublished) });
            return result;
        }

        public async Task<List<FaqArticleDto>> GetByCategoryAsync(int categoryId)
            => (await _db.FaqArticles.Where(a => a.FaqCategoryId == categoryId && a.IsPublished).ToListAsync())
                .Select(ToDto).ToList();

        public async Task<List<FaqArticleDto>> SearchAsync(SearchFaqRequestDto req)
        {
            var q = _db.FaqArticles.Where(a => a.IsPublished && (a.Audience == "All" || a.Audience == req.Audience));
            if (!string.IsNullOrWhiteSpace(req.Query))
                q = q.Where(a => a.Question.Contains(req.Query) || a.Answer.Contains(req.Query));
            return (await q.OrderByDescending(a => a.ViewCount).Take(20).ToListAsync()).Select(ToDto).ToList();
        }

        public async Task<FaqArticleDto?> GetArticleAsync(int articleId)
        {
            var article = await _db.FaqArticles.FindAsync(articleId);
            if (article == null) return null;
            article.ViewCount++;
            await _db.SaveChangesAsync();
            return ToDto(article);
        }

        public async Task<FaqArticleDto> CreateArticleAsync(CreateFaqArticleRequestDto req)
        {
            var article = new FaqArticle { FaqCategoryId = req.FaqCategoryId, Question = req.Question, Answer = req.Answer, Audience = req.Audience };
            _db.FaqArticles.Add(article);
            await _db.SaveChangesAsync();
            return ToDto(article);
        }

        public async Task<(bool Success, string Message)> RateAsync(RateFaqArticleRequestDto req)
        {
            var article = await _db.FaqArticles.FindAsync(req.ArticleId);
            if (article == null) return (false, "Article not found.");
            if (req.WasHelpful) article.HelpfulCount++; else article.NotHelpfulCount++;
            await _db.SaveChangesAsync();
            return (true, "Thanks for the feedback.");
        }

        private static FaqArticleDto ToDto(FaqArticle a) => new()
        {
            Id = a.Id, Question = a.Question, Answer = a.Answer, ViewCount = a.ViewCount, HelpfulCount = a.HelpfulCount, NotHelpfulCount = a.NotHelpfulCount,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/bill-splits"), Authorize]
    public class BillSplitController : ControllerBase
    {
        private readonly Services.BillSplitService _svc;
        public BillSplitController(Services.BillSplitService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBillSplitRequestDto req)
        {
            var (success, message, split) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = split });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var split = await _svc.GetAsync(id);
            if (split == null) return NotFound(new { success = false, message = "Bill split not found." });
            return Ok(new { success = true, data = split });
        }

        [HttpPost("mark-paid")]
        public async Task<IActionResult> MarkPaid([FromBody] MarkSplitPaidRequestDto req)
        {
            var (success, message) = await _svc.MarkPaidAsync(req.ParticipantId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/meal-schedules"), Authorize]
    public class MealScheduleController : ControllerBase
    {
        private readonly Services.MealScheduleService _svc;
        public MealScheduleController(Services.MealScheduleService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMealScheduleRequestDto req)
        {
            var (success, message, schedule) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = schedule });
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(int customerId) => Ok(new { success = true, data = await _svc.GetForCustomerAsync(customerId) });

        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle([FromBody] ToggleMealScheduleRequestDto req)
        {
            var (success, message) = await _svc.ToggleAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("generate-due"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateDue() => Ok(new { success = true, data = await _svc.GenerateDueBookingsAsync() });
    }

    [ApiController, Route("api/accessibility"), Authorize]
    public class AccessibilityController : ControllerBase
    {
        private readonly Services.AccessibilityService _svc;
        public AccessibilityController(Services.AccessibilityService svc) => _svc = svc;

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(int customerId) => Ok(new { success = true, data = await _svc.GetOrCreateAsync(customerId) });

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateAccessibilitySettingsRequestDto req) => Ok(new { success = true, data = await _svc.UpdateAsync(req) });
    }

    [ApiController, Route("api/faq")]
    public class FaqController : ControllerBase
    {
        private readonly Services.FaqService _svc;
        public FaqController(Services.FaqService svc) => _svc = svc;

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories() => Ok(new { success = true, data = await _svc.GetCategoriesAsync() });

        [HttpGet("categories/{categoryId}/articles")]
        public async Task<IActionResult> GetByCategory(int categoryId) => Ok(new { success = true, data = await _svc.GetByCategoryAsync(categoryId) });

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchFaqRequestDto req) => Ok(new { success = true, data = await _svc.SearchAsync(req) });

        [HttpGet("articles/{id}")]
        public async Task<IActionResult> GetArticle(int id)
        {
            var article = await _svc.GetArticleAsync(id);
            if (article == null) return NotFound(new { success = false, message = "Article not found." });
            return Ok(new { success = true, data = article });
        }

        [HttpPost("articles"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateArticle([FromBody] CreateFaqArticleRequestDto req) => Ok(new { success = true, data = await _svc.CreateArticleAsync(req) });

        [HttpPost("articles/rate")]
        public async Task<IActionResult> Rate([FromBody] RateFaqArticleRequestDto req)
        {
            var (success, message) = await _svc.RateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
