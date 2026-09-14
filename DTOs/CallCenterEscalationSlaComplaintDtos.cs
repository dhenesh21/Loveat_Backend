namespace LovEat.API.DTOs
{
    // ── M105: Call Center Dashboard ────────────────────────────────
    public class LogCallRequestDto
    {
        public int UserId { get; set; }
        public int? AgentAdminId { get; set; }
        public string Direction { get; set; } = "Inbound";
        public string PhoneNumber { get; set; } = "";
        public string Status { get; set; } = "Answered";
        public int? LinkedTicketId { get; set; }
        public string? Notes { get; set; }
    }

    public class EndCallRequestDto
    {
        public int CallLogId { get; set; }
        public int DurationSeconds { get; set; }
        public string? Notes { get; set; }
    }

    public class CallLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int? AgentAdminId { get; set; }
        public string? AgentName { get; set; }
        public string Direction { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public int DurationSeconds { get; set; }
        public int? LinkedTicketId { get; set; }
        public string? Notes { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    public class CallCenterStatsDto
    {
        public int TotalCallsToday { get; set; }
        public int AnsweredToday { get; set; }
        public int MissedToday { get; set; }
        public double AvgHandleTimeSeconds { get; set; }
        public double MissedRatePercent { get; set; }
    }

    // ── M106: Escalation Matrix ────────────────────────────────────
    public class CreateEscalationLevelRequestDto
    {
        public int Level { get; set; }
        public string Name { get; set; } = "";
        public int TriggerAfterHours { get; set; } = 24;
        public int? NotifyAdminId { get; set; }
    }

    public class EscalationLevelDto
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Name { get; set; } = "";
        public int TriggerAfterHours { get; set; }
        public string? NotifyAdminName { get; set; }
        public bool IsActive { get; set; }
    }

    public class ManualEscalateRequestDto
    {
        public int TicketId { get; set; }
        public string Reason { get; set; } = "";
        public int EscalatedByAdminId { get; set; }
    }

    public class EscalationEventDto
    {
        public int TicketId { get; set; }
        public int FromLevel { get; set; }
        public int ToLevel { get; set; }
        public string Reason { get; set; } = "";
        public bool WasAutomatic { get; set; }
        public string? EscalatedByName { get; set; }
        public DateTime EscalatedAt { get; set; }
    }

    public class AutoEscalationSweepResultDto
    {
        public int ScannedCount { get; set; }
        public int EscalatedCount { get; set; }
        public List<EscalationEventDto> Events { get; set; } = new();
    }

    // ── M107: SLA Management ───────────────────────────────────────
    public class CreateSlaPolicyRequestDto
    {
        public string Category { get; set; } = "General";
        public string Priority { get; set; } = "Medium";
        public int ResponseTimeMinutes { get; set; } = 60;
        public int ResolutionTimeHours { get; set; } = 24;
    }

    public class SlaPolicyDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public string Priority { get; set; } = "";
        public int ResponseTimeMinutes { get; set; }
        public int ResolutionTimeHours { get; set; }
        public bool IsActive { get; set; }
    }

    public class AttachSlaRequestDto { public int TicketId { get; set; } }

    public class MarkFirstResponseRequestDto { public int TicketId { get; set; } }
    public class MarkResolvedRequestDto { public int TicketId { get; set; } }

    public class SlaTrackerDto
    {
        public int TicketId { get; set; }
        public string? PolicyCategory { get; set; }
        public DateTime FirstResponseDueAt { get; set; }
        public DateTime ResolutionDueAt { get; set; }
        public DateTime? FirstRespondedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public bool IsResponseBreached { get; set; }
        public bool IsResolutionBreached { get; set; }
    }

    public class SlaComplianceStatsDto
    {
        public int TotalTracked { get; set; }
        public int ResponseBreaches { get; set; }
        public int ResolutionBreaches { get; set; }
        public double ResponseComplianceRate { get; set; }
        public double ResolutionComplianceRate { get; set; }
    }

    // ── M108: Complaint Tracking ───────────────────────────────────
    public class FileComplaintRequestDto
    {
        public int ComplainantUserId { get; set; }
        public int? AgainstChefId { get; set; }
        public int? BookingId { get; set; }
        public string Category { get; set; } = "Service Quality";
        public string Severity { get; set; } = "Medium";
        public string Description { get; set; } = "";
    }

    public class UpdateComplaintStatusRequestDto
    {
        public int ComplaintId { get; set; }
        public string Status { get; set; } = "";
        public string? ResolutionAction { get; set; }
        public int? AssignedAdminId { get; set; }
    }

    public class AddComplaintNoteRequestDto
    {
        public int ComplaintId { get; set; }
        public string Note { get; set; } = "";
        public int AddedByAdminId { get; set; }
    }

    public class ComplaintDto
    {
        public int Id { get; set; }
        public string ComplaintNumber { get; set; } = "";
        public int ComplainantUserId { get; set; }
        public string? ComplainantName { get; set; }
        public int? AgainstChefId { get; set; }
        public string? AgainstChefName { get; set; }
        public int? BookingId { get; set; }
        public string Category { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public string? ResolutionAction { get; set; }
        public string? AssignedAdminName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<ComplaintUpdateDto> Updates { get; set; } = new();
    }

    public class ComplaintUpdateDto
    {
        public string Note { get; set; } = "";
        public string? AddedByName { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
