using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M91: Feature Flag Management ───────────────────────────────
    // Distinct from the M20 AppSettings "Feature Flags" category, which is a
    // flat on/off string toggle. This is a real flag engine: gradual
    // percentage rollout, platform/audience/city targeting, and a kill
    // switch — the kind of thing Super Admin needs to ship risky features
    // safely and turn them off instantly if something breaks.
    public class FeatureFlag
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(80)] public string Key { get; set; } = ""; // e.g. "new_checkout_flow"
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = false; // master kill switch
        public int RolloutPercentage { get; set; } = 100; // 0-100, applied only when IsEnabled
        [MaxLength(20)] public string TargetPlatform { get; set; } = "All"; // All / iOS / Android / Web
        [MaxLength(20)] public string TargetAudience { get; set; } = "All"; // All / Customer / Chef / Admin
        [MaxLength(60)] public string? TargetCity { get; set; }
        [MaxLength(20)] public string Environment { get; set; } = "Production"; // Production / Staging
        public int CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FeatureFlagAuditLog
    {
        [Key] public int Id { get; set; }
        public int FeatureFlagId { get; set; }
        public FeatureFlag? FeatureFlag { get; set; }
        [MaxLength(30)] public string Action { get; set; } = ""; // Created / Enabled / Disabled / RolloutChanged / TargetingChanged
        public int ChangedByUserId { get; set; }
        public User? ChangedBy { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M92: Announcement Management ───────────────────────────────
    // In-app announcements (maintenance notices, policy updates, festival
    // greetings) shown to a targeted audience for a time window, with
    // per-user read/dismiss tracking so Support can see reach.
    public class Announcement
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(150)] public string Title { get; set; } = "";
        [Required] public string Body { get; set; } = "";
        [MaxLength(20)] public string AnnouncementType { get; set; } = "Info"; // Info / Warning / Maintenance / Promo
        [MaxLength(20)] public string TargetAudience { get; set; } = "All"; // All / Customer / Chef
        [MaxLength(60)] public string? TargetCity { get; set; }
        public int Priority { get; set; } = 100; // lower shows first when multiple are live
        public bool IsDismissible { get; set; } = true;
        [MaxLength(20)] public string Status { get; set; } = "Draft"; // Draft / Scheduled / Live / Ended
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AnnouncementReceipt
    {
        [Key] public int Id { get; set; }
        public int AnnouncementId { get; set; }
        public Announcement? Announcement { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? DismissedAt { get; set; }
    }
}
