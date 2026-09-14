using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>
    /// The central transactional entity — every module that touches money,
    /// tracking, disputes, or reviews (batches 17, 19, 21-23, 33) already
    /// keys off `Booking.Id` / `Booking.ChefId` / `Booking.Status`. Confirmed
    /// against existing code: CustomerRatingController checks
    /// `b.Id == dto.BookingId && b.ChefId == chefId && b.Status == "Completed"`.
    ///
    /// Covers M7 (instant), M8 (scheduled), M9 (event) directly via BookingType.
    /// M10 (emergency), M11 (team), M12 (corporate tiffin) extend this row —
    /// see EmergencyAvailability.cs and TiffinTeamBooking.cs.
    /// </summary>
    public class Booking
    {
        [Key] public int Id { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(20)]
        public string BookingType { get; set; } = "Instant"; // Instant / Scheduled / Event / Emergency / Team / CorporateTiffin

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending / Accepted / InProgress / Completed / Cancelled / Disputed

        [MaxLength(50)]
        public string? Cuisine { get; set; }

        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 120;
        public int GuestCount { get; set; } = 1;

        [MaxLength(300)]
        public string Address { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public decimal BaseAmount { get; set; }
        public decimal SurchargeAmount { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending / Paid / Refunded / Failed

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(300)]
        public string? CancellationReason { get; set; }

        public DateTime? AcceptedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ──
        public Review? Review { get; set; }
        public BookingTracking? Tracking { get; set; }
    }
}
