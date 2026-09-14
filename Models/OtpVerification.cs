using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>
    /// One-time-password records for login/registration (M2). OtpService
    /// (Phase 3) generates and validates against this table. Real SMS
    /// delivery via Twilio/MSG91 is a separate infra gap (G3) — this table
    /// works fine with any provider, or none (dev/test mode logs the OTP).
    /// </summary>
    public class OtpVerification
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(15)]
        public string Phone { get; set; } = "";

        [Required, MaxLength(6)]
        public string OtpCode { get; set; } = "";

        [MaxLength(20)]
        public string Purpose { get; set; } = "Login"; // Login / Register / ResetPassword / ChangePhone

        public int Attempts { get; set; } = 0;
        public bool IsUsed { get; set; } = false;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
