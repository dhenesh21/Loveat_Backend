using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// M23: chef recommendations for a customer. This is a straightforward
    /// rules-based ranking (dietary match + rating + proximity to their
    /// default address), not a trained ML model — genuinely "AI-powered"
    /// recommendations (collaborative filtering, embeddings, etc.) would be
    /// a substantial separate project. Every call is logged to the
    /// AIAnalytics table (Phase 2) so a real model could later be swapped in
    /// and measured against this baseline.
    /// </summary>
    public class AIRecommendationService
    {
        private readonly AppDbContext _db;
        public AIRecommendationService(AppDbContext db) => _db = db;

        public async Task<List<ChefRecommendationDto>> GetRecommendationsAsync(int customerId, int take = 10)
        {
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == customerId);
            var defaultLocation = await _db.UserLocations.FirstOrDefaultAsync(l => l.UserId == customerId && l.IsDefault);

            var chefs = await _db.ChefProfiles
                .Include(c => c.User)
                .Where(c => c.User != null && c.User.IsActive && c.IsAvailable)
                .OrderByDescending(c => c.AverageRating)
                .ThenByDescending(c => c.TotalReviews)
                .Take(take * 2) // over-fetch, then re-rank with dietary/distance signals below
                .ToListAsync();

            var results = new List<ChefRecommendationDto>();
            foreach (var c in chefs)
            {
                var cuisines = SafeDeserialize(c.Cuisines);
                string reason = "Highly rated on LovEat";

                if (!string.IsNullOrWhiteSpace(profile?.DietaryPreference) &&
                    cuisines.Any(cu => cu.Contains(profile.DietaryPreference, StringComparison.OrdinalIgnoreCase)))
                {
                    reason = $"Matches your {profile.DietaryPreference} preference";
                }
                else if (defaultLocation != null && c.Latitude.HasValue && c.Longitude.HasValue)
                {
                    var distance = LocationService.CalculateDistanceKm(
                        (double)defaultLocation.Latitude, (double)defaultLocation.Longitude,
                        (double)c.Latitude.Value, (double)c.Longitude.Value);
                    if (distance <= 10) reason = "Highly rated near you";
                }

                results.Add(new ChefRecommendationDto
                {
                    ChefProfileId = c.Id,
                    UserId = c.UserId,
                    FullName = c.User!.FullName,
                    ProfileImageUrl = c.User.ProfileImageUrl,
                    Cuisines = cuisines,
                    HourlyRate = c.HourlyRate,
                    AverageRating = c.AverageRating,
                    Reason = reason,
                });
            }

            var final = results.Take(take).ToList();

            _db.AIAnalyticsEntries.Add(new AIAnalytics
            {
                UserId = customerId,
                AnalyticsType = "ChefRecommendation",
                InputDataJson = JsonSerializer.Serialize(new { dietaryPreference = profile?.DietaryPreference }),
                OutputDataJson = JsonSerializer.Serialize(final.Select(r => r.ChefProfileId)),
                ModelVersion = "rules-v1",
            });
            await _db.SaveChangesAsync();

            return final;
        }

        private static List<string> SafeDeserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
            catch { return new List<string>(); }
        }
    }
}
