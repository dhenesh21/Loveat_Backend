using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>
    /// Persists refresh tokens so sessions can actually be revoked — fixes a
    /// gap flagged since Phase 3, where AuthService issued a refresh token
    /// string but never stored it, making "logout" a no-op. The raw token is
    /// never stored, only its SHA-256 hash (same principle as password
    /// hashing: if this table leaked, no usable tokens would leak with it).
    /// </summary>
    public class RefreshToken
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(100)]
        public string TokenHash { get; set; } = "";

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Stores a device's push token so NotificationService.PushToDeviceAsync
    /// has something real to send to once G2 (a real FCM server key) is
    /// wired — fixes a gap where the push stub had no token to look up in
    /// the first place. A user can have more than one active device.
    /// </summary>
    public class DeviceToken
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(300)]
        public string Token { get; set; } = "";

        [MaxLength(10)]
        public string Platform { get; set; } = "Android"; // Android / iOS

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
