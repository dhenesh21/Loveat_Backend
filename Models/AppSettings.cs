using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>M20: app-wide key/value config editable from AdminWeb (AdminAppSettings.jsx), read by both mobile apps at startup. Keep values as strings and parse client-side/server-side as needed — avoids a migration every time a new setting is added.</summary>
    public class AppSettings
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = "";

        [MaxLength(2000)]
        public string Value { get; set; } = "";

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(30)]
        public string Category { get; set; } = "General"; // General / Payment / Booking / Notification / Feature Flags

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedByAdminId { get; set; }
    }
}
