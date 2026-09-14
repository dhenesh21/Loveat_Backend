using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class WishlistService
    {
        private readonly AppDbContext _db;
        public WishlistService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, WishlistItemDto? Item)> AddAsync(AddWishlistItemRequestDto req)
        {
            if (await _db.WishlistItems.AnyAsync(w => w.CustomerId == req.CustomerId && w.ItemType == req.ItemType && w.ItemId == req.ItemId))
                return (false, "Already in your wishlist.", null);

            var item = new WishlistItem { CustomerId = req.CustomerId, ItemType = req.ItemType, ItemId = req.ItemId };
            _db.WishlistItems.Add(item);
            await _db.SaveChangesAsync();
            return (true, "Added to wishlist.", await ToDtoAsync(item));
        }

        public async Task<List<WishlistItemDto>> GetForCustomerAsync(int customerId)
        {
            var items = await _db.WishlistItems.Where(w => w.CustomerId == customerId).OrderByDescending(w => w.CreatedAt).ToListAsync();
            var result = new List<WishlistItemDto>();
            foreach (var i in items) result.Add(await ToDtoAsync(i));
            return result;
        }

        public async Task<(bool Success, string Message)> RemoveAsync(int wishlistItemId)
        {
            var item = await _db.WishlistItems.FindAsync(wishlistItemId);
            if (item == null) return (false, "Wishlist item not found.");
            _db.WishlistItems.Remove(item);
            await _db.SaveChangesAsync();
            return (true, "Removed from wishlist.");
        }

        private async Task<WishlistItemDto> ToDtoAsync(WishlistItem w)
        {
            string? displayName = null, imageUrl = null;
            if (w.ItemType == "Chef")
            {
                var chef = await _db.Users.FindAsync(w.ItemId);
                displayName = chef?.FullName;
                imageUrl = chef?.ProfileImageUrl;
            }
            else if (w.ItemType == "MenuItem")
            {
                var menuItem = await _db.ChefMenuItems.FindAsync(w.ItemId);
                displayName = menuItem?.DishName;
                imageUrl = menuItem?.ImageUrl;
            }
            return new WishlistItemDto { Id = w.Id, ItemType = w.ItemType, ItemId = w.ItemId, DisplayName = displayName, ImageUrl = imageUrl, CreatedAt = w.CreatedAt };
        }
    }

    public class GamificationService
    {
        private readonly AppDbContext _db;
        public GamificationService(AppDbContext db) => _db = db;

        public async Task<List<CustomerBadgeDto>> GetForCustomerAsync(int customerId)
        {
            var badges = await _db.CustomerBadges.Include(b => b.BadgeDefinition).Where(b => b.CustomerId == customerId).OrderByDescending(b => b.EarnedAt).ToListAsync();
            return badges.Select(b => new CustomerBadgeDto
            {
                Key = b.BadgeDefinition!.Key, Name = b.BadgeDefinition.Name, Description = b.BadgeDefinition.Description,
                IconName = b.BadgeDefinition.IconName, Tier = b.BadgeDefinition.Tier, EarnedAt = b.EarnedAt,
            }).ToList();
        }

        public async Task<BadgeAwardResultDto> CheckAndAwardAsync(int customerId)
        {
            var completedCount = await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status == "Completed");
            var distinctCuisines = await _db.Bookings.Where(b => b.CustomerId == customerId && b.Status == "Completed").Select(b => b.Cuisine).Distinct().CountAsync();
            var alreadyEarnedKeys = (await _db.CustomerBadges.Include(b => b.BadgeDefinition).Where(b => b.CustomerId == customerId).ToListAsync())
                .Select(b => b.BadgeDefinition!.Key).ToHashSet();

            var definitions = await _db.BadgeDefinitions.Where(d => d.IsActive).ToListAsync();
            var newlyAwarded = new List<CustomerBadgeDto>();

            foreach (var def in definitions)
            {
                if (alreadyEarnedKeys.Contains(def.Key)) continue;

                var qualifies = def.Key switch
                {
                    "first_order" => completedCount >= 1,
                    "five_orders" => completedCount >= 5,
                    "ten_orders" => completedCount >= 10,
                    "fifty_orders" => completedCount >= 50,
                    "three_cuisines" => distinctCuisines >= 3,
                    "five_cuisines" => distinctCuisines >= 5,
                    _ => false,
                };

                if (qualifies)
                {
                    _db.CustomerBadges.Add(new CustomerBadge { CustomerId = customerId, BadgeDefinitionId = def.Id });
                    newlyAwarded.Add(new CustomerBadgeDto { Key = def.Key, Name = def.Name, Description = def.Description, IconName = def.IconName, Tier = def.Tier, EarnedAt = DateTime.UtcNow });
                }
            }

            if (newlyAwarded.Count > 0) await _db.SaveChangesAsync();
            return new BadgeAwardResultDto { NewlyAwarded = newlyAwarded };
        }

        public async Task SeedDefaultBadgesAsync()
        {
            if (await _db.BadgeDefinitions.AnyAsync()) return;
            _db.BadgeDefinitions.AddRange(
                new BadgeDefinition { Key = "first_order", Name = "First Bite", Description = "Completed your first booking.", IconName = "restaurant", Tier = "Bronze" },
                new BadgeDefinition { Key = "five_orders", Name = "Regular", Description = "Completed 5 bookings.", IconName = "repeat", Tier = "Silver" },
                new BadgeDefinition { Key = "ten_orders", Name = "Loyal Foodie", Description = "Completed 10 bookings.", IconName = "heart", Tier = "Gold" },
                new BadgeDefinition { Key = "fifty_orders", Name = "LovEat Legend", Description = "Completed 50 bookings.", IconName = "trophy", Tier = "Platinum" },
                new BadgeDefinition { Key = "three_cuisines", Name = "Explorer", Description = "Tried 3 different cuisines.", IconName = "compass", Tier = "Bronze" },
                new BadgeDefinition { Key = "five_cuisines", Name = "World Traveler", Description = "Tried 5 different cuisines.", IconName = "globe", Tier = "Gold" }
            );
            await _db.SaveChangesAsync();
        }
    }

    public class CustomerPreferencesService
    {
        private readonly AppDbContext _db;
        public CustomerPreferencesService(AppDbContext db) => _db = db;

        public async Task<CustomerPreferencesDto> GetOrCreateAsync(int customerId)
        {
            var prefs = await _db.CustomerPreferences.FirstOrDefaultAsync(p => p.CustomerId == customerId);
            if (prefs == null)
            {
                prefs = new CustomerPreferences { CustomerId = customerId };
                _db.CustomerPreferences.Add(prefs);
                await _db.SaveChangesAsync();
            }
            return ToDto(prefs);
        }

        public async Task<CustomerPreferencesDto> UpdateAsync(UpdatePreferencesRequestDto req)
        {
            var prefs = await _db.CustomerPreferences.FirstOrDefaultAsync(p => p.CustomerId == req.CustomerId);
            if (prefs == null) { prefs = new CustomerPreferences { CustomerId = req.CustomerId }; _db.CustomerPreferences.Add(prefs); }

            prefs.PushNotificationsEnabled = req.PushNotificationsEnabled;
            prefs.EmailNotificationsEnabled = req.EmailNotificationsEnabled;
            prefs.SmsNotificationsEnabled = req.SmsNotificationsEnabled;
            prefs.PromoNotificationsEnabled = req.PromoNotificationsEnabled;
            prefs.Theme = req.Theme;
            prefs.Language = req.Language;
            prefs.DefaultPaymentMethod = req.DefaultPaymentMethod;
            prefs.ShowSpicyWarnings = req.ShowSpicyWarnings;
            prefs.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(prefs);
        }

        private static CustomerPreferencesDto ToDto(CustomerPreferences p) => new()
        {
            PushNotificationsEnabled = p.PushNotificationsEnabled, EmailNotificationsEnabled = p.EmailNotificationsEnabled,
            SmsNotificationsEnabled = p.SmsNotificationsEnabled, PromoNotificationsEnabled = p.PromoNotificationsEnabled,
            Theme = p.Theme, Language = p.Language, DefaultPaymentMethod = p.DefaultPaymentMethod,
            ShowSpicyWarnings = p.ShowSpicyWarnings, UpdatedAt = p.UpdatedAt,
        };
    }

    public class QuickReorderService
    {
        private readonly AppDbContext _db;
        public QuickReorderService(AppDbContext db) => _db = db;

        public async Task<QuickReorderComboDto> SaveAsync(SaveQuickReorderComboRequestDto req)
        {
            var combo = new QuickReorderCombo
            {
                CustomerId = req.CustomerId, ChefId = req.ChefId, SourceBookingId = req.SourceBookingId,
                ComboName = string.IsNullOrWhiteSpace(req.ComboName) ? "My combo" : req.ComboName,
                ItemsJson = JsonSerializer.Serialize(req.Items),
            };
            _db.QuickReorderCombos.Add(combo);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(combo);
        }

        public async Task<List<QuickReorderComboDto>> GetForCustomerAsync(int customerId)
        {
            var combos = await _db.QuickReorderCombos.Where(c => c.CustomerId == customerId).OrderByDescending(c => c.LastUsedAt ?? c.CreatedAt).ToListAsync();
            var result = new List<QuickReorderComboDto>();
            foreach (var c in combos) result.Add(await ToDtoAsync(c));
            return result;
        }

        public async Task<(bool Success, string Message)> MarkUsedAsync(int comboId)
        {
            var combo = await _db.QuickReorderCombos.FindAsync(comboId);
            if (combo == null) return (false, "Combo not found.");
            combo.UseCount += 1;
            combo.LastUsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Marked used.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int comboId)
        {
            var combo = await _db.QuickReorderCombos.FindAsync(comboId);
            if (combo == null) return (false, "Combo not found.");
            _db.QuickReorderCombos.Remove(combo);
            await _db.SaveChangesAsync();
            return (true, "Deleted.");
        }

        private async Task<QuickReorderComboDto> ToDtoAsync(QuickReorderCombo c)
        {
            var chef = await _db.Users.FindAsync(c.ChefId);
            var items = JsonSerializer.Deserialize<List<SaveComboItemRequestDto>>(c.ItemsJson) ?? new();
            return new QuickReorderComboDto
            {
                Id = c.Id, ChefId = c.ChefId, ChefName = chef?.FullName, ComboName = c.ComboName, Items = items,
                EstimatedTotal = items.Sum(i => i.Quantity * i.UnitPrice), UseCount = c.UseCount, LastUsedAt = c.LastUsedAt,
            };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/wishlist"), Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly Services.WishlistService _svc;
        public WishlistController(Services.WishlistService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddWishlistItemRequestDto req)
        {
            var (success, message, item) = await _svc.AddAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = item });
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(int customerId) => Ok(new { success = true, data = await _svc.GetForCustomerAsync(customerId) });

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] RemoveWishlistItemRequestDto req)
        {
            var (success, message) = await _svc.RemoveAsync(req.WishlistItemId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/gamification"), Authorize]
    public class GamificationController : ControllerBase
    {
        private readonly Services.GamificationService _svc;
        public GamificationController(Services.GamificationService svc) => _svc = svc;

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(int customerId) => Ok(new { success = true, data = await _svc.GetForCustomerAsync(customerId) });

        [HttpPost("check")]
        public async Task<IActionResult> CheckAndAward([FromBody] CheckAndAwardBadgesRequestDto req) => Ok(new { success = true, data = await _svc.CheckAndAwardAsync(req.CustomerId) });

        [HttpPost("seed"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Seed() { await _svc.SeedDefaultBadgesAsync(); return Ok(new { success = true, message = "Badge definitions seeded." }); }
    }

    [ApiController, Route("api/customer-preferences"), Authorize]
    public class CustomerPreferencesController : ControllerBase
    {
        private readonly Services.CustomerPreferencesService _svc;
        public CustomerPreferencesController(Services.CustomerPreferencesService svc) => _svc = svc;

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(int customerId) => Ok(new { success = true, data = await _svc.GetOrCreateAsync(customerId) });

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdatePreferencesRequestDto req) => Ok(new { success = true, data = await _svc.UpdateAsync(req) });
    }

    [ApiController, Route("api/quick-reorder"), Authorize]
    public class QuickReorderController : ControllerBase
    {
        private readonly Services.QuickReorderService _svc;
        public QuickReorderController(Services.QuickReorderService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveQuickReorderComboRequestDto req) => Ok(new { success = true, data = await _svc.SaveAsync(req) });

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(int customerId) => Ok(new { success = true, data = await _svc.GetForCustomerAsync(customerId) });

        [HttpPost("use")]
        public async Task<IActionResult> MarkUsed([FromBody] UseComboRequestDto req)
        {
            var (success, message) = await _svc.MarkUsedAsync(req.ComboId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteComboRequestDto req)
        {
            var (success, message) = await _svc.DeleteAsync(req.ComboId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
