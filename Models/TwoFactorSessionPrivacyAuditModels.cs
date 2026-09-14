using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    public class TwoFactorSetting
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [MaxLength(64)] public string? TotpSecretEncrypted { get; set; }
        public bool IsEnabled { get; set; } = false;
        public string? BackupCodesJson { get; set; }
        public DateTime? EnabledAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Note: M162 (Session Management) is already fully implemented in
    // DeviceSecurityCorporateModels.cs (UserSession, with DeviceId, hashed
    // Token, IsCurrent, LoginAt/LastSeenAt/LoggedOutAt) as part of the
    // earlier M58 module — nothing new needed here. See
    // DeviceSecurityCorporateService for session listing/revocation.

    public class DataPrivacyRequest
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [MaxLength(20)] public string RequestType { get; set; } = "Export";
        [MaxLength(20)] public string Status { get; set; } = "Pending";
        public string? ResultDataJson { get; set; }
        [MaxLength(300)] public string? RejectionReason { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    public class SecurityAuditEntry
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [MaxLength(60)] public string EventType { get; set; } = "";
        [MaxLength(45)] public string? IpAddress { get; set; }
        [MaxLength(200)] public string? Details { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
