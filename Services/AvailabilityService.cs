using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M25 (general calendar/on-off toggle) + M10 (emergency "available right now").</summary>
    public class AvailabilityService
    {
        private readonly AppDbContext _db;
        public AvailabilityService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message)> SetGeneralAvailabilityAsync(int chefUserId, bool isAvailable)
        {
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefUserId);
            if (profile == null) return (false, "Chef profile not found. Complete your profile first.");

            profile.IsAvailable = isAvailable;
            profile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, isAvailable ? "You're now visible in search." : "You're now hidden from search.");
        }

        public async Task<(bool Success, string Message)> SetEmergencyAvailabilityAsync(int chefUserId, SetEmergencyAvailabilityRequestDto req)
        {
            var record = await _db.EmergencyAvailabilities.FirstOrDefaultAsync(e => e.ChefId == chefUserId);
            if (record == null)
            {
                record = new EmergencyAvailability { ChefId = chefUserId };
                _db.EmergencyAvailabilities.Add(record);
            }

            record.IsAvailableNow = req.IsAvailableNow;
            record.AvailableUntil = req.IsAvailableNow ? DateTime.UtcNow.AddMinutes(req.AvailableForMinutes) : null;
            if (req.Latitude.HasValue) record.CurrentLatitude = req.Latitude;
            if (req.Longitude.HasValue) record.CurrentLongitude = req.Longitude;
            record.ServiceRadiusKm = req.ServiceRadiusKm;
            record.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return (true, req.IsAvailableNow
                ? $"Marked available for emergency bookings for the next {req.AvailableForMinutes} minutes."
                : "Emergency availability turned off.");
        }

        public async Task<ChefAvailabilityStatusDto> GetStatusAsync(int chefUserId)
        {
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefUserId);
            var emergency = await _db.EmergencyAvailabilities.FirstOrDefaultAsync(e => e.ChefId == chefUserId);

            var emergencyStillActive = emergency != null && emergency.IsAvailableNow
                                        && (emergency.AvailableUntil == null || emergency.AvailableUntil > DateTime.UtcNow);

            return new ChefAvailabilityStatusDto
            {
                GeneralAvailability = profile?.IsAvailable ?? false,
                EmergencyAvailableNow = emergencyStillActive,
                EmergencyAvailableUntil = emergency?.AvailableUntil,
            };
        }
    }
}
