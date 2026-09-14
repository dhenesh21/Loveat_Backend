using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class SupportService
    {
        private readonly AppDbContext _db;
        public SupportService(AppDbContext db) => _db = db;

        // ── Create ticket ──────────────────────────────────────────
        public async Task<TicketDto> CreateTicketAsync(int userId, CreateTicketDto dto)
        {
            var count  = await _db.SupportTickets.CountAsync() + 1;
            var ticket = new SupportTicket
            {
                UserId       = userId,
                TicketNumber = $"TKT-{count:D5}",
                Subject      = dto.Subject,
                Category     = dto.Category,
                Priority     = dto.Priority,
                Description  = dto.Description,
                BookingId    = dto.BookingId,
            };
            _db.SupportTickets.Add(ticket);
            await _db.SaveChangesAsync();

            // Auto-add first message from description
            _db.SupportMessages.Add(new SupportMessage
            {
                TicketId   = ticket.Id,
                SenderId   = userId,
                SenderRole = "User",
                Message    = dto.Description,
            });
            await _db.SaveChangesAsync();
            return MapTicket(ticket);
        }

        // ── Get user's tickets ─────────────────────────────────────
        public async Task<List<TicketDto>> GetMyTicketsAsync(int userId)
        {
            var tickets = await _db.SupportTickets
                .Where(t => t.UserId == userId)
                .Include(t => t.Messages)
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync();
            return tickets.Select(MapTicket).ToList();
        }

        // ── Get single ticket with messages ───────────────────────
        public async Task<TicketDto?> GetTicketAsync(int ticketId, int userId, bool isAdmin = false)
        {
            var ticket = await _db.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == ticketId && (isAdmin || t.UserId == userId));
            return ticket == null ? null : MapTicket(ticket);
        }

        // ── Reply to ticket ────────────────────────────────────────
        public async Task<SupportMessageDto?> ReplyAsync(int senderId, string senderRole, ReplyTicketDto dto)
        {
            var ticket = await _db.SupportTickets.FindAsync(dto.TicketId);
            if (ticket == null) return null;

            var msg = new SupportMessage
            {
                TicketId   = dto.TicketId,
                SenderId   = senderId,
                SenderRole = senderRole,
                Message    = dto.Message,
                IsInternal = dto.IsInternal,
            };
            _db.SupportMessages.Add(msg);

            // Auto move to InProgress when admin replies
            if (senderRole == "Admin" && ticket.Status == "Open")
                ticket.Status = "InProgress";

            ticket.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new SupportMessageDto
            {
                Id         = msg.Id,
                SenderId   = msg.SenderId,
                SenderRole = msg.SenderRole,
                Message    = msg.Message,
                CreatedAt  = msg.CreatedAt.ToString("MMM dd, yyyy hh:mm tt"),
            };
        }

        // ── Update ticket status ───────────────────────────────────
        public async Task<bool> UpdateStatusAsync(int ticketId, UpdateTicketStatusDto dto)
        {
            var ticket = await _db.SupportTickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.Status    = dto.Status;
            ticket.UpdatedAt = DateTime.UtcNow;
            if (dto.Resolution != null) ticket.Resolution = dto.Resolution;
            if (dto.Rating     != null) ticket.Rating     = dto.Rating;
            if (dto.Status == "Resolved") ticket.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── FAQs ───────────────────────────────────────────────────
        public async Task<List<FaqDto>> GetFaqsAsync(string? audience = null)
        {
            var q = _db.FAQs.Where(f => f.IsActive);
            if (audience != null) q = q.Where(f => f.Audience == audience || f.Audience == "All");
            var faqs = await q.OrderBy(f => f.Category).ThenBy(f => f.SortOrder).ToListAsync();
            return faqs.Select(f => new FaqDto
            {
                Id = f.Id, Question = f.Question, Answer = f.Answer,
                Category = f.Category, Audience = f.Audience, HelpfulCount = f.HelpfulCount,
            }).ToList();
        }

        // ── Admin stats ────────────────────────────────────────────
        public async Task<AdminSupportStatsDto> GetAdminStatsAsync()
        {
            var all     = await _db.SupportTickets.Include(t => t.User).ToListAsync();
            var today   = DateTime.UtcNow.Date;
            var resolved= all.Where(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == today).ToList();
            var rated   = all.Where(t => t.Rating.HasValue).ToList();
            var resolvedWithTime = all.Where(t => t.ResolvedAt.HasValue).ToList();
            double avgHours = resolvedWithTime.Any()
                ? resolvedWithTime.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours) : 0;

            return new AdminSupportStatsDto
            {
                OpenTickets       = all.Count(t => t.Status == "Open"),
                InProgressTickets = all.Count(t => t.Status == "InProgress"),
                ResolvedToday     = resolved.Count,
                AvgResolutionHours= Math.Round(avgHours, 1),
                AvgRating         = rated.Any() ? Math.Round(rated.Average(t => (double)t.Rating!), 1) : 0,
                RecentTickets     = all.OrderByDescending(t => t.UpdatedAt).Take(20).Select(MapTicket).ToList(),
            };
        }

        // ── Mapper ─────────────────────────────────────────────────
        private static TicketDto MapTicket(SupportTicket t) => new()
        {
            Id           = t.Id,
            TicketNumber = t.TicketNumber,
            Subject      = t.Subject,
            Category     = t.Category,
            Priority     = t.Priority,
            Status       = t.Status,
            Description  = t.Description,
            Resolution   = t.Resolution,
            Rating       = t.Rating,
            CreatedAt    = t.CreatedAt.ToString("MMM dd, yyyy"),
            UpdatedAt    = t.UpdatedAt.ToString("MMM dd, yyyy hh:mm tt"),
            Messages     = t.Messages.OrderBy(m => m.CreatedAt).Select(m => new SupportMessageDto
            {
                Id         = m.Id,
                SenderId   = m.SenderId,
                SenderRole = m.SenderRole,
                Message    = m.Message,
                CreatedAt  = m.CreatedAt.ToString("MMM dd, yyyy hh:mm tt"),
            }).ToList(),
        };
    }
}
