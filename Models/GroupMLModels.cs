using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M69: Group bookings split across multiple chefs
    //
    // Distinct from M11's TeamBookingAssignment (Phase 6): that feature ties
    // several chefs to ONE Booking row working together under shared roles
    // (Lead/Cook/Assistant) — e.g. one wedding, one bill. This feature
    // instead splits a large guest count into SEPARATE, independent Booking
    // rows (one per chef, each fully priced and tracked through the normal
    // lifecycle via BookingService), for when no single chef has capacity
    // for the whole group. GroupBooking is the parent record tying those
    // independent bookings together for the customer's view.
    // ══════════════════════════════════════════════════════════════
    public class GroupBooking
    {
        [Key] public int Id { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        public int TotalGuestCount { get; set; }
        public string? Cuisine { get; set; }

        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 120;

        [MaxLength(300)]
        public string Address { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending / Confirmed / Cancelled
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>One chef's slice of a GroupBooking — points at the real Booking row that carries pricing/status/payment.</summary>
    public class GroupBookingSplit
    {
        [Key] public int Id { get; set; }

        public int GroupBookingId { get; set; }
        public GroupBooking? GroupBooking { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        public int GuestCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M70: "ML-based" chef-customer matching
    //
    // Honest framing: this is a weighted heuristic score (repeat-booking
    // history + customer's own past ratings of a chef + cuisine overlap +
    // the chef's overall rating), not a trained ML model — building and
    // validating an actual model (collaborative filtering, embeddings, etc.)
    // is a substantially larger project on its own. This table caches the
    // heuristic's output so repeated calls within RecomputeWindowHours don't
    // re-scan booking history every time; swapping in a real trained model
    // later only means changing MLMatchingService's scoring function, not
    // this table's shape.
    // ══════════════════════════════════════════════════════════════
    public class ChefMatchScore
    {
        [Key] public int Id { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        public decimal Score { get; set; } // 0-100

        /// <summary>JSON breakdown of what contributed to the score, e.g. {"repeatBookings":3,"cuisineMatch":true,"avgRatingGiven":4.5}</summary>
        public string? FactorsJson { get; set; }

        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }
}
