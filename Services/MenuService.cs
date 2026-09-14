using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class MenuService
    {
        private readonly AppDbContext _db;
        public MenuService(AppDbContext db) => _db = db;

        // ── Get chef's full menu ───────────────────────────────────
        public async Task<ChefMenuDto?> GetChefMenuAsync(int chefProfileId)
        {
            var profile = await _db.ChefProfiles
                .Include(c => c.Skills)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == chefProfileId);
            if (profile == null) return null;

            var items = await _db.ChefMenuItems
                .Where(m => m.ChefProfileId == chefProfileId)
                .OrderBy(m => m.Cuisine).ThenBy(m => m.SortOrder)
                .ToListAsync();

            return new ChefMenuDto
            {
                ChefProfileId = chefProfileId,
                ChefName      = profile.User?.FullName ?? "",
                Cuisines      = items.Select(i => i.Cuisine).Distinct().ToList(),
                Items         = items.Select(MapItem).ToList(),
            };
        }

        // ── Get menu by chef user ID ───────────────────────────────
        public async Task<ChefMenuDto?> GetMenuByUserIdAsync(int userId)
        {
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            return profile == null ? null : await GetChefMenuAsync(profile.Id);
        }

        // ── Add menu item ──────────────────────────────────────────
        public async Task<ChefMenuItemDto> AddItemAsync(int chefProfileId, CreateMenuItemDto dto)
        {
            var item = new ChefMenuItem
            {
                ChefProfileId = chefProfileId,
                Cuisine       = dto.Cuisine,
                DishName      = dto.DishName,
                Description   = dto.Description,
                Price         = dto.Price,
                PriceUnit     = dto.PriceUnit,
                IsVeg         = dto.IsVeg,
                Tags          = dto.Tags,
            };
            _db.ChefMenuItems.Add(item);
            await _db.SaveChangesAsync();
            return MapItem(item);
        }

        // ── Toggle availability ────────────────────────────────────
        public async Task<bool> ToggleAvailabilityAsync(int itemId, int chefProfileId)
        {
            var item = await _db.ChefMenuItems.FirstOrDefaultAsync(m => m.Id == itemId && m.ChefProfileId == chefProfileId);
            if (item == null) return false;
            item.IsAvailable = !item.IsAvailable;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Delete menu item ───────────────────────────────────────
        public async Task<bool> DeleteItemAsync(int itemId, int chefProfileId)
        {
            var item = await _db.ChefMenuItems.FirstOrDefaultAsync(m => m.Id == itemId && m.ChefProfileId == chefProfileId);
            if (item == null) return false;
            _db.ChefMenuItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Admin: get all menus summary ───────────────────────────
        public async Task<List<ChefMenuDto>> GetAllMenusAsync()
        {
            var items = await _db.ChefMenuItems
                .Include(m => m.ChefProfile).ThenInclude(p => p!.User)
                .OrderBy(m => m.ChefProfileId)
                .ToListAsync();

            return items.GroupBy(m => m.ChefProfileId).Select(g => new ChefMenuDto
            {
                ChefProfileId = g.Key,
                ChefName      = g.First().ChefProfile?.User?.FullName ?? "Chef",
                Cuisines      = g.Select(m => m.Cuisine).Distinct().ToList(),
                Items         = g.Select(MapItem).ToList(),
            }).ToList();
        }

        private static ChefMenuItemDto MapItem(ChefMenuItem m) => new()
        {
            Id = m.Id, Cuisine = m.Cuisine, DishName = m.DishName,
            Description = m.Description, ImageUrl = m.ImageUrl,
            Price = m.Price, PriceUnit = m.PriceUnit, IsVeg = m.IsVeg,
            IsAvailable = m.IsAvailable, Tags = m.Tags,
            OrderCount = m.OrderCount, SortOrder = m.SortOrder,
        };
    }
}
