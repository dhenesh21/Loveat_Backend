using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>
    /// Customer → chef review after a completed booking (M16). Chef → customer
    /// rating (M37, "rate customer") is modeled separately below as
    /// CustomerRating since it's a different direction/shape and the existing
    /// CustomerRatingController (api/reviews/rate-customer) treats it that way.
    /// </summary>
    public class Review
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [MaxLength(500)]
        public string? ChefResponse { get; set; }

        public bool IsVerified { get; set; } = true; // true = linked to a real completed booking
        public bool IsFlagged { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ChefRespondedAt { get; set; }
    }

    /// <summary>Chef rating a customer after a completed booking (M37).</summary>
    public class CustomerRating
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
