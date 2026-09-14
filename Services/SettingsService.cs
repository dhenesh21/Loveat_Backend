using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M20: app-wide key/value settings, editable from AdminWeb, read by both mobile apps at startup.</summary>
    public class SettingsService
    {
        private readonly AppDbContext _db;
        public SettingsService(AppDbContext db) => _db = db;

        public async Task<List<AppSettingDto>> GetAllAsync(string? category = null)
        {
            var query = _db.AppSettingsEntries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(s => s.Category == category);

            return await query.OrderBy(s => s.Category).ThenBy(s => s.Key)
                .Select(s => new AppSettingDto { Id = s.Id, Key = s.Key, Value = s.Value, Description = s.Description, Category = s.Category, UpdatedAt = s.UpdatedAt })
                .ToListAsync();
        }

        public async Task<string?> GetValueAsync(string key)
            => (await _db.AppSettingsEntries.FirstOrDefaultAsync(s => s.Key == key))?.Value;

        public async Task<(bool Success, string Message)> UpsertAsync(int adminUserId, UpsertAppSettingRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Key)) return (false, "Key is required.");

            var setting = await _db.AppSettingsEntries.FirstOrDefaultAsync(s => s.Key == req.Key);
            if (setting == null)
            {
                setting = new AppSettings { Key = req.Key };
                _db.AppSettingsEntries.Add(setting);
            }

            setting.Value = req.Value;
            setting.Description = req.Description ?? setting.Description;
            setting.Category = req.Category;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByAdminId = adminUserId;

            await _db.SaveChangesAsync();
            return (true, "Setting saved.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string key)
        {
            var setting = await _db.AppSettingsEntries.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null) return (false, "Setting not found.");

            _db.AppSettingsEntries.Remove(setting);
            await _db.SaveChangesAsync();
            return (true, "Setting deleted.");
        }
    }
}
