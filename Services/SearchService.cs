using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;

namespace LovEat.API.Services
{
    /// <summary>M5/M6: chef search and discovery with distance + filter support.</summary>
    public class SearchService
    {
        private readonly AppDbContext _db;
        public SearchService(AppDbContext db) => _db = db;

        public async Task<List<ChefSearchResultDto>> SearchChefsAsync(SearchChefsRequestDto req)
        {
            // Coarse filter in the DB (active, non-null coordinates), then compute
            // exact distance in-memory since SQLite has no native geo functions.
            var query = _db.ChefProfiles
                .Include(p => p.User)
                .Where(p => p.User != null && p.User.IsActive && p.Latitude != null && p.Longitude != null);

            if (req.AvailableNow)
                query = query.Where(p => p.IsAvailable);

            if (req.MinRating.HasValue)
                query = query.Where(p => p.AverageRating >= req.MinRating.Value);

            if (req.MaxHourlyRate.HasValue)
                query = query.Where(p => p.HourlyRate <= req.MaxHourlyRate.Value);

            var candidates = await query.ToListAsync();

            var results = new List<ChefSearchResultDto>();
            foreach (var p in candidates)
            {
                var distance = LocationService.CalculateDistanceKm(
                    (double)req.Latitude, (double)req.Longitude,
                    (double)p.Latitude!.Value, (double)p.Longitude!.Value);

                if (distance > (double)req.RadiusKm) continue;

                var cuisines = SafeDeserialize(p.Cuisines);
                if (!string.IsNullOrWhiteSpace(req.Cuisine) &&
                    !cuisines.Any(c => c.Equals(req.Cuisine, StringComparison.OrdinalIgnoreCase)))
                    continue;

                results.Add(new ChefSearchResultDto
                {
                    ChefProfileId = p.Id,
                    UserId = p.UserId,
                    FullName = p.User!.FullName,
                    ProfileImageUrl = p.User.ProfileImageUrl,
                    Cuisines = cuisines,
                    HourlyRate = p.HourlyRate,
                    AverageRating = p.AverageRating,
                    TotalReviews = p.TotalReviews,
                    DistanceKm = Math.Round(distance, 1),
                    IsAvailable = p.IsAvailable,
                    IsVerified = p.IsVerified,
                    City = p.City,
                });
            }

            return results.OrderBy(r => r.DistanceKm).ToList();
        }

        /// <summary>M10: chefs who've opted in to "available right now" within range — used for emergency cook bookings.</summary>
        public async Task<List<ChefSearchResultDto>> SearchEmergencyChefsAsync(decimal latitude, decimal longitude, decimal radiusKm)
        {
            var now = DateTime.UtcNow;
            var emergencyChefIds = await _db.EmergencyAvailabilities
                .Where(e => e.IsAvailableNow && (e.AvailableUntil == null || e.AvailableUntil > now))
                .Select(e => e.ChefId)
                .ToListAsync();

            if (emergencyChefIds.Count == 0) return new List<ChefSearchResultDto>();

            var req = new SearchChefsRequestDto
            {
                Latitude = latitude,
                Longitude = longitude,
                RadiusKm = radiusKm,
                AvailableNow = false, // emergency list already filtered above, don't double-filter on general availability
            };

            var allNearby = await SearchChefsAsync(req);
            return allNearby.Where(c => emergencyChefIds.Contains(c.UserId)).ToList();
        }

        private static List<string> SafeDeserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
            catch { return new List<string>(); }
        }
    }
}
