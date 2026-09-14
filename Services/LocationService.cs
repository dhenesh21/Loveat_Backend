using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// Geocoding + distance/service-area helpers used by Search and Booking.
    /// Real Google Maps geocoding — P1 hardening, replacing the previous
    /// fixed-coordinate stub in GeocodeAddressAsync.
    /// </summary>
    public class LocationService
    {
        private readonly AppDbContext _db;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<LocationService> _logger;

        public LocationService(AppDbContext db, IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<LocationService> logger)
        {
            _db = db;
            _http = httpClientFactory.CreateClient();
            _config = config;
            _logger = logger;
        }

        /// <summary>Haversine great-circle distance in kilometers. Pure math — safe to reuse anywhere two lat/lng pairs need comparing (SQLite has no native geo functions, so this runs in-memory after a coarse DB filter).</summary>
        public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180.0;

        // Real geocoding via Google Maps Geocoding API. Falls back to a
        // clearly-flagged stub result (not a silent fake success) when no
        // API key is configured, so callers built against IsStub can still
        // distinguish "we don't know" from "this is where it really is."
        public async Task<GeocodeResultDto> GeocodeAddressAsync(string address)
        {
            var apiKey = _config["ExternalServices:Maps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_VIA_ENV_VAR")
            {
                _logger.LogWarning("Geocoding skipped for '{Address}' — ExternalServices:Maps:ApiKey not configured.", address);
                return new GeocodeResultDto { Latitude = 11.0168m, Longitude = 76.9558m, FormattedAddress = address, IsStub = true };
            }

            try
            {
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";
                var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(body);

                var status = doc.RootElement.GetProperty("status").GetString();
                if (status != "OK")
                {
                    _logger.LogWarning("Google Geocoding API returned status {Status} for '{Address}'", status, address);
                    return new GeocodeResultDto { Latitude = 11.0168m, Longitude = 76.9558m, FormattedAddress = address, IsStub = true };
                }

                var result = doc.RootElement.GetProperty("results")[0];
                var location = result.GetProperty("geometry").GetProperty("location");
                return new GeocodeResultDto
                {
                    Latitude = location.GetProperty("lat").GetDecimal(),
                    Longitude = location.GetProperty("lng").GetDecimal(),
                    FormattedAddress = result.GetProperty("formatted_address").GetString() ?? address,
                    IsStub = false,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geocoding request threw an exception for '{Address}'", address);
                return new GeocodeResultDto { Latitude = 11.0168m, Longitude = 76.9558m, FormattedAddress = address, IsStub = true };
            }
        }

        /// <summary>Checks whether a lat/lng falls inside any active ServiceCity's radius (batch 27). Used before confirming a booking so we don't accept orders outside operating cities.</summary>
        public async Task<ServiceAreaCheckResultDto> CheckServiceAreaAsync(decimal latitude, decimal longitude)
        {
            var cities = await _db.ServiceCities.Where(c => c.IsActive).ToListAsync();

            ServiceCity? nearest = null;
            double nearestDistance = double.MaxValue;

            foreach (var city in cities)
            {
                var distance = CalculateDistanceKm((double)latitude, (double)longitude, (double)city.Latitude, (double)city.Longitude);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = city;
                }
            }

            if (nearest == null)
                return new ServiceAreaCheckResultDto { IsCovered = false, DistanceToNearestCityKm = -1 };

            return new ServiceAreaCheckResultDto
            {
                IsCovered = nearestDistance <= (double)nearest.RadiusKm,
                NearestCity = nearest.CityName,
                DistanceToNearestCityKm = Math.Round(nearestDistance, 1),
            };
        }

        /// <summary>Admin: chef density and booking volume by service city, for the location analytics dashboard.</summary>
        public async Task<List<AdminCityAnalyticsDto>> GetAdminCityAnalyticsAsync()
        {
            var cities = await _db.ServiceCities.OrderByDescending(c => c.BookingCount).ToListAsync();
            return cities.Select(c => new AdminCityAnalyticsDto
            {
                City = c.CityName,
                State = c.State,
                Chefs = c.ChefCount,
                Bookings = c.BookingCount,
                Active = c.IsActive,
                Launching = c.IsLaunching,
            }).ToList();
        }
    }
}
