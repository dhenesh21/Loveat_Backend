using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M101: Expense Management ───────────────────────────────────
    public class ExpenseService
    {
        private readonly AppDbContext _db;
        public ExpenseService(AppDbContext db) => _db = db;

        public async Task<ExpenseDto> CreateAsync(int adminUserId, CreateExpenseRequestDto req)
        {
            var expense = new Expense
            {
                Category = req.Category, Description = req.Description, Amount = req.Amount, Vendor = req.Vendor,
                ExpenseDate = req.ExpenseDate, PaymentMethod = req.PaymentMethod, ReceiptUrl = req.ReceiptUrl, SubmittedByAdminId = adminUserId,
            };
            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(expense);
        }

        public async Task<(bool Success, string Message)> DecideAsync(int adminUserId, DecideExpenseRequestDto req)
        {
            var expense = await _db.Expenses.FindAsync(req.ExpenseId);
            if (expense == null) return (false, "Expense not found.");
            if (expense.Status != "Pending") return (false, $"Only Pending expenses can be decided (current: {expense.Status}).");
            expense.Status = req.Approve ? "Approved" : "Rejected";
            expense.ApprovedByAdminId = adminUserId;
            expense.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, req.Approve ? "Expense approved." : "Expense rejected.");
        }

        public async Task<(bool Success, string Message)> MarkPaidAsync(int expenseId)
        {
            var expense = await _db.Expenses.FindAsync(expenseId);
            if (expense == null) return (false, "Expense not found.");
            if (expense.Status != "Approved") return (false, $"Only Approved expenses can be marked Paid (current: {expense.Status}).");
            expense.Status = "Paid";
            await _db.SaveChangesAsync();
            return (true, "Expense marked Paid.");
        }

        public async Task<List<ExpenseDto>> GetAllAsync(string? status = null)
        {
            var q = _db.Expenses.Include(e => e.SubmittedByAdmin).Include(e => e.ApprovedByAdmin).AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);
            var list = await q.OrderByDescending(e => e.CreatedAt).ToListAsync();
            var result = new List<ExpenseDto>();
            foreach (var e in list) result.Add(await ToDtoAsync(e));
            return result;
        }

        private Task<ExpenseDto> ToDtoAsync(Expense e) => Task.FromResult(new ExpenseDto
        {
            Id = e.Id, Category = e.Category, Description = e.Description, Amount = e.Amount, Vendor = e.Vendor,
            ExpenseDate = e.ExpenseDate, PaymentMethod = e.PaymentMethod, Status = e.Status, ReceiptUrl = e.ReceiptUrl,
            SubmittedByName = e.SubmittedByAdmin?.FullName, ApprovedByName = e.ApprovedByAdmin?.FullName, ApprovedAt = e.ApprovedAt, CreatedAt = e.CreatedAt,
        });
    }

    // ── M102: Profit & Loss Dashboard ──────────────────────────────
    public class ProfitLossService
    {
        private readonly AppDbContext _db;
        public ProfitLossService(AppDbContext db) => _db = db;

        // Reads the M100 FinancialReportSnapshot for the revenue side (must
        // be generated first) and sums Approved/Paid M101 Expenses for the
        // cost side — no duplicate revenue computation here.
        public async Task<(bool Success, string Message, PnLDto? Report)> GetAsync(string periodKey)
        {
            if (!DateTime.TryParseExact(periodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", null);
            var monthEnd = monthStart.AddMonths(1);

            var snapshot = await _db.FinancialReportSnapshots.FirstOrDefaultAsync(s => s.PeriodKey == periodKey);
            if (snapshot == null) return (false, "No financial report found for this period — generate one via /api/financial-reports/generate first.", null);

            var expenses = await _db.Expenses.Where(e => (e.Status == "Approved" || e.Status == "Paid") && e.ExpenseDate >= monthStart && e.ExpenseDate < monthEnd).ToListAsync();
            var totalExpenses = expenses.Sum(e => e.Amount);
            var netProfit = snapshot.PlatformCommissionEarned - snapshot.TotalRefundsIssued - totalExpenses;

            var breakdown = expenses.GroupBy(e => e.Category).Select(g => new PnLExpenseBreakdownDto { Category = g.Key, Amount = g.Sum(x => x.Amount) }).OrderByDescending(x => x.Amount).ToList();

            return (true, "OK", new PnLDto
            {
                PeriodKey = periodKey, GrossRevenue = snapshot.GrossRevenue, PlatformCommissionEarned = snapshot.PlatformCommissionEarned,
                TotalRefundsIssued = snapshot.TotalRefundsIssued, TotalExpenses = totalExpenses, NetProfit = netProfit, ExpenseBreakdown = breakdown,
            });
        }
    }

    // ── M103: Ticket System (routing layer) ────────────────────────
    public class TicketRoutingService
    {
        private readonly AppDbContext _db;
        public TicketRoutingService(AppDbContext db) => _db = db;

        public async Task<TicketQueueDto> CreateQueueAsync(CreateTicketQueueRequestDto req)
        {
            var queue = new TicketQueue { Name = req.Name, Description = req.Description };
            _db.TicketQueues.Add(queue);
            await _db.SaveChangesAsync();
            return new TicketQueueDto { Id = queue.Id, Name = queue.Name, Description = queue.Description, IsActive = queue.IsActive, OpenTicketCount = 0 };
        }

        public async Task<List<TicketQueueDto>> GetQueuesAsync()
        {
            var queues = await _db.TicketQueues.ToListAsync();
            var rules = await _db.TicketAssignmentRules.ToListAsync();
            var result = new List<TicketQueueDto>();
            foreach (var q in queues)
            {
                var categories = rules.Where(r => r.TicketQueueId == q.Id).Select(r => r.Category).ToList();
                var openCount = await _db.SupportTickets.CountAsync(t => categories.Contains(t.Category) && (t.Status == "Open" || t.Status == "InProgress"));
                result.Add(new TicketQueueDto { Id = q.Id, Name = q.Name, Description = q.Description, IsActive = q.IsActive, OpenTicketCount = openCount });
            }
            return result;
        }

        public async Task<AssignmentRuleDto> CreateRuleAsync(CreateAssignmentRuleRequestDto req)
        {
            var rule = new TicketAssignmentRule { Category = req.Category, TicketQueueId = req.TicketQueueId, DefaultAssigneeAdminId = req.DefaultAssigneeAdminId, Priority = req.Priority };
            _db.TicketAssignmentRules.Add(rule);
            await _db.SaveChangesAsync();
            return await ToRuleDtoAsync(rule);
        }

        public async Task<List<AssignmentRuleDto>> GetRulesAsync()
        {
            var rules = await _db.TicketAssignmentRules.Include(r => r.TicketQueue).Include(r => r.DefaultAssigneeAdmin).OrderBy(r => r.Priority).ToListAsync();
            var result = new List<AssignmentRuleDto>();
            foreach (var r in rules) result.Add(await ToRuleDtoAsync(r));
            return result;
        }

        // Finds the highest-priority active rule matching the ticket's
        // category and applies it: sets AssignedAdminId if the rule has a
        // default assignee, then writes an activity log entry either way.
        public async Task<(bool Success, string Message)> RouteAsync(int ticketId)
        {
            var ticket = await _db.SupportTickets.FindAsync(ticketId);
            if (ticket == null) return (false, "Ticket not found.");
            var rule = await _db.TicketAssignmentRules.Where(r => r.Category == ticket.Category && r.IsActive).OrderBy(r => r.Priority).FirstOrDefaultAsync();
            if (rule == null) return (false, $"No active routing rule for category '{ticket.Category}'.");

            var fromAssignee = ticket.AssignedAdminId;
            if (rule.DefaultAssigneeAdminId.HasValue) ticket.AssignedAdminId = rule.DefaultAssigneeAdminId;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _db.TicketActivityLogs.Add(new TicketActivityLog
            {
                TicketId = ticketId, Action = "Routed", FromValue = fromAssignee?.ToString(), ToValue = rule.DefaultAssigneeAdminId?.ToString() ?? $"Queue #{rule.TicketQueueId}",
            });
            await _db.SaveChangesAsync();
            return (true, "Ticket routed.");
        }

        public async Task<(bool Success, string Message)> ReassignAsync(ReassignTicketRequestDto req)
        {
            var ticket = await _db.SupportTickets.FindAsync(req.TicketId);
            if (ticket == null) return (false, "Ticket not found.");
            var from = ticket.AssignedAdminId;
            ticket.AssignedAdminId = req.NewAssigneeAdminId;
            ticket.UpdatedAt = DateTime.UtcNow;
            _db.TicketActivityLogs.Add(new TicketActivityLog { TicketId = req.TicketId, Action = "Reassigned", FromValue = from?.ToString(), ToValue = req.NewAssigneeAdminId.ToString(), ChangedByAdminId = req.ChangedByAdminId });
            await _db.SaveChangesAsync();
            return (true, "Ticket reassigned.");
        }

        public async Task<List<TicketActivityDto>> GetActivityAsync(int ticketId)
        {
            var logs = await _db.TicketActivityLogs.Include(l => l.ChangedByAdmin).Where(l => l.TicketId == ticketId).OrderByDescending(l => l.ChangedAt).ToListAsync();
            return logs.Select(l => new TicketActivityDto { Action = l.Action, FromValue = l.FromValue, ToValue = l.ToValue, ChangedByName = l.ChangedByAdmin?.FullName, ChangedAt = l.ChangedAt }).ToList();
        }

        private async Task<AssignmentRuleDto> ToRuleDtoAsync(TicketAssignmentRule r)
        {
            var queue = r.TicketQueue ?? await _db.TicketQueues.FindAsync(r.TicketQueueId);
            var assignee = r.DefaultAssigneeAdmin ?? (r.DefaultAssigneeAdminId.HasValue ? await _db.Users.FindAsync(r.DefaultAssigneeAdminId.Value) : null);
            return new AssignmentRuleDto { Id = r.Id, Category = r.Category, TicketQueueId = r.TicketQueueId, QueueName = queue?.Name, DefaultAssigneeAdminId = r.DefaultAssigneeAdminId, DefaultAssigneeName = assignee?.FullName, Priority = r.Priority, IsActive = r.IsActive };
        }
    }

    // ── M104: Live Chat Support ─────────────────────────────────────
    public class LiveChatService
    {
        private readonly AppDbContext _db;
        public LiveChatService(AppDbContext db) => _db = db;

        public async Task<LiveChatSessionDto> StartAsync(StartLiveChatRequestDto req)
        {
            var session = new LiveChatSession { UserId = req.UserId, InitialMessage = req.InitialMessage };
            _db.LiveChatSessions.Add(session);
            await _db.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(req.InitialMessage))
            {
                _db.LiveChatMessages.Add(new LiveChatMessage { LiveChatSessionId = session.Id, SenderId = req.UserId, SenderRole = "User", Message = req.InitialMessage });
                await _db.SaveChangesAsync();
            }
            return await ToDtoAsync(session);
        }

        public async Task<List<LiveChatSessionDto>> GetWaitingAsync()
        {
            var sessions = await _db.LiveChatSessions.Include(s => s.User).Where(s => s.Status == "Waiting").OrderBy(s => s.StartedAt).ToListAsync();
            var result = new List<LiveChatSessionDto>();
            foreach (var s in sessions) result.Add(await ToDtoAsync(s));
            return result;
        }

        public async Task<(bool Success, string Message)> AssignAsync(AssignLiveChatRequestDto req)
        {
            var session = await _db.LiveChatSessions.FindAsync(req.SessionId);
            if (session == null) return (false, "Session not found.");
            if (session.Status != "Waiting") return (false, $"Only Waiting sessions can be assigned (current: {session.Status}).");
            session.AgentAdminId = req.AgentAdminId;
            session.Status = "Active";
            session.AssignedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Session assigned.");
        }

        public async Task<(bool Success, string Message, LiveChatMessageDto? Message2)> SendMessageAsync(SendLiveChatMessageRequestDto req)
        {
            var session = await _db.LiveChatSessions.FindAsync(req.SessionId);
            if (session == null) return (false, "Session not found.", null);
            if (session.Status == "Closed") return (false, "This session is closed.", null);
            var message = new LiveChatMessage { LiveChatSessionId = req.SessionId, SenderId = req.SenderId, SenderRole = req.SenderRole, Message = req.Message };
            _db.LiveChatMessages.Add(message);
            await _db.SaveChangesAsync();
            return (true, "Sent.", new LiveChatMessageDto { Id = message.Id, SessionId = message.LiveChatSessionId, SenderId = message.SenderId, SenderRole = message.SenderRole, Message = message.Message, SentAt = message.SentAt });
        }

        public async Task<List<LiveChatMessageDto>> GetMessagesAsync(int sessionId)
        {
            var messages = await _db.LiveChatMessages.Where(m => m.LiveChatSessionId == sessionId).OrderBy(m => m.SentAt).ToListAsync();
            return messages.Select(m => new LiveChatMessageDto { Id = m.Id, SessionId = m.LiveChatSessionId, SenderId = m.SenderId, SenderRole = m.SenderRole, Message = m.Message, SentAt = m.SentAt }).ToList();
        }

        public async Task<(bool Success, string Message)> CloseAsync(CloseLiveChatRequestDto req)
        {
            var session = await _db.LiveChatSessions.FindAsync(req.SessionId);
            if (session == null) return (false, "Session not found.");
            session.Status = "Closed";
            session.ClosedAt = DateTime.UtcNow;
            if (req.Rating.HasValue) session.Rating = Math.Clamp(req.Rating.Value, 1, 5);
            await _db.SaveChangesAsync();
            return (true, "Session closed.");
        }

        private async Task<LiveChatSessionDto> ToDtoAsync(LiveChatSession s)
        {
            var user = s.User ?? await _db.Users.FindAsync(s.UserId);
            var agent = s.AgentAdmin ?? (s.AgentAdminId.HasValue ? await _db.Users.FindAsync(s.AgentAdminId.Value) : null);
            return new LiveChatSessionDto
            {
                Id = s.Id, UserId = s.UserId, UserName = user?.FullName, AgentAdminId = s.AgentAdminId, AgentName = agent?.FullName,
                Status = s.Status, InitialMessage = s.InitialMessage, Rating = s.Rating, StartedAt = s.StartedAt, AssignedAt = s.AssignedAt, ClosedAt = s.ClosedAt,
            };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/expenses"), Authorize(Roles = "Admin")]
    public class ExpenseController : ControllerBase
    {
        private readonly Services.ExpenseService _svc;
        public ExpenseController(Services.ExpenseService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(UserId, req) });

        [HttpPost("decide")]
        public async Task<IActionResult> Decide([FromBody] DecideExpenseRequestDto req)
        {
            var (success, message) = await _svc.DecideAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("mark-paid")]
        public async Task<IActionResult> MarkPaid([FromBody] MarkExpensePaidRequestDto req)
        {
            var (success, message) = await _svc.MarkPaidAsync(req.ExpenseId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status) => Ok(new { success = true, data = await _svc.GetAllAsync(status) });
    }

    [ApiController, Route("api/profit-loss"), Authorize(Roles = "Admin")]
    public class ProfitLossController : ControllerBase
    {
        private readonly Services.ProfitLossService _svc;
        public ProfitLossController(Services.ProfitLossService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string periodKey)
        {
            var (success, message, report) = await _svc.GetAsync(periodKey);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, data = report });
        }
    }

    [ApiController, Route("api/ticket-routing"), Authorize(Roles = "Admin")]
    public class TicketRoutingController : ControllerBase
    {
        private readonly Services.TicketRoutingService _svc;
        public TicketRoutingController(Services.TicketRoutingService svc) => _svc = svc;

        [HttpPost("queues")]
        public async Task<IActionResult> CreateQueue([FromBody] CreateTicketQueueRequestDto req) => Ok(new { success = true, data = await _svc.CreateQueueAsync(req) });

        [HttpGet("queues")]
        public async Task<IActionResult> GetQueues() => Ok(new { success = true, data = await _svc.GetQueuesAsync() });

        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] CreateAssignmentRuleRequestDto req) => Ok(new { success = true, data = await _svc.CreateRuleAsync(req) });

        [HttpGet("rules")]
        public async Task<IActionResult> GetRules() => Ok(new { success = true, data = await _svc.GetRulesAsync() });

        [HttpPost("route")]
        public async Task<IActionResult> Route([FromBody] RouteTicketRequestDto req)
        {
            var (success, message) = await _svc.RouteAsync(req.TicketId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("reassign")]
        public async Task<IActionResult> Reassign([FromBody] ReassignTicketRequestDto req)
        {
            var (success, message) = await _svc.ReassignAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("{ticketId}/activity")]
        public async Task<IActionResult> GetActivity(int ticketId) => Ok(new { success = true, data = await _svc.GetActivityAsync(ticketId) });
    }

    [ApiController, Route("api/live-chat"), Authorize]
    public class LiveChatController : ControllerBase
    {
        private readonly Services.LiveChatService _svc;
        private readonly IHubContext<Hubs.LiveChatHub> _hub;
        public LiveChatController(Services.LiveChatService svc, IHubContext<Hubs.LiveChatHub> hub) { _svc = svc; _hub = hub; }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartLiveChatRequestDto req)
        {
            var session = await _svc.StartAsync(req);
            await _hub.Clients.Group("support-agents").SendAsync("NewWaitingSession", session);
            return Ok(new { success = true, data = session });
        }

        [HttpGet("waiting"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWaiting() => Ok(new { success = true, data = await _svc.GetWaitingAsync() });

        [HttpPost("assign"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign([FromBody] AssignLiveChatRequestDto req)
        {
            var (success, message) = await _svc.AssignAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendLiveChatMessageRequestDto req)
        {
            var (success, message, dto) = await _svc.SendMessageAsync(req);
            if (!success) return BadRequest(new { success, message });
            await _hub.Clients.Group($"livechat-{req.SessionId}").SendAsync("ReceiveMessage", dto);
            return Ok(new { success, message, data = dto });
        }

        [HttpGet("{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(int sessionId) => Ok(new { success = true, data = await _svc.GetMessagesAsync(sessionId) });

        [HttpPost("close")]
        public async Task<IActionResult> Close([FromBody] CloseLiveChatRequestDto req)
        {
            var (success, message) = await _svc.CloseAsync(req);
            if (!success) return BadRequest(new { success, message });
            await _hub.Clients.Group($"livechat-{req.SessionId}").SendAsync("SessionClosed");
            return Ok(new { success, message });
        }
    }
}

namespace LovEat.API.Hubs
{
    /// <summary>
    /// M104: real-time delivery for Live Chat Support sessions. Follows the
    /// same pattern as M17 ChatHub — persistence happens through
    /// LiveChatService (called from LiveChatController), this hub is purely
    /// the live-delivery layer. Connects at /hubs/live-chat (already covered
    /// by the JWT-via-querystring wiring in Program.cs for the /hubs prefix).
    /// </summary>
    [Authorize]
    public class LiveChatHub : Hub
    {
        /// <summary>Client calls this after starting/opening a session to receive live messages for it.</summary>
        public async Task JoinSession(int sessionId) => await Groups.AddToGroupAsync(Context.ConnectionId, $"livechat-{sessionId}");

        /// <summary>Support agents call this on login to receive NewWaitingSession notifications.</summary>
        public async Task JoinAgentPool() => await Groups.AddToGroupAsync(Context.ConnectionId, "support-agents");
    }
}
