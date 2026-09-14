using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M101: Expense Management ───────────────────────────────────
    // Platform operating expenses — server costs, marketing spend, office,
    // salaries, etc. Distinct from chef payouts (M42 SettlementRequest) and
    // vendor/refund accounting (M95, M97-M99): this is money the platform
    // itself spends, not money owed to a chef or customer.
    public class Expense
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string Category { get; set; } = "General"; // Marketing / Infrastructure / Salaries / Office / Legal / Other
        [MaxLength(200)] public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        [MaxLength(60)] public string? Vendor { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        [MaxLength(20)] public string PaymentMethod { get; set; } = "BankTransfer"; // BankTransfer / Card / Cash / UPI
        [MaxLength(20)] public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected / Paid
        public string? ReceiptUrl { get; set; }
        public int SubmittedByAdminId { get; set; }
        public User? SubmittedByAdmin { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public User? ApprovedByAdmin { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M102: Profit & Loss Dashboard ──────────────────────────────
    // No new source-of-truth table needed — P&L is computed on demand from
    // M100 FinancialReportSnapshot (revenue side) + M101 Expense (cost side)
    // for a period. See PnLDto/ProfitLossService.
    // (Intentionally no model here — see DTOs/Services below.)

    // ── M103: Ticket System (queueing/routing layer) ───────────────
    // M31 SupportTicket/SupportMessage already provide the base help-desk
    // CRUD (subject, category, status, a single AssignedAdminId). This adds
    // what Support actually needs to run at volume: named queues, rule-based
    // auto-routing, and a status/assignment activity trail — without
    // touching the M31 tables.
    public class TicketQueue
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string Name { get; set; } = ""; // e.g. "Payments Queue", "Chef Onboarding Queue"
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class TicketAssignmentRule
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string Category { get; set; } = ""; // matches SupportTicket.Category
        public int TicketQueueId { get; set; }
        public TicketQueue? TicketQueue { get; set; }
        public int? DefaultAssigneeAdminId { get; set; } // optional direct-to-agent routing
        public User? DefaultAssigneeAdmin { get; set; }
        public int Priority { get; set; } = 100; // lower evaluates first when multiple rules could match
        public bool IsActive { get; set; } = true;
    }

    public class TicketActivityLog
    {
        [Key] public int Id { get; set; }
        public int TicketId { get; set; }
        public SupportTicket? Ticket { get; set; }
        [MaxLength(30)] public string Action { get; set; } = ""; // Routed / Reassigned / StatusChanged / QueueChanged
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }
        public int? ChangedByAdminId { get; set; }
        public User? ChangedByAdmin { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M104: Live Chat Support ─────────────────────────────────────
    // Real-time customer/chef <-> support-agent chat. Distinct from the M17
    // ChatThread/ChatMessage tables, which are booking-context chat between
    // a customer and a chef — this is a support session with an agent,
    // queued if nobody's free yet.
    public class LiveChatSession
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int? AgentAdminId { get; set; }
        public User? AgentAdmin { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Waiting"; // Waiting / Active / Closed
        [MaxLength(200)] public string? InitialMessage { get; set; }
        public int? Rating { get; set; } // 1-5, set on close
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    public class LiveChatMessage
    {
        [Key] public int Id { get; set; }
        public int LiveChatSessionId { get; set; }
        public LiveChatSession? LiveChatSession { get; set; }
        public int SenderId { get; set; }
        [MaxLength(10)] public string SenderRole { get; set; } = "User"; // User / Agent
        public string Message { get; set; } = "";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
