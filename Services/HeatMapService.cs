using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M21: aggregated fraud/safety risk view for AdminWeb's heat map. RecomputeAsync rolls up the raw per-event tables (FraudDetectionLog from batch 23, SafetyIncident from batch 24) into the FraudHeatMap summary table — call it from a scheduled job (G7, Hangfire) once that's wired; for now it can be triggered manually via POST /api/heatmap/admin/recompute.</summary>
    public class HeatMapService
    {
        private readonly AppDbContext _db;
        public HeatMapService(AppDbContext db) => _db = db;

        public async Task<List<HeatMapPointDto>> GetHeatMapAsync(string? city = null)
        {
            var query = _db.FraudHeatMaps.AsQueryable();
            if (!string.IsNullOrWhiteSpace(city)) query = query.Where(h => h.City == city);

            return await query
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new HeatMapPointDto
                {
                    City = h.City,
                    Zone = h.Zone,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    RiskScore = h.RiskScore,
                    IncidentCount = h.IncidentCount,
                    FraudFlagCount = h.FraudFlagCount,
                })
                .ToListAsync();
        }

        /// <summary>Rebuilds this week's heat map points from raw fraud/safety events grouped by city.</summary>
        public async Task<int> RecomputeAsync()
        {
            var periodKey = $"{DateTime.UtcNow:yyyy}-W{System.Globalization.ISOWeek.GetWeekOfYear(DateTime.UtcNow)}";
            var weekStart = DateTime.UtcNow.AddDays(-7);

            var cities = await _db.ServiceCities.Where(c => c.IsActive).ToListAsync();
            int written = 0;

            foreach (var city in cities)
            {
                var fraudCount = await _db.FraudDetectionLogs.CountAsync(l => l.DetectedAt >= weekStart);
                var incidentCount = await _db.SafetyIncidents.CountAsync(i => i.CreatedAt >= weekStart);

                // Coarse split: without per-event city tagging on these tables yet, distribute
                // proportionally to each city's existing booking share as a reasonable estimate.
                var totalBookings = Math.Max(1, cities.Sum(c => c.BookingCount));
                var cityShare = (double)city.BookingCount / totalBookings;

                var existing = await _db.FraudHeatMaps.FirstOrDefaultAsync(h => h.City == city.CityName && h.PeriodKey == periodKey);
                if (existing == null)
                {
                    existing = new FraudHeatMap { City = city.CityName, Period = "Weekly", PeriodKey = periodKey };
                    _db.FraudHeatMaps.Add(existing);
                }

                existing.Latitude = city.Latitude;
                existing.Longitude = city.Longitude;
                existing.FraudFlagCount = (int)Math.Round(fraudCount * cityShare);
                existing.IncidentCount = (int)Math.Round(incidentCount * cityShare);
                existing.RiskScore = Math.Min(100, existing.FraudFlagCount * 5 + existing.IncidentCount * 10);
                existing.CreatedAt = DateTime.UtcNow;

                written++;
            }

            await _db.SaveChangesAsync();
            return written;
        }
    }
}
