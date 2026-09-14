using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M105: Call Center Dashboard ────────────────────────────────
    public class CallLog
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int? AgentAdminId { get; set; }
        public User? AgentAdmin { get; set; }
        [MaxLength(10)] public string Direction { get; set; } = "Inbound"; // Inbound / Outbound
        [MaxLength(20)] public string PhoneNumber { get; set; } = "";
        [MaxLength(20)] public string Status { get; set; } = "Answered"; // Answered / Missed / Ongoing
        public int DurationSeconds { get; set; } = 0;
        public int? LinkedTicketId { get; set; }
        public SupportTicket? LinkedTicket { get; set; }
        public string? Notes { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
    }

    // ── M106: Escalation Matrix ────────────────────────────────────
    // Tiered escalation path (L1 -> L2 -> L3...) with an hours-based trigger
    // per level. AutoEscalateOverdueAsync (in the service) sweeps open
    // tickets that have sat past their current level's threshold.
    public class EscalationLevel
    {
        [Key] public int Id { get; set; }
        public int Level { get; set; } = 1; // 1, 2, 3...
        [MaxLength(60)] public string Name { get; set; } = ""; // e.g. "L1 Agent", "L2 Supervisor", "L3 Manager"
        public int TriggerAfterHours { get; set; } = 24; // escalate to next level if unresolved this long at this level
        public int? NotifyAdminId { get; set; }
        public User? NotifyAdmin { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EscalationEvent
    {
        [Key] public int Id { get; set; }
        public int TicketId { get; set; }
        public SupportTicket? Ticket { get; set; }
        public int FromLevel { get; set; }
        public int ToLevel { get; set; }
        [MaxLength(200)] public string Reason { get; set; } = "";
        public bool WasAutomatic { get; set; } = false;
        public int? EscalatedByAdminId { get; set; }
        public User? EscalatedByAdmin { get; set; }
        public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;
    }

    // Tracks which level a given ticket currently sits at (separate from
    // SupportTicket itself, which stays untouched).
    public class TicketEscalationState
    {
        [Key] public int Id { get; set; }
        public int TicketId { get; set; }
        public SupportTicket? Ticket { get; set; }
        public int CurrentLevel { get; set; } = 1;
        public DateTime EnteredLevelAt { get; set; } = DateTime.UtcNow;
    }

    // ── M107: SLA Management ───────────────────────────────────────
    public class SlaPolicy
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string Category { get; set; } = "General"; // matches SupportTicket.Category, or "All"
        [MaxLength(20)] public string Priority { get; set; } = "Medium"; // matches SupportTicket.Priority, or "All"
        public int ResponseTimeMinutes { get; set; } = 60;
        public int ResolutionTimeHours { get; set; } = 24;
        public bool IsActive { get; set; } = true;
    }

    public class SlaTracker
    {
        [Key] public int Id { get; set; }
        public int TicketId { get; set; }
        public SupportTicket? Ticket { get; set; }
        public int SlaPolicyId { get; set; }
        public SlaPolicy? SlaPolicy { get; set; }
        public DateTime FirstResponseDueAt { get; set; }
        public DateTime ResolutionDueAt { get; set; }
        public DateTime? FirstRespondedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public bool IsResponseBreached { get; set; } = false;
        public bool IsResolutionBreached { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M108: Complaint Tracking ───────────────────────────────────
    // Distinct from a general SupportTicket: a formal complaint, often
    // against a specific chef/booking, that needs an investigation trail and
    // a documented resolution action.
    public class Complaint
    {
        [Key] public int Id { get; set; }
        [MaxLength(20)] public string ComplaintNumber { get; set; } = "";
        public int ComplainantUserId { get; set; }
        public User? Complainant { get; set; }
        public int? AgainstChefId { get; set; }
        public User? AgainstChef { get; set; }
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }
        [MaxLength(60)] public string Category { get; set; } = "Service Quality"; // Service Quality / Hygiene / Behavior / Fraud / Safety / Other
        [MaxLength(20)] public string Severity { get; set; } = "Medium"; // Low / Medium / High / Critical
        public string Description { get; set; } = "";
        [MaxLength(20)] public string Status { get; set; } = "Received"; // Received / UnderInvestigation / Resolved / Dismissed
        public string? ResolutionAction { get; set; }
        public int? AssignedAdminId { get; set; }
        public User? AssignedAdmin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }

    public class ComplaintUpdate
    {
        [Key] public int Id { get; set; }
        public int ComplaintId { get; set; }
        public Complaint? Complaint { get; set; }
        public string Note { get; set; } = "";
        public int AddedByAdminId { get; set; }
        public User? AddedByAdmin { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
