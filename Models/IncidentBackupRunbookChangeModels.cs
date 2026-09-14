using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    public class SystemIncident
    {
        [Key] public int Id { get; set; }
        [MaxLength(120)] public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        [MaxLength(20)] public string Severity { get; set; } = "P3";
        [MaxLength(20)] public string Status { get; set; } = "Investigating";
        public int? OnCallAdminId { get; set; }
        public User? OnCallAdmin { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public string? PostmortemNotes { get; set; }
    }

    public class OnCallSchedule
    {
        [Key] public int Id { get; set; }
        public int AdminId { get; set; }
        public User? Admin { get; set; }
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BackupRecord
    {
        [Key] public int Id { get; set; }
        [MaxLength(20)] public string BackupType { get; set; } = "Database";
        [MaxLength(20)] public string Status { get; set; } = "Success";
        public long? SizeBytes { get; set; }
        [MaxLength(300)] public string? StorageLocation { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class Runbook
    {
        [Key] public int Id { get; set; }
        [MaxLength(120)] public string Title { get; set; } = "";
        [MaxLength(60)] public string Category { get; set; } = "";
        public string StepsMarkdown { get; set; } = "";
        public int CreatedByAdminId { get; set; }
        public User? CreatedByAdmin { get; set; }
        public int UsageCount { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChangeRequest
    {
        [Key] public int Id { get; set; }
        [MaxLength(120)] public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        [MaxLength(20)] public string RiskLevel { get; set; } = "Low";
        [MaxLength(20)] public string Status { get; set; } = "Pending";
        public int RequestedByAdminId { get; set; }
        public User? RequestedByAdmin { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public User? ApprovedByAdmin { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? DeployedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
