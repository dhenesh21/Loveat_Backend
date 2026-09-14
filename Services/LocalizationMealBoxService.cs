using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // M75: Localization Service
    // ══════════════════════════════════════════════════════════════
    public class LocalizationService
    {
        private readonly AppDbContext _db;
        public LocalizationService(AppDbContext db) => _db = db;

        public async Task<List<SupportedLanguageDto>> GetSupportedLanguagesAsync()
        {
            var list = await _db.SupportedLanguages.Where(l => l.IsActive).ToListAsync();
            return list.Select(l => new SupportedLanguageDto { Code = l.Code, Name = l.Name, NativeName = l.NativeName }).ToList();
        }

        /// <summary>Returns every translated string for a language as a flat key/value dictionary — the shape a mobile app's i18n layer typically wants to load once at startup.</summary>
        public async Task<Dictionary<string, string>> GetTranslationsAsync(string languageCode)
        {
            var strings = await _db.TranslationStrings.Where(t => t.LanguageCode == languageCode).ToListAsync();
            return strings.ToDictionary(t => t.Key, t => t.Value);
        }

        public async Task<(bool Success, string Message)> UpsertTranslationAsync(UpsertTranslationRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Key) || string.IsNullOrWhiteSpace(req.LanguageCode))
                return (false, "Key and language code are required.");

            var existing = await _db.TranslationStrings.FirstOrDefaultAsync(t => t.Key == req.Key && t.LanguageCode == req.LanguageCode);
            if (existing == null)
            {
                existing = new TranslationString { Key = req.Key, LanguageCode = req.LanguageCode };
                _db.TranslationStrings.Add(existing);
            }

            existing.Value = req.Value;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Translation saved.");
        }

        public async Task<(bool Success, string Message)> SetUserLanguageAsync(int userId, string code)
        {
            var lang = await _db.SupportedLanguages.FirstOrDefaultAsync(l => l.Code == code && l.IsActive);
            if (lang == null) return (false, $"Language '{code}' is not supported.");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return (false, "User not found.");

            user.PreferredLanguage = lang.Code;
            await _db.SaveChangesAsync();
            return (true, $"Language set to {lang.Name}.");
        }

        public async Task SeedAsync()
        {
            if (!await _db.SupportedLanguages.AnyAsync())
            {
                _db.SupportedLanguages.AddRange(
                    new SupportedLanguage { Code = "en", Name = "English", NativeName = "English" },
                    new SupportedLanguage { Code = "ta", Name = "Tamil", NativeName = "தமிழ்" },
                    new SupportedLanguage { Code = "hi", Name = "Hindi", NativeName = "हिन्दी" },
                    new SupportedLanguage { Code = "te", Name = "Telugu", NativeName = "తెలుగు" },
                    new SupportedLanguage { Code = "kn", Name = "Kannada", NativeName = "ಕನ್ನಡ" },
                    new SupportedLanguage { Code = "ml", Name = "Malayalam", NativeName = "മലയാളം" }
                );
            }

            if (!await _db.TranslationStrings.AnyAsync())
            {
                // A small representative starter set, not a full translation of every screen —
                // proves the mechanism end-to-end; filling out the rest is a content task, not a code task.
                var seed = new (string Key, string En, string Ta, string Hi)[]
                {
                    ("welcome_title", "Welcome to LovEat", "லவ்ஈட் க்கு வரவேற்கிறோம்", "लवईट में आपका स्वागत है"),
                    ("search_placeholder", "Search chefs by name", "பெயரால் சமையல்காரர்களைத் தேடுங்கள்", "नाम से शेफ खोजें"),
                    ("book_now", "Book Now", "இப்போது பதிவு செய்யுங்கள்", "अभी बुक करें"),
                    ("my_bookings", "My Bookings", "எனது பதிவுகள்", "मेरी बुकिंग"),
                    ("cancel_booking", "Cancel Booking", "பதிவை ரத்து செய்யவும்", "बुकिंग रद्द करें"),
                };

                foreach (var (key, en, ta, hi) in seed)
                {
                    _db.TranslationStrings.Add(new TranslationString { Key = key, LanguageCode = "en", Value = en });
                    _db.TranslationStrings.Add(new TranslationString { Key = key, LanguageCode = "ta", Value = ta });
                    _db.TranslationStrings.Add(new TranslationString { Key = key, LanguageCode = "hi", Value = hi });
                }
            }

            await _db.SaveChangesAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // M76: Meal Box Delivery Service
    // ══════════════════════════════════════════════════════════════
    public class MealBoxService
    {
        private readonly AppDbContext _db;
        private const int UpcomingDeliveriesToKeepScheduled = 4;

        public MealBoxService(AppDbContext db) => _db = db;

        public async Task<List<MealBoxPlanDto>> GetPlansAsync()
        {
            var plans = await _db.MealBoxPlans.Where(p => p.IsActive).ToListAsync();
            return plans.Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message, MealBoxSubscriptionDto? Data)> SubscribeAsync(int customerId, SubscribeMealBoxRequestDto req)
        {
            var plan = await _db.MealBoxPlans.FirstOrDefaultAsync(p => p.Id == req.PlanId && p.IsActive);
            if (plan == null) return (false, "Meal box plan not found.", null);

            var existing = await _db.MealBoxSubscriptions.FirstOrDefaultAsync(s => s.CustomerId == customerId && s.Status == "Active");
            if (existing != null) return (false, "You already have an active meal box subscription. Cancel it first to switch plans.", null);

            var firstDelivery = DateTime.UtcNow.Date.AddDays(3); // first box arrives a few days out, giving fulfillment time

            var sub = new MealBoxSubscription
            {
                CustomerId = customerId,
                PlanId = plan.Id,
                NextDeliveryDate = firstDelivery,
                DeliveryAddress = req.DeliveryAddress,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
            };
            _db.MealBoxSubscriptions.Add(sub);
            await _db.SaveChangesAsync();

            ScheduleUpcomingDeliveries(sub, plan, firstDelivery);
            await _db.SaveChangesAsync();

            return (true, "Subscribed! Your first box is on its way.", await ToSubDtoAsync(sub.Id));
        }

        public async Task<MealBoxSubscriptionDto?> GetMineAsync(int customerId)
        {
            var sub = await _db.MealBoxSubscriptions
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
            return sub == null ? null : await ToSubDtoAsync(sub.Id);
        }

        public async Task<List<MealBoxDeliveryDto>> GetUpcomingDeliveriesAsync(int customerId)
        {
            var sub = await _db.MealBoxSubscriptions.FirstOrDefaultAsync(s => s.CustomerId == customerId && s.Status == "Active");
            if (sub == null) return new List<MealBoxDeliveryDto>();

            var deliveries = await _db.MealBoxDeliveries
                .Where(d => d.SubscriptionId == sub.Id && d.Status != "Delivered" && d.Status != "Skipped")
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            return deliveries.Select(ToDeliveryDto).ToList();
        }

        public async Task<(bool Success, string Message)> PauseAsync(int customerId)
        {
            var sub = await _db.MealBoxSubscriptions.FirstOrDefaultAsync(s => s.CustomerId == customerId && s.Status == "Active");
            if (sub == null) return (false, "No active subscription found.");
            sub.Status = "Paused";
            await _db.SaveChangesAsync();
            return (true, "Deliveries paused.");
        }

        public async Task<(bool Success, string Message)> ResumeAsync(int customerId)
        {
            var sub = await _db.MealBoxSubscriptions.FirstOrDefaultAsync(s => s.CustomerId == customerId && s.Status == "Paused");
            if (sub == null) return (false, "No paused subscription found.");
            sub.Status = "Active";
            await _db.SaveChangesAsync();
            return (true, "Deliveries resumed.");
        }

        public async Task<(bool Success, string Message)> CancelAsync(int customerId)
        {
            var sub = await _db.MealBoxSubscriptions.FirstOrDefaultAsync(s => s.CustomerId == customerId && s.Status != "Cancelled");
            if (sub == null) return (false, "No subscription found.");

            sub.Status = "Cancelled";
            sub.CancelledAt = DateTime.UtcNow;

            var pending = await _db.MealBoxDeliveries.Where(d => d.SubscriptionId == sub.Id && d.Status == "Scheduled").ToListAsync();
            foreach (var d in pending) d.Status = "Skipped";

            await _db.SaveChangesAsync();
            return (true, "Subscription cancelled — remaining scheduled deliveries were skipped.");
        }

        /// <summary>Called by fulfillment (admin/ops) once a box actually goes out. Automatically schedules the next delivery to keep the rolling window full — this is the closest thing to "renewal" for a box subscription with no fixed end date.</summary>
        public async Task<(bool Success, string Message)> MarkDeliveredAsync(int deliveryId)
        {
            var delivery = await _db.MealBoxDeliveries.Include(d => d.Subscription).ThenInclude(s => s!.Plan).FirstOrDefaultAsync(d => d.Id == deliveryId);
            if (delivery == null) return (false, "Delivery not found.");

            delivery.Status = "Delivered";
            delivery.DeliveredAt = DateTime.UtcNow;

            var sub = delivery.Subscription;
            if (sub != null && sub.Status == "Active" && sub.Plan != null)
            {
                var lastScheduled = await _db.MealBoxDeliveries
                    .Where(d => d.SubscriptionId == sub.Id)
                    .OrderByDescending(d => d.ScheduledDate)
                    .Select(d => d.ScheduledDate)
                    .FirstOrDefaultAsync();

                var intervalDays = sub.Plan.Frequency == "BiWeekly" ? 14 : 7;
                var nextDate = lastScheduled.AddDays(intervalDays);

                _db.MealBoxDeliveries.Add(new MealBoxDelivery { SubscriptionId = sub.Id, ScheduledDate = nextDate });
                sub.NextDeliveryDate = nextDate;
            }

            await _db.SaveChangesAsync();
            return (true, "Delivery marked complete; next box scheduled.");
        }

        public async Task SeedPlansAsync()
        {
            if (await _db.MealBoxPlans.AnyAsync()) return;
            _db.MealBoxPlans.AddRange(
                new MealBoxPlan { Name = "Starter Box", MealsPerBox = 5, PricePerBox = 450, Frequency = "Weekly", CuisineType = "South Indian", Description = "5 home-style meals delivered weekly." },
                new MealBoxPlan { Name = "Family Box", MealsPerBox = 10, PricePerBox = 850, Frequency = "Weekly", CuisineType = "Mixed", Description = "10 meals — enough for a family of 2-3, weekly." },
                new MealBoxPlan { Name = "Fortnightly Saver", MealsPerBox = 8, PricePerBox = 700, Frequency = "BiWeekly", CuisineType = "North Indian", Description = "8 meals delivered every two weeks." }
            );
            await _db.SaveChangesAsync();
        }

        private void ScheduleUpcomingDeliveries(MealBoxSubscription sub, MealBoxPlan plan, DateTime firstDate)
        {
            var intervalDays = plan.Frequency == "BiWeekly" ? 14 : 7;
            var date = firstDate;
            for (int i = 0; i < UpcomingDeliveriesToKeepScheduled; i++)
            {
                _db.MealBoxDeliveries.Add(new MealBoxDelivery { SubscriptionId = sub.Id, ScheduledDate = date });
                date = date.AddDays(intervalDays);
            }
        }

        private async Task<MealBoxSubscriptionDto?> ToSubDtoAsync(int subId)
        {
            var sub = await _db.MealBoxSubscriptions.Include(s => s.Plan).FirstOrDefaultAsync(s => s.Id == subId);
            if (sub?.Plan == null) return null;

            return new MealBoxSubscriptionDto
            {
                Id = sub.Id,
                PlanName = sub.Plan.Name,
                MealsPerBox = sub.Plan.MealsPerBox,
                PricePerBox = sub.Plan.PricePerBox,
                Frequency = sub.Plan.Frequency,
                Status = sub.Status,
                NextDeliveryDate = sub.NextDeliveryDate,
                DeliveryAddress = sub.DeliveryAddress,
            };
        }

        private static MealBoxPlanDto ToDto(MealBoxPlan p) => new()
        {
            Id = p.Id, Name = p.Name, MealsPerBox = p.MealsPerBox, PricePerBox = p.PricePerBox,
            Frequency = p.Frequency, CuisineType = p.CuisineType, Description = p.Description,
        };

        private static MealBoxDeliveryDto ToDeliveryDto(MealBoxDelivery d) => new()
        {
            Id = d.Id, SubscriptionId = d.SubscriptionId, ScheduledDate = d.ScheduledDate,
            Status = d.Status, DeliveredAt = d.DeliveredAt,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/localization")]
    public class LocalizationController : ControllerBase
    {
        private readonly Services.LocalizationService _svc;
        public LocalizationController(Services.LocalizationService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("languages")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLanguages()
        {
            var data = await _svc.GetSupportedLanguagesAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("translations/{languageCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTranslations(string languageCode)
        {
            var data = await _svc.GetTranslationsAsync(languageCode);
            return Ok(new { success = true, data });
        }

        [HttpPut("preference")]
        [Authorize]
        public async Task<IActionResult> SetPreference([FromBody] SetPreferredLanguageRequestDto req)
        {
            var (success, message) = await _svc.SetUserLanguageAsync(UserId, req.Code);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("admin/translations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpsertTranslation([FromBody] UpsertTranslationRequestDto req)
        {
            var (success, message) = await _svc.UpsertTranslationAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("admin/seed")]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            await _svc.SeedAsync();
            return Ok(new { success = true });
        }
    }

    [ApiController]
    [Route("api/meal-box")]
    public class MealBoxController : ControllerBase
    {
        private readonly Services.MealBoxService _svc;
        public MealBoxController(Services.MealBoxService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlans()
        {
            var data = await _svc.GetPlansAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("subscribe")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeMealBoxRequestDto req)
        {
            var (success, message, data) = await _svc.SubscribeAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpGet("me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Mine()
        {
            var data = await _svc.GetMineAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpGet("upcoming")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Upcoming()
        {
            var data = await _svc.GetUpcomingDeliveriesAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("pause")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Pause()
        {
            var (success, message) = await _svc.PauseAsync(UserId);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("resume")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Resume()
        {
            var (success, message) = await _svc.ResumeAsync(UserId);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel()
        {
            var (success, message) = await _svc.CancelAsync(UserId);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("admin/deliveries/{deliveryId}/deliver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkDelivered(int deliveryId)
        {
            var (success, message) = await _svc.MarkDeliveredAsync(deliveryId);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("admin/seed")]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            await _svc.SeedPlansAsync();
            return Ok(new { success = true });
        }
    }
}
