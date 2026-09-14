namespace LovEat.API.DTOs
{
    public class CreateSystemIncidentRequestDto { public string Title { get; set; } = ""; public string Description { get; set; } = ""; public string Severity { get; set; } = "P3"; public int? OnCallAdminId { get; set; } }
    public class UpdateIncidentStatusRequestDto { public int IncidentId { get; set; } public string Status { get; set; } = ""; public string? PostmortemNotes { get; set; } }
    public class SystemIncidentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Status { get; set; } = "";
        public string? OnCallAdminName { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
    public class CreateOnCallScheduleRequestDto { public int AdminId { get; set; } public DateTime ShiftStart { get; set; } public DateTime ShiftEnd { get; set; } }
    public class OnCallScheduleDto { public int Id { get; set; } public string? AdminName { get; set; } public DateTime ShiftStart { get; set; } public DateTime ShiftEnd { get; set; } public bool IsActive { get; set; } }

    public class LogBackupRequestDto { public string BackupType { get; set; } = "Database"; public string Status { get; set; } = "Success"; public long? SizeBytes { get; set; } public string? StorageLocation { get; set; } public string? ErrorMessage { get; set; } }
    public class BackupRecordDto
    {
        public int Id { get; set; }
        public string BackupType { get; set; } = "";
        public string Status { get; set; } = "";
        public long? SizeBytes { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
    public class BackupHealthSummaryDto { public DateTime? LastSuccessfulBackupAt { get; set; } public int FailuresLast7Days { get; set; } public bool IsHealthy { get; set; } }

    public class CreateRunbookRequestDto { public string Title { get; set; } = ""; public string Category { get; set; } = ""; public string StepsMarkdown { get; set; } = ""; public int CreatedByAdminId { get; set; } }
    public class RunbookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string StepsMarkdown { get; set; } = "";
        public int UsageCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class MarkRunbookUsedRequestDto { public int RunbookId { get; set; } }

    public class CreateChangeRequestDto { public string Title { get; set; } = ""; public string Description { get; set; } = ""; public string RiskLevel { get; set; } = "Low"; public int RequestedByAdminId { get; set; } public DateTime? ScheduledAt { get; set; } }
    public class DecideChangeRequestDto { public int ChangeRequestId { get; set; } public bool Approve { get; set; } = true; public int ApprovedByAdminId { get; set; } }
    public class MarkChangeDeployedRequestDto { public int ChangeRequestId { get; set; } }
    public class ChangeRequestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string Status { get; set; } = "";
        public string? RequestedByName { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? DeployedAt { get; set; }
    }
}
