using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>
    /// Extended customer-facing profile data (M3). One-to-one with User.
    /// Chef-specific fields live separately in ChefProfile.cs.
    /// </summary>
    public class UserProfile
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? DietaryPreference { get; set; } // Veg / NonVeg / Vegan / Jain / Eggetarian

        /// <summary>JSON array of allergy strings, e.g. ["peanuts","shellfish"]. Feeds M65 AI dietary matching.</summary>
        public string? Allergies { get; set; }

        [MaxLength(200)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(15)]
        public string? EmergencyContactPhone { get; set; }

        // M180: Trusted Contact Alert toggle — customer/chef can opt out of the automated
        // SMS notice sent to their emergency contact when a booking is accepted.
        public bool TrustedContactNotifyEnabled { get; set; } = true;

        public bool ProfileCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
