using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // Phase 18 — Safety Suite (Batch: SafetySuite Phase 1)
    // M179: Gender-Preference Matching & Safety Filters
    // M180: Trusted Contact Auto-Notification
    // M181: Arrival Identity Verification
    // ══════════════════════════════════════════════════════════════

    // ── M179: Gender-Preference Matching & Safety Filters ──────────
    /// <summary>
    /// One row per user (customer or chef). Customers set a chef-gender search
    /// preference; chefs set which households they're willing to accept.
    /// Enforcement: SearchService filters by PreferredChefGender (customer side);
    /// BookingService checks the target chef's AcceptOnlyFemaleVerifiedHouseholds /
    /// RequireSecondPersonPresent before allowing a booking to be created (chef side).
    /// </summary>
    public class SafetyPreference
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(20)]
        public string? PreferredChefGender { get; set; } // "Female" / "Male" / "NoPreference" (customer-side)

        public bool AcceptOnlyFemaleVerifiedHouseholds { get; set; } = false; // chef-side
        public bool RequireSecondPersonPresent { get; set; } = false;         // chef-side

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M180: Trusted Contact Auto-Notification ────────────────────
    /// <summary>
    /// Fired automatically when a booking moves to Accepted, if the user has
    /// TrustedContactNotifyEnabled on (see UserProfile addition below). Reuses
    /// the existing ISmsService (Msg91) — no new gateway integration needed.
    /// </summary>
    public class TrustedContactAlert
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }       // whose trusted contact gets notified
        public User? User { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        [MaxLength(15)]
        public string ContactPhone { get; set; } = ""; // snapshot of UserProfile.EmergencyContactPhone at send time

        [MaxLength(500)]
        public string Message { get; set; } = "";

        [MaxLength(20)]
        public string Channel { get; set; } = "SMS"; // SMS (WhatsApp reserved for later)

        [MaxLength(20)]
        public string DeliveryStatus { get; set; } = "Pending"; // Pending / Sent / Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }

    // ── M181: Arrival Identity Verification ─────────────────────────
    /// <summary>
    /// Generated once a booking is Accepted. Customer sees the OTP nowhere but
    /// their own app; chef must ask the customer for it in person and submit it
    /// to unlock the OnTheWay→Arrived transition on BookingTracking. Prevents a
    /// substitute/unverified person showing up in place of the verified chef.
    /// </summary>
    public class ArrivalVerification
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(6)]
        public string ArrivalOtp { get; set; } = "";

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending / Verified / Failed / Skipped

        public string? SelfieUrl { get; set; } // optional check-in selfie, manual review only — no biometric match claimed

        public int FailedAttempts { get; set; } = 0;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }
    }
}
