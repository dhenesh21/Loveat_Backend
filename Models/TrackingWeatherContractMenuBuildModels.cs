using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M157: Customer Experience — Delivery/Order ETA Tracking ─────
    public class OrderTrackingEvent
    {
        [Key] public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        [MaxLength(30)] public string Stage { get; set; } = "Confirmed"; // Confirmed / Preparing / OutForDelivery / Delivered / Cancelled
        public DateTime? EstimatedAt { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        [MaxLength(200)] public string? Notes { get; set; }
    }

    // ── M158: Customer Experience — Weather-Based Recommendations ───
    public class WeatherRecommendationRule
    {
        [Key] public int Id { get; set; }
        [MaxLength(30)] public string WeatherCondition { get; set; } = "Hot"; // Hot / Rainy / Cold / Humid
        [MaxLength(200)] public string SuggestedCuisines { get; set; } = ""; // comma-separated
        [MaxLength(300)] public string Message { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    // ── M159: Customer Experience — Digital Contracts/Agreements ────
    public class DigitalContract
    {
        [Key] public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public string TermsText { get; set; } = "";
        public bool CustomerAccepted { get; set; } = false;
        public DateTime? CustomerAcceptedAt { get; set; }
        [MaxLength(45)] public string? CustomerAcceptedIp { get; set; }
        public bool ChefAccepted { get; set; } = false;
        public DateTime? ChefAcceptedAt { get; set; }
        [MaxLength(45)] public string? ChefAcceptedIp { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Pending"; // Pending / FullyAccepted / Voided
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M160: Customer Experience — Interactive Menu Builder ────────
    public class CustomMenuBuild
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(80)] public string BuildName { get; set; } = "";
        public string SelectedItemsJson { get; set; } = "[]"; // [{ menuItemId, dishName, quantity }]
        public decimal EstimatedTotal { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Draft"; // Draft / SentToChef / Confirmed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
