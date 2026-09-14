using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>M10: chefs opt in to "available right now" status for emergency/same-hour bookings. Checked by SearchService/AvailabilityService (Phase 4) when a customer requests an emergency cook.</summary>
    public class EmergencyAvailability
    {
        [Key] public int Id { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        public bool IsAvailableNow { get; set; } = false;
        public DateTime? AvailableUntil { get; set; }

        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public int ServiceRadiusKm { get; set; } = 10;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
