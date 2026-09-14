using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M3: customer-facing profile management + saved addresses.</summary>
    public class UserProfileService
    {
        private readonly AppDbContext _db;
        public UserProfileService(AppDbContext db) => _db = db;

        public async Task<ProfileResponseDto?> GetProfileAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _db.UserProfiles.Add(profile);
                await _db.SaveChangesAsync();
            }

            return new ProfileResponseDto
            {
                User = new UserDto
                {
                    Id = user.Id,
                    Phone = user.PhoneNumber,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.FullName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    IsPhoneVerified = user.IsPhoneVerified,
                    WalletBalance = user.WalletBalance,
                    ProfileCompleted = profile.ProfileCompleted,
                },
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                Bio = profile.Bio,
                City = profile.City,
                DietaryPreference = profile.DietaryPreference,
                Allergies = profile.Allergies,
                EmergencyContactName = profile.EmergencyContactName,
                EmergencyContactPhone = profile.EmergencyContactPhone,
                ProfileCompleted = profile.ProfileCompleted,
                TrustedContactNotifyEnabled = profile.TrustedContactNotifyEnabled,
            };
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileRequestDto req)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return (false, "User not found.");

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
                          ?? new UserProfile { UserId = userId };
            if (profile.Id == 0) _db.UserProfiles.Add(profile);

            if (!string.IsNullOrWhiteSpace(req.FullName)) user.FullName = req.FullName;
            if (!string.IsNullOrWhiteSpace(req.Email)) user.Email = req.Email;
            if (!string.IsNullOrWhiteSpace(req.ProfileImageUrl)) user.ProfileImageUrl = req.ProfileImageUrl;

            profile.DateOfBirth = req.DateOfBirth ?? profile.DateOfBirth;
            profile.Gender = req.Gender ?? profile.Gender;
            profile.Bio = req.Bio ?? profile.Bio;
            profile.City = req.City ?? profile.City;
            profile.DietaryPreference = req.DietaryPreference ?? profile.DietaryPreference;
            profile.Allergies = req.Allergies ?? profile.Allergies;
            profile.EmergencyContactName = req.EmergencyContactName ?? profile.EmergencyContactName;
            profile.EmergencyContactPhone = req.EmergencyContactPhone ?? profile.EmergencyContactPhone;
            profile.TrustedContactNotifyEnabled = req.TrustedContactNotifyEnabled ?? profile.TrustedContactNotifyEnabled;
            profile.UpdatedAt = DateTime.UtcNow;

            // Consider the profile "complete" once the essentials are filled in —
            // used by the mobile app to decide whether to show ProfileSetupScreen again.
            profile.ProfileCompleted = !string.IsNullOrWhiteSpace(user.FullName)
                                        && !string.IsNullOrWhiteSpace(profile.City)
                                        && profile.DateOfBirth != null;

            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Profile updated.");
        }

        public async Task<List<LocationDto>> GetLocationsAsync(int userId)
        {
            return await _db.UserLocations
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.IsDefault)
                .ThenByDescending(l => l.UpdatedAt)
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    Label = l.Label,
                    Address = l.Address,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    City = l.City,
                    Pincode = l.Pincode,
                    IsDefault = l.IsDefault,
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, LocationDto? Data)> SaveLocationAsync(int userId, SaveLocationRequestDto req)
        {
            UserLocation location;

            if (req.Id.HasValue)
            {
                var existing = await _db.UserLocations.FirstOrDefaultAsync(l => l.Id == req.Id && l.UserId == userId);
                if (existing == null) return (false, "Location not found.", null);
                location = existing;
            }
            else
            {
                location = new UserLocation { UserId = userId };
                _db.UserLocations.Add(location);
            }

            location.Label = req.Label;
            location.Address = req.Address;
            location.Latitude = req.Latitude;
            location.Longitude = req.Longitude;
            location.City = req.City;
            location.Pincode = req.Pincode;
            location.UpdatedAt = DateTime.UtcNow;

            if (req.IsDefault)
            {
                var others = await _db.UserLocations.Where(l => l.UserId == userId && l.Id != location.Id).ToListAsync();
                foreach (var o in others) o.IsDefault = false;
                location.IsDefault = true;
            }

            await _db.SaveChangesAsync();

            return (true, "Location saved.", new LocationDto
            {
                Id = location.Id,
                Label = location.Label,
                Address = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                City = location.City,
                Pincode = location.Pincode,
                IsDefault = location.IsDefault,
            });
        }

        public async Task<(bool Success, string Message)> DeleteLocationAsync(int userId, int locationId)
        {
            var location = await _db.UserLocations.FirstOrDefaultAsync(l => l.Id == locationId && l.UserId == userId);
            if (location == null) return (false, "Location not found.");

            _db.UserLocations.Remove(location);
            await _db.SaveChangesAsync();
            return (true, "Location removed.");
        }
    }
}
