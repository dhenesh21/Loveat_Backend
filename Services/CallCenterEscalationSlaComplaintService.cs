using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M105: Call Center Dashboard ────────────────────────────────
    public class CallCenterService
    {
        private readonly AppDbContext _db;
        public CallCenterService(AppDbContext db) => _db = db;

        public async Task<CallLogDto> LogCallAsync(LogCallRequestDto req)
        {
            var call = new CallLog
            {
                UserId = req.UserId, AgentAdminId = req.AgentAdminId, Direction = req.Direction, PhoneNumber = req.PhoneNumber,
                Status = req.Status, LinkedTicketId = req.LinkedTicketId, Notes = req.Notes,
                EndedAt = req.Status != "Ongoing" ? DateTime.UtcNow : null,
            };
            _db.CallLogs.Add(call);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(call);
        }

        public async Task<(bool Success, string Message)> EndCallAsync(EndCallRequestDto req)
        {
            var call = await _db.CallLogs.FindAsync(req.CallLogId);
            if (call == null) return (false, "Call log not found.");
            call.Status = "Answered";
            call.DurationSeconds = req.DurationSeconds;
            call.Notes = req.Notes ?? call.Notes;
            call.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Call ended.");
        }

        public async Task<List<CallLogDto>> GetRecentAsync(int take = 100)
        {
            var calls = await _db.CallLogs.Include(c => c.User).Include(c => c.AgentAdmin).OrderByDescending(c => c.StartedAt).Take(take).ToListAsync();
            var result = new List<CallLogDto>();
            foreach (var c in calls) result.Add(await ToDtoAsync(c));
            return result;
        }

        public async Task<CallCenterStatsDto> GetStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var calls = await _db.CallLogs.Where(c => c.StartedAt >= today).ToListAsync();
            var answered = calls.Where(c => c.Status == "Answered").ToList();
            var missed = calls.Count(c => c.Status == "Missed");
            return new CallCenterStatsDto
            {
                TotalCallsToday = calls.Count, AnsweredToday = answered.Count, MissedToday = missed,
                AvgHandleTimeSeconds = answered.Count == 0 ? 0 : Math.Round(answered.Average(c => c.DurationSeconds), 1),
                MissedRatePercent = calls.Count == 0 ? 0 : Math.Round((double)missed / calls.Count * 100, 1),
            };
        }

        private Task<CallLogDto> ToDtoAsync(CallLog c) => Task.FromResult(new CallLogDto
        {
            Id = c.Id, UserId = c.UserId, UserName = c.User?.FullName, AgentAdminId = c.AgentAdminId, AgentName = c.AgentAdmin?.FullName,
            Direction = c.Direction, PhoneNumber = c.PhoneNumber, Status = c.Status, DurationSeconds = c.DurationSeconds,
            LinkedTicketId = c.LinkedTicketId, Notes = c.Notes, StartedAt = c.StartedAt, EndedAt = c.EndedAt,
        });
    }

    // ── M106: Escalation Matrix ────────────────────────────────────
    public class EscalationMatrixService
    {
        private readonly AppDbContext _db;
        public EscalationMatrixService(AppDbContext db) => _db = db;

        public async Task<EscalationLevelDto> CreateLevelAsync(CreateEscalationLevelRequestDto req)
        {
            var level = new EscalationLevel { Level = req.Level, Name = req.Name, TriggerAfterHours = req.TriggerAfterHours, NotifyAdminId = req.NotifyAdminId };
            _db.EscalationLevels.Add(level);
            await _db.SaveChangesAsync();
            var notify = req.NotifyAdminId.HasValue ? await _db.Users.FindAsync(req.NotifyAdminId.Value) : null;
            return new EscalationLevelDto { Id = level.Id, Level = level.Level, Name = level.Name, TriggerAfterHours = level.TriggerAfterHours, NotifyAdminName = notify?.FullName, IsActive = level.IsActive };
        }

        public async Task<List<EscalationLevelDto>> GetLevelsAsync()
        {
            var levels = await _db.EscalationLevels.Include(l => l.NotifyAdmin).OrderBy(l => l.Level).ToListAsync();
            return levels.Select(l => new EscalationLevelDto { Id = l.Id, Level = l.Level, Name = l.Name, TriggerAfterHours = l.TriggerAfterHours, NotifyAdminName = l.NotifyAdmin?.FullName, IsActive = l.IsActive }).ToList();
        }

        public async Task<(bool Success, string Message, EscalationEventDto? Event)> EscalateAsync(int ticketId, string reason, bool automatic, int? byAdminId)
        {
            var state = await _db.TicketEscalationStates.FirstOrDefaultAsync(s => s.TicketId == ticketId);
            if (state == null) { state = new TicketEscalationState { TicketId = ticketId }; _db.TicketEscalationStates.Add(state); await _db.SaveChangesAsync(); }

            var nextLevel = await _db.EscalationLevels.Where(l => l.Level > state.CurrentLevel && l.IsActive).OrderBy(l => l.Level).FirstOrDefaultAsync();
            if (nextLevel == null) return (false, "Already at the highest escalation level.", null);

            var evt = new EscalationEvent { TicketId = ticketId, FromLevel = state.CurrentLevel, ToLevel = nextLevel.Level, Reason = reason, WasAutomatic = automatic, EscalatedByAdminId = byAdminId };
            _db.EscalationEvents.Add(evt);
            state.CurrentLevel = nextLevel.Level;
            state.EnteredLevelAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var byAdmin = byAdminId.HasValue ? await _db.Users.FindAsync(byAdminId.Value) : null;
            return (true, $"Escalated to level {nextLevel.Level} ({nextLevel.Name}).", new EscalationEventDto
            {
                TicketId = ticketId, FromLevel = evt.FromLevel, ToLevel = evt.ToLevel, Reason = evt.Reason, WasAutomatic = evt.WasAutomatic,
                EscalatedByName = byAdmin?.FullName, EscalatedAt = evt.EscalatedAt,
            });
        }

        // Sweeps every open ticket's current escalation state; if it's sat
        // longer than the current level's TriggerAfterHours, auto-escalates.
        public async Task<AutoEscalationSweepResultDto> AutoEscalateOverdueAsync()
        {
            var openTicketIds = await _db.SupportTickets.Where(t => t.Status == "Open" || t.Status == "InProgress").Select(t => t.Id).ToListAsync();
            var states = await _db.TicketEscalationStates.Where(s => openTicketIds.Contains(s.TicketId)).ToListAsync();
            var levels = await _db.EscalationLevels.Where(l => l.IsActive).ToDictionaryAsync(l => l.Level);

            var result = new AutoEscalationSweepResultDto { ScannedCount = states.Count };
            foreach (var state in states)
            {
                if (!levels.TryGetValue(state.CurrentLevel, out var level)) continue;
                if ((DateTime.UtcNow - state.EnteredLevelAt).TotalHours < level.TriggerAfterHours) continue;
                var (ok, _, evt) = await EscalateAsync(state.TicketId, $"Auto-escalated: unresolved for {level.TriggerAfterHours}+ hours at level {level.Level}.", true, null);
                if (ok && evt != null) { result.EscalatedCount++; result.Events.Add(evt); }
            }
            return result;
        }
    }

    // ── M107: SLA Management ───────────────────────────────────────
    public class SlaManagementService
    {
        private readonly AppDbContext _db;
        public SlaManagementService(AppDbContext db) => _db = db;

        public async Task<SlaPolicyDto> CreatePolicyAsync(CreateSlaPolicyRequestDto req)
        {
            var policy = new SlaPolicy { Category = req.Category, Priority = req.Priority, ResponseTimeMinutes = req.ResponseTimeMinutes, ResolutionTimeHours = req.ResolutionTimeHours };
            _db.SlaPolicies.Add(policy);
            await _db.SaveChangesAsync();
            return ToPolicyDto(policy);
        }

        public async Task<List<SlaPolicyDto>> GetPoliciesAsync() => (await _db.SlaPolicies.ToListAsync()).Select(ToPolicyDto).ToList();

        // Finds the best-matching active policy (exact category+priority,
        // falling back to category-only, then priority-only, then "All")
        // and creates a tracker with computed due times.
        public async Task<(bool Success, string Message, SlaTrackerDto? Tracker)> AttachAsync(int ticketId)
        {
            var ticket = await _db.SupportTickets.FindAsync(ticketId);
            if (ticket == null) return (false, "Ticket not found.", null);
            if (await _db.SlaTrackers.AnyAsync(t => t.TicketId == ticketId)) return (false, "SLA already attached to this ticket.", null);

            var policies = await _db.SlaPolicies.Where(p => p.IsActive).ToListAsync();
            var policy = policies.FirstOrDefault(p => p.Category == ticket.Category && p.Priority == ticket.Priority)
                ?? policies.FirstOrDefault(p => p.Category == ticket.Category && p.Priority == "All")
                ?? policies.FirstOrDefault(p => p.Category == "All" && p.Priority == ticket.Priority)
                ?? policies.FirstOrDefault(p => p.Category == "All" && p.Priority == "All");
            if (policy == null) return (false, "No matching SLA policy found.", null);

            var tracker = new SlaTracker
            {
                TicketId = ticketId, SlaPolicyId = policy.Id,
                FirstResponseDueAt = DateTime.UtcNow.AddMinutes(policy.ResponseTimeMinutes),
                ResolutionDueAt = DateTime.UtcNow.AddHours(policy.ResolutionTimeHours),
            };
            _db.SlaTrackers.Add(tracker);
            await _db.SaveChangesAsync();
            return (true, "SLA attached.", ToTrackerDto(tracker, policy.Category));
        }

        public async Task<(bool Success, string Message)> MarkFirstResponseAsync(int ticketId)
        {
            var tracker = await _db.SlaTrackers.FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (tracker == null) return (false, "No SLA tracker for this ticket.");
            if (tracker.FirstRespondedAt.HasValue) return (false, "First response already recorded.");
            tracker.FirstRespondedAt = DateTime.UtcNow;
            tracker.IsResponseBreached = DateTime.UtcNow > tracker.FirstResponseDueAt;
            await _db.SaveChangesAsync();
            return (true, tracker.IsResponseBreached ? "Recorded — response SLA was breached." : "Recorded — within SLA.");
        }

        public async Task<(bool Success, string Message)> MarkResolvedAsync(int ticketId)
        {
            var tracker = await _db.SlaTrackers.FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (tracker == null) return (false, "No SLA tracker for this ticket.");
            tracker.ResolvedAt = DateTime.UtcNow;
            tracker.IsResolutionBreached = DateTime.UtcNow > tracker.ResolutionDueAt;
            await _db.SaveChangesAsync();
            return (true, tracker.IsResolutionBreached ? "Recorded — resolution SLA was breached." : "Recorded — within SLA.");
        }

        public async Task<SlaComplianceStatsDto> GetComplianceStatsAsync()
        {
            var trackers = await _db.SlaTrackers.Where(t => t.ResolvedAt != null).ToListAsync();
            var respBreach = trackers.Count(t => t.IsResponseBreached);
            var resBreach = trackers.Count(t => t.IsResolutionBreached);
            return new SlaComplianceStatsDto
            {
                TotalTracked = trackers.Count, ResponseBreaches = respBreach, ResolutionBreaches = resBreach,
                ResponseComplianceRate = trackers.Count == 0 ? 100 : Math.Round((1 - (double)respBreach / trackers.Count) * 100, 1),
                ResolutionComplianceRate = trackers.Count == 0 ? 100 : Math.Round((1 - (double)resBreach / trackers.Count) * 100, 1),
            };
        }

        private static SlaPolicyDto ToPolicyDto(SlaPolicy p) => new() { Id = p.Id, Category = p.Category, Priority = p.Priority, ResponseTimeMinutes = p.ResponseTimeMinutes, ResolutionTimeHours = p.ResolutionTimeHours, IsActive = p.IsActive };

        private static SlaTrackerDto ToTrackerDto(SlaTracker t, string? category) => new()
        {
            TicketId = t.TicketId, PolicyCategory = category, FirstResponseDueAt = t.FirstResponseDueAt, ResolutionDueAt = t.ResolutionDueAt,
            FirstRespondedAt = t.FirstRespondedAt, ResolvedAt = t.ResolvedAt, IsResponseBreached = t.IsResponseBreached, IsResolutionBreached = t.IsResolutionBreached,
        };
    }

    // ── M108: Complaint Tracking ───────────────────────────────────
    public class ComplaintTrackingService
    {
        private readonly AppDbContext _db;
        public ComplaintTrackingService(AppDbContext db) => _db = db;

        public async Task<ComplaintDto> FileAsync(FileComplaintRequestDto req)
        {
            var complaint = new Complaint
            {
                ComplaintNumber = $"CMP-{DateTime.UtcNow:yyyy}-{(await _db.Complaints.CountAsync() + 1):D5}",
                ComplainantUserId = req.ComplainantUserId, AgainstChefId = req.AgainstChefId, BookingId = req.BookingId,
                Category = req.Category, Severity = req.Severity, Description = req.Description,
            };
            _db.Complaints.Add(complaint);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(complaint);
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdateComplaintStatusRequestDto req)
        {
            var valid = new[] { "Received", "UnderInvestigation", "Resolved", "Dismissed" };
            if (!valid.Contains(req.Status)) return (false, $"Invalid status. Must be one of: {string.Join(", ", valid)}.");
            var complaint = await _db.Complaints.FindAsync(req.ComplaintId);
            if (complaint == null) return (false, "Complaint not found.");
            complaint.Status = req.Status;
            if (req.ResolutionAction != null) complaint.ResolutionAction = req.ResolutionAction;
            if (req.AssignedAdminId.HasValue) complaint.AssignedAdminId = req.AssignedAdminId;
            if (req.Status is "Resolved" or "Dismissed") complaint.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"Complaint {complaint.ComplaintNumber} set to {req.Status}.");
        }

        public async Task<(bool Success, string Message)> AddNoteAsync(AddComplaintNoteRequestDto req)
        {
            if (!await _db.Complaints.AnyAsync(c => c.Id == req.ComplaintId)) return (false, "Complaint not found.");
            _db.ComplaintUpdates.Add(new ComplaintUpdate { ComplaintId = req.ComplaintId, Note = req.Note, AddedByAdminId = req.AddedByAdminId });
            await _db.SaveChangesAsync();
            return (true, "Note added.");
        }

        public async Task<List<ComplaintDto>> GetAllAsync(string? status = null)
        {
            var q = _db.Complaints.Include(c => c.Complainant).Include(c => c.AgainstChef).Include(c => c.AssignedAdmin).AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(c => c.Status == status);
            var list = await q.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var result = new List<ComplaintDto>();
            foreach (var c in list) result.Add(await ToDtoAsync(c, includeUpdates: false));
            return result;
        }

        public async Task<ComplaintDto?> GetDetailAsync(int id)
        {
            var complaint = await _db.Complaints.Include(c => c.Complainant).Include(c => c.AgainstChef).Include(c => c.AssignedAdmin).FirstOrDefaultAsync(c => c.Id == id);
            if (complaint == null) return null;
            return await ToDtoAsync(complaint, includeUpdates: true);
        }

        private async Task<ComplaintDto> ToDtoAsync(Complaint c, bool includeUpdates = true)
        {
            var dto = new ComplaintDto
            {
                Id = c.Id, ComplaintNumber = c.ComplaintNumber, ComplainantUserId = c.ComplainantUserId, ComplainantName = c.Complainant?.FullName,
                AgainstChefId = c.AgainstChefId, AgainstChefName = c.AgainstChef?.FullName, BookingId = c.BookingId, Category = c.Category,
                Severity = c.Severity, Description = c.Description, Status = c.Status, ResolutionAction = c.ResolutionAction,
                AssignedAdminName = c.AssignedAdmin?.FullName, CreatedAt = c.CreatedAt, ResolvedAt = c.ResolvedAt,
            };
            if (includeUpdates)
            {
                var updates = await _db.ComplaintUpdates.Include(u => u.AddedByAdmin).Where(u => u.ComplaintId == c.Id).OrderByDescending(u => u.AddedAt).ToListAsync();
                dto.Updates = updates.Select(u => new ComplaintUpdateDto { Note = u.Note, AddedByName = u.AddedByAdmin?.FullName, AddedAt = u.AddedAt }).ToList();
            }
            return dto;
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/call-center"), Authorize(Roles = "Admin")]
    public class CallCenterController : ControllerBase
    {
        private readonly Services.CallCenterService _svc;
        public CallCenterController(Services.CallCenterService svc) => _svc = svc;

        [HttpPost("calls")]
        public async Task<IActionResult> LogCall([FromBody] LogCallRequestDto req) => Ok(new { success = true, data = await _svc.LogCallAsync(req) });

        [HttpPost("calls/end")]
        public async Task<IActionResult> EndCall([FromBody] EndCallRequestDto req)
        {
            var (success, message) = await _svc.EndCallAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("calls")]
        public async Task<IActionResult> GetRecent([FromQuery] int take = 100) => Ok(new { success = true, data = await _svc.GetRecentAsync(take) });

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats() => Ok(new { success = true, data = await _svc.GetStatsAsync() });
    }

    [ApiController, Route("api/escalation-matrix"), Authorize(Roles = "Admin")]
    public class EscalationMatrixController : ControllerBase
    {
        private readonly Services.EscalationMatrixService _svc;
        public EscalationMatrixController(Services.EscalationMatrixService svc) => _svc = svc;

        [HttpPost("levels")]
        public async Task<IActionResult> CreateLevel([FromBody] CreateEscalationLevelRequestDto req) => Ok(new { success = true, data = await _svc.CreateLevelAsync(req) });

        [HttpGet("levels")]
        public async Task<IActionResult> GetLevels() => Ok(new { success = true, data = await _svc.GetLevelsAsync() });

        [HttpPost("escalate")]
        public async Task<IActionResult> Escalate([FromBody] ManualEscalateRequestDto req)
        {
            var (success, message, evt) = await _svc.EscalateAsync(req.TicketId, req.Reason, false, req.EscalatedByAdminId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = evt });
        }

        [HttpPost("sweep")]
        public async Task<IActionResult> Sweep() => Ok(new { success = true, data = await _svc.AutoEscalateOverdueAsync() });
    }

    [ApiController, Route("api/sla-management"), Authorize(Roles = "Admin")]
    public class SlaManagementController : ControllerBase
    {
        private readonly Services.SlaManagementService _svc;
        public SlaManagementController(Services.SlaManagementService svc) => _svc = svc;

        [HttpPost("policies")]
        public async Task<IActionResult> CreatePolicy([FromBody] CreateSlaPolicyRequestDto req) => Ok(new { success = true, data = await _svc.CreatePolicyAsync(req) });

        [HttpGet("policies")]
        public async Task<IActionResult> GetPolicies() => Ok(new { success = true, data = await _svc.GetPoliciesAsync() });

        [HttpPost("attach")]
        public async Task<IActionResult> Attach([FromBody] AttachSlaRequestDto req)
        {
            var (success, message, tracker) = await _svc.AttachAsync(req.TicketId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = tracker });
        }

        [HttpPost("mark-first-response")]
        public async Task<IActionResult> MarkFirstResponse([FromBody] MarkFirstResponseRequestDto req)
        {
            var (success, message) = await _svc.MarkFirstResponseAsync(req.TicketId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("mark-resolved")]
        public async Task<IActionResult> MarkResolved([FromBody] MarkResolvedRequestDto req)
        {
            var (success, message) = await _svc.MarkResolvedAsync(req.TicketId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("compliance-stats")]
        public async Task<IActionResult> GetComplianceStats() => Ok(new { success = true, data = await _svc.GetComplianceStatsAsync() });
    }

    [ApiController, Route("api/complaints")]
    public class ComplaintController : ControllerBase
    {
        private readonly Services.ComplaintTrackingService _svc;
        public ComplaintController(Services.ComplaintTrackingService svc) => _svc = svc;

        [HttpPost, Authorize]
        public async Task<IActionResult> File([FromBody] FileComplaintRequestDto req) => Ok(new { success = true, data = await _svc.FileAsync(req) });

        [HttpPost("status"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateComplaintStatusRequestDto req)
        {
            var (success, message) = await _svc.UpdateStatusAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("notes"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddNote([FromBody] AddComplaintNoteRequestDto req)
        {
            var (success, message) = await _svc.AddNoteAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status) => Ok(new { success = true, data = await _svc.GetAllAsync(status) });

        [HttpGet("{id}"), Authorize]
        public async Task<IActionResult> GetDetail(int id)
        {
            var complaint = await _svc.GetDetailAsync(id);
            if (complaint == null) return NotFound(new { success = false, message = "Complaint not found." });
            return Ok(new { success = true, data = complaint });
        }
    }
}
