namespace LovEat.API.DTOs
{
    // ── M101: Expense Management ───────────────────────────────────
    public class CreateExpenseRequestDto
    {
        public string Category { get; set; } = "General";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public string? Vendor { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; } = "BankTransfer";
        public string? ReceiptUrl { get; set; }
    }

    public class DecideExpenseRequestDto
    {
        public int ExpenseId { get; set; }
        public bool Approve { get; set; } = true;
    }

    public class MarkExpensePaidRequestDto { public int ExpenseId { get; set; } }

    public class ExpenseDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public string? Vendor { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";
        public string? ReceiptUrl { get; set; }
        public string? SubmittedByName { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── M102: Profit & Loss Dashboard ──────────────────────────────
    public class PnLRequestDto { public string PeriodKey { get; set; } = ""; } // yyyy-MM

    public class PnLExpenseBreakdownDto
    {
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class PnLDto
    {
        public string PeriodKey { get; set; } = "";
        public decimal GrossRevenue { get; set; }
        public decimal PlatformCommissionEarned { get; set; }
        public decimal TotalRefundsIssued { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; } // commission - refunds - expenses
        public List<PnLExpenseBreakdownDto> ExpenseBreakdown { get; set; } = new();
    }

    // ── M103: Ticket System (routing layer) ────────────────────────
    public class CreateTicketQueueRequestDto { public string Name { get; set; } = ""; public string? Description { get; set; } }

    public class TicketQueueDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int OpenTicketCount { get; set; }
    }

    public class CreateAssignmentRuleRequestDto
    {
        public string Category { get; set; } = "";
        public int TicketQueueId { get; set; }
        public int? DefaultAssigneeAdminId { get; set; }
        public int Priority { get; set; } = 100;
    }

    public class AssignmentRuleDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public int TicketQueueId { get; set; }
        public string? QueueName { get; set; }
        public int? DefaultAssigneeAdminId { get; set; }
        public string? DefaultAssigneeName { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
    }

    public class RouteTicketRequestDto { public int TicketId { get; set; } }

    public class ReassignTicketRequestDto
    {
        public int TicketId { get; set; }
        public int NewAssigneeAdminId { get; set; }
        public int ChangedByAdminId { get; set; }
    }

    public class TicketActivityDto
    {
        public string Action { get; set; } = "";
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }
        public string? ChangedByName { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    // ── M104: Live Chat Support ─────────────────────────────────────
    public class StartLiveChatRequestDto
    {
        public int UserId { get; set; }
        public string? InitialMessage { get; set; }
    }

    public class AssignLiveChatRequestDto
    {
        public int SessionId { get; set; }
        public int AgentAdminId { get; set; }
    }

    public class SendLiveChatMessageRequestDto
    {
        public int SessionId { get; set; }
        public int SenderId { get; set; }
        public string SenderRole { get; set; } = "User";
        public string Message { get; set; } = "";
    }

    public class CloseLiveChatRequestDto
    {
        public int SessionId { get; set; }
        public int? Rating { get; set; }
    }

    public class LiveChatSessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int? AgentAdminId { get; set; }
        public string? AgentName { get; set; }
        public string Status { get; set; } = "";
        public string? InitialMessage { get; set; }
        public int? Rating { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    public class LiveChatMessageDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int SenderId { get; set; }
        public string SenderRole { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime SentAt { get; set; }
    }
}
