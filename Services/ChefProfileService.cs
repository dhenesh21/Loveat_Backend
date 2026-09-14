using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M4: chef-specific profile setup and public viewing.</summary>
    public class ChefProfileService
    {
        private readonly AppDbContext _db;
        public ChefProfileService(AppDbContext db) => _db = db;

        public async Task<ChefProfileDto?> GetMyProfileAsync(int chefUserId)
        {
            var profile = await _db.ChefProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == chefUserId);
            if (profile == null)
            {
                profile = new ChefProfile { UserId = chefUserId };
                _db.ChefProfiles.Add(profile);
                await _db.SaveChangesAsync();
                profile.User = await _db.Users.FindAsync(chefUserId);
            }

            return ToDto(profile);
        }

        /// <summary>Public view of a chef's profile, e.g. for CustomerApp search results — doesn't require the caller to own the profile.</summary>
        public async Task<ChefProfileDto?> GetPublicProfileAsync(int chefProfileId)
        {
            var profile = await _db.ChefProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == chefProfileId);
            return profile == null ? null : ToDto(profile);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int chefUserId, UpdateChefProfileRequestDto req)
        {
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefUserId);
            if (profile == null)
            {
                profile = new ChefProfile { UserId = chefUserId };
                _db.ChefProfiles.Add(profile);
            }

            if (req.Bio != null) profile.Bio = req.Bio;
            if (req.Cuisines != null) profile.Cuisines = JsonSerializer.Serialize(req.Cuisines);
            if (req.Skills != null) profile.Skills = JsonSerializer.Serialize(req.Skills);
            if (req.ExperienceYears.HasValue) profile.ExperienceYears = req.ExperienceYears.Value;
            if (req.HourlyRate.HasValue) profile.HourlyRate = req.HourlyRate.Value;
            if (req.City != null) profile.City = req.City;
            if (req.Latitude.HasValue) profile.Latitude = req.Latitude.Value;
            if (req.Longitude.HasValue) profile.Longitude = req.Longitude.Value;
            if (req.IsAvailable.HasValue) profile.IsAvailable = req.IsAvailable.Value;

            profile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Chef profile updated.");
        }

        /// <summary>Admin: every chef's profile, for the chef directory dashboard</summary>
        public async Task<List<ChefProfileDto>> GetAllForAdminAsync()
        {
            var profiles = await _db.ChefProfiles.Include(p => p.User).OrderByDescending(p => p.TotalBookingsCompleted).ToListAsync();
            return profiles.Select(ToDto).ToList();
        }

        private static ChefProfileDto ToDto(ChefProfile p) => new()
        {
            Id = p.Id,
            User = p.User == null ? new UserDto() : new UserDto
            {
                Id = p.User.Id,
                Phone = p.User.PhoneNumber,
                Email = p.User.Email,
                Role = p.User.Role,
                FullName = p.User.FullName,
                ProfileImageUrl = p.User.ProfileImageUrl,
                IsPhoneVerified = p.User.IsPhoneVerified,
                WalletBalance = p.User.WalletBalance,
            },
            Bio = p.Bio,
            Cuisines = SafeDeserialize(p.Cuisines),
            Skills = SafeDeserialize(p.Skills),
            ExperienceYears = p.ExperienceYears,
            HourlyRate = p.HourlyRate,
            City = p.City,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            IsVerified = p.IsVerified,
            IsAvailable = p.IsAvailable,
            AverageRating = p.AverageRating,
            TotalReviews = p.TotalReviews,
            TotalBookingsCompleted = p.TotalBookingsCompleted,
        };

        private static List<string> SafeDeserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
            catch { return new List<string>(); }
        }
    }
}
