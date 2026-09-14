namespace LovEat.API.DTOs
{
    // ── M91: Feature Flag Management ───────────────────────────────
    public class CreateFeatureFlagRequestDto
    {
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string TargetPlatform { get; set; } = "All";
        public string TargetAudience { get; set; } = "All";
        public string? TargetCity { get; set; }
        public string Environment { get; set; } = "Production";
    }

    public class UpdateFeatureFlagRequestDto
    {
        public int FlagId { get; set; }
        public bool? IsEnabled { get; set; }
        public int? RolloutPercentage { get; set; }
        public string? TargetPlatform { get; set; }
        public string? TargetAudience { get; set; }
        public string? TargetCity { get; set; }
        public string? Notes { get; set; }
    }

    public class FeatureFlagDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public int RolloutPercentage { get; set; }
        public string TargetPlatform { get; set; } = "";
        public string TargetAudience { get; set; } = "";
        public string? TargetCity { get; set; }
        public string Environment { get; set; } = "";
        public string? CreatedByName { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FeatureFlagAuditEntryDto
    {
        public string Action { get; set; } = "";
        public string? ChangedByName { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class EvaluateFlagRequestDto
    {
        public string Key { get; set; } = "";
        public int UserId { get; set; }
        public string? Platform { get; set; }
        public string? Audience { get; set; }
        public string? City { get; set; }
    }

    public class EvaluateFlagResultDto
    {
        public string Key { get; set; } = "";
        public bool IsActive { get; set; }
        public string Reason { get; set; } = "";
    }

    // ── M92: Announcement Management ───────────────────────────────
    public class CreateAnnouncementRequestDto
    {
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string AnnouncementType { get; set; } = "Info";
        public string TargetAudience { get; set; } = "All";
        public string? TargetCity { get; set; }
        public int Priority { get; set; } = 100;
        public bool IsDismissible { get; set; } = true;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }

    public class UpdateAnnouncementStatusRequestDto
    {
        public int AnnouncementId { get; set; }
        public string Status { get; set; } = ""; // Draft / Scheduled / Live / Ended
    }

    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string AnnouncementType { get; set; } = "";
        public string TargetAudience { get; set; } = "";
        public string? TargetCity { get; set; }
        public int Priority { get; set; }
        public bool IsDismissible { get; set; }
        public string Status { get; set; } = "";
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? CreatedByName { get; set; }
        public int ReadCount { get; set; }
        public int DismissCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ActiveAnnouncementRequestDto
    {
        public int UserId { get; set; }
        public string Audience { get; set; } = "All";
        public string? City { get; set; }
    }

    public class MarkAnnouncementRequestDto
    {
        public int AnnouncementId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = "Read"; // Read / Dismiss
    }
}
