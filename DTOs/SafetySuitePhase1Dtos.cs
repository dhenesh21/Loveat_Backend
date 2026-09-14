namespace LovEat.API.DTOs
{
    // ── M179: Gender-Preference & Safety Filters ────────────────────
    public class SafetyPreferenceDto
    {
        public string? PreferredChefGender { get; set; }
        public bool AcceptOnlyFemaleVerifiedHouseholds { get; set; }
        public bool RequireSecondPersonPresent { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateSafetyPreferenceRequestDto
    {
        public string? PreferredChefGender { get; set; }
        public bool? AcceptOnlyFemaleVerifiedHouseholds { get; set; }
        public bool? RequireSecondPersonPresent { get; set; }
    }

    /// <summary>Result of BookingService checking a target chef's safety rules
    /// before allowing booking creation to proceed.</summary>
    public class SafetyEligibilityResult
    {
        public bool Allowed { get; set; } = true;
        public string? Reason { get; set; }
    }

    // ── M180: Trusted Contact Auto-Notification ─────────────────────
    public class TrustedContactAlertDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string ContactPhone { get; set; } = "";
        public string Channel { get; set; } = "";
        public string DeliveryStatus { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }

    // ── M181: Arrival Identity Verification ─────────────────────────
    public class ArrivalVerificationDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Status { get; set; } = "";
        public int FailedAttempts { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        // ArrivalOtp deliberately excluded — only ever returned to the customer via GetOtpForCustomerAsync
    }

    public class SubmitArrivalOtpRequestDto
    {
        public int BookingId { get; set; }
        public string Otp { get; set; } = "";
    }

    public class SkipArrivalVerificationRequestDto
    {
        public int BookingId { get; set; }
        public string? Reason { get; set; }
    }
}
