using LovEat.API.Data;
using LovEat.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LovEat.API.Services
{
    /// <summary>
    /// M10: convenience flow for "get me a cook right now" — finds the
    /// nearest chef currently opted into emergency availability (via Phase
    /// 4's SearchService) and books them immediately (via Phase 4's
    /// BookingService) rather than making the customer search and book
    /// manually. Both underlying services already fully support this; this
    /// class just chains them for the one-tap emergency flow.
    /// </summary>
    public class EmergencyService
    {
        private readonly SearchService _search;
        private readonly BookingService _booking;
        private readonly AppDbContext _db;

        public EmergencyService(SearchService search, BookingService booking, AppDbContext db)
        {
            _search = search;
            _booking = booking;
            _db = db;
        }

        public async Task<(bool Success, string Message, BookingDto? Data)> BookNearestAvailableAsync(int customerId, BookEmergencyChefRequestDto req)
        {
            var candidates = await _search.SearchEmergencyChefsAsync(req.Latitude, req.Longitude, radiusKm: 20);
            if (candidates.Count == 0)
                return (false, "No chefs are available for an emergency booking near you right now. Please try a scheduled booking instead.", null);

            var nearest = candidates.First(); // already sorted by distance in SearchService

            var bookingReq = new CreateBookingRequestDto
            {
                ChefId = nearest.UserId,
                BookingType = "Emergency",
                Cuisine = req.Cuisine,
                ScheduledAt = DateTime.UtcNow,
                DurationMinutes = 120,
                GuestCount = req.GuestCount,
                Address = req.Address,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Notes = req.Notes,
            };

            var (success, message, data) = await _booking.CreateBookingAsync(customerId, bookingReq);
            return (success, success ? $"Booked {nearest.FullName} — nearest available chef ({nearest.DistanceKm}km away)." : message, data);
        }

        /// <summary>Admin: live view of in-flight emergency bookings + chefs currently opted into emergency availability</summary>
        public async Task<AdminEmergencyOverviewDto> GetAdminOverviewAsync()
        {
            var activeBookings = await _db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Chef)
                .Where(b => b.BookingType == "Emergency" && (b.Status == "Pending" || b.Status == "Accepted" || b.Status == "InProgress"))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var availableChefs = await _db.EmergencyAvailabilities
                .Include(e => e.Chef)
                .Where(e => e.IsAvailableNow && (e.AvailableUntil == null || e.AvailableUntil > now))
                .ToListAsync();

            var chefIds = availableChefs.Select(e => e.ChefId).ToList();
            var cities = await _db.ChefProfiles
                .Where(p => chefIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p.City);

            return new AdminEmergencyOverviewDto
            {
                ActiveRequests = activeBookings.Select(b => new AdminEmergencyRequestDto
                {
                    Id = b.Id,
                    Customer = b.Customer?.FullName ?? "Unknown",
                    Chef = b.Status == "Pending" ? null : b.Chef?.FullName,
                    RequestedAt = b.CreatedAt.ToString("MMM dd, HH:mm"),
                    Status = b.Status,
                    Address = b.Address,
                }).ToList(),
                AvailableChefs = availableChefs.Select(e => new AdminAvailableChefDto
                {
                    Id = e.ChefId,
                    Name = e.Chef?.FullName ?? "Unknown",
                    City = cities.TryGetValue(e.ChefId, out var city) ? city : null,
                    AvailableUntil = e.AvailableUntil.HasValue ? e.AvailableUntil.Value.ToString("MMM dd, HH:mm") : "No limit set",
                }).ToList(),
            };
        }
    }
}
