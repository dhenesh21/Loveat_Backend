using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>Saved addresses for a user (M-location capture). A booking still stores its own snapshot address on Booking.Address — this is the reusable "book again at Home/Work" list.</summary>
    public class UserLocation
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(20)]
        public string Label { get; set; } = "Home"; // Home / Work / Other

        [MaxLength(300)]
        public string Address { get; set; } = "";

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(10)]
        public string? Pincode { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
