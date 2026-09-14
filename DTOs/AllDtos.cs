namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // Auth (M1) + OTP (M2)
    // ══════════════════════════════════════════════════════════════
    public class SendOtpRequestDto
    {
        public string Phone { get; set; } = "";
        public string Purpose { get; set; } = "Login"; // Login / Register / ResetPassword / ChangePhone
    }

    public class VerifyOtpRequestDto
    {
        public string Phone { get; set; } = "";
        public string Code { get; set; } = "";
        public string Purpose { get; set; } = "Login";

        /// <summary>Only used the first time a phone number logs in (account creation). Ignored for existing users.</summary>
        public string Role { get; set; } = "Customer"; // Customer / Chef
        public string? FullName { get; set; }
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool IsNewUser { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public string Role { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
        public bool IsPhoneVerified { get; set; }
        public decimal WalletBalance { get; set; }
        public bool ProfileCompleted { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // User profile (M3)
    // ══════════════════════════════════════════════════════════════
    public class UpdateProfileRequestDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? DietaryPreference { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? ProfileImageUrl { get; set; }
        // M180: lets the customer opt out of the automatic trusted-contact SMS
        // (TrustedContactAlertService.NotifyBothPartiesAsync checks this before sending).
        public bool? TrustedContactNotifyEnabled { get; set; }
    }

    public class ProfileResponseDto
    {
        public UserDto User { get; set; } = new();
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? DietaryPreference { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public bool ProfileCompleted { get; set; }
        public bool TrustedContactNotifyEnabled { get; set; }
    }

    public class SaveLocationRequestDto
    {
        public int? Id { get; set; } // present = update, absent = create
        public string Label { get; set; } = "Home";
        public string Address { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LocationDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public string Address { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public bool IsDefault { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // Chef profile (M4)
    // ══════════════════════════════════════════════════════════════
    public class UpdateChefProfileRequestDto
    {
        public string? Bio { get; set; }
        public List<string>? Cuisines { get; set; }
        public List<string>? Skills { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool? IsAvailable { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M20: App settings (admin-editable key/value config)
    // ══════════════════════════════════════════════════════════════
    public class ChefProfileDto
    {
        public int Id { get; set; }
        public UserDto User { get; set; } = new();
        public string? Bio { get; set; }
        public List<string> Cuisines { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public int ExperienceYears { get; set; }
        public decimal HourlyRate { get; set; }
        public string? City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalBookingsCompleted { get; set; }
    }

    public class UpsertAppSettingRequestDto
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Description { get; set; }
        public string Category { get; set; } = "General";
    }

    public class AppSettingDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Description { get; set; }
        public string Category { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }
}
