using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M45: Dispute Service ───────────────────────────────────────
    public class DisputeService
    {
        private readonly AppDbContext _db;
        public DisputeService(AppDbContext db) => _db = db;

        public async Task<DisputeDto> CreateAsync(int userId, CreateDisputeDto dto)
        {
            var count = await _db.Disputes.CountAsync() + 1;
            var booking = await _db.Bookings.FindAsync(dto.BookingId);
            var dispute = new Dispute
            {
                DisputeNumber  = $"DSP-{DateTime.UtcNow:yyyy}-{count:D5}",
                BookingId      = dto.BookingId,
                RaisedByUserId = userId,
                AgainstUserId  = booking?.ChefId,
                DisputeType    = dto.DisputeType,
                Description    = dto.Description,
                Priority       = dto.Priority,
            };
            _db.Disputes.Add(dispute);
            // Add first message
            dispute.Messages.Add(new DisputeMessage { SenderId=userId, SenderRole="Customer", Message=dto.Description });
            await _db.SaveChangesAsync();
            return Map(dispute, "You", "Chef");
        }

        public async Task<List<DisputeDto>> GetUserDisputesAsync(int userId)
        {
            var list = await _db.Disputes
                .Include(d => d.RaisedBy).Include(d => d.Messages).Include(d => d.Evidences)
                .Where(d => d.RaisedByUserId == userId || d.AgainstUserId == userId)
                .OrderByDescending(d => d.UpdatedAt).ToListAsync();
            return list.Select(d => Map(d, d.RaisedBy?.FullName ?? "", "Chef")).ToList();
        }

        public async Task<List<DisputeDto>> GetAllAdminAsync()
        {
            var list = await _db.Disputes
                .Include(d => d.RaisedBy).Include(d => d.Messages).Include(d => d.Evidences)
                .OrderByDescending(d => d.UpdatedAt).ToListAsync();
            return list.Select(d => Map(d, d.RaisedBy?.FullName ?? "", "")).ToList();
        }

        public async Task<bool> ReplyAsync(int disputeId, int senderId, string role, string message)
        {
            var dispute = await _db.Disputes.FindAsync(disputeId);
            if (dispute == null) return false;
            _db.DisputeMessages.Add(new DisputeMessage { DisputeId=disputeId, SenderId=senderId, SenderRole=role, Message=message });
            dispute.UpdatedAt = DateTime.UtcNow;
            if (role == "Admin" && dispute.Status == "Open") dispute.Status = "InReview";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResolveAsync(int disputeId, int adminId, ResolveDisputeDto dto)
        {
            var dispute = await _db.Disputes.FindAsync(disputeId);
            if (dispute == null) return false;
            dispute.Status        = dto.Status;
            dispute.Resolution    = dto.Resolution;
            dispute.RefundAmount  = dto.RefundAmount;
            dispute.ResolvedByAdmin= adminId;
            dispute.ResolvedAt    = DateTime.UtcNow;
            dispute.UpdatedAt     = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static DisputeDto Map(Dispute d, string raisedBy, string against) => new()
        {
            Id=d.Id, DisputeNumber=d.DisputeNumber, BookingId=d.BookingId,
            RaisedByName=raisedBy, AgainstName=against,
            DisputeType=d.DisputeType, Description=d.Description,
            Status=d.Status, Priority=d.Priority, RefundAmount=d.RefundAmount,
            Resolution=d.Resolution,
            CreatedAt=d.CreatedAt.ToString("MMM dd, yyyy"),
            UpdatedAt=d.UpdatedAt.ToString("MMM dd, yyyy hh:mm tt"),
            Messages=d.Messages.OrderBy(m=>m.CreatedAt).Select(m=>new DisputeMessageDto
                { Id=m.Id, SenderRole=m.SenderRole, Message=m.Message, CreatedAt=m.CreatedAt.ToString("hh:mm tt") }).ToList(),
            Evidences=d.Evidences.Select(e=>new DisputeEvidenceDto
                { Id=e.Id, FileUrl=e.FileUrl, FileType=e.FileType, Description=e.Description, UploadedAt=e.UploadedAt.ToString("MMM dd") }).ToList(),
        };
    }

    // ── M46: Fraud Detection Service ───────────────────────────────
    public class FraudDetectionService
    {
        private readonly AppDbContext _db;
        public FraudDetectionService(AppDbContext db) => _db = db;

        // Analyze a booking for fraud signals
        public async Task<FraudLogDto?> AnalyzeBookingAsync(int bookingId, int userId)
        {
            var recentBookings = await _db.Bookings
                .Where(b => b.CustomerId == userId && b.CreatedAt >= DateTime.UtcNow.AddHours(-24))
                .CountAsync();

            var cancelledCount = await _db.Bookings
                .Where(b => b.CustomerId == userId && b.Status == "Cancelled" && b.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            decimal riskScore = 0;
            var signals = new List<string>();

            if (recentBookings > 5) { riskScore += 40; signals.Add("5+ bookings in 24h"); }
            if (cancelledCount > 4) { riskScore += 30; signals.Add($"{cancelledCount} cancellations in 7 days"); }

            if (riskScore >= 40)
            {
                var log = new FraudDetectionLog
                {
                    UserId        = userId,
                    BookingId     = bookingId,
                    DetectionType = "FakeBooking",
                    Severity      = riskScore >= 70 ? "High" : "Medium",
                    RiskScore     = riskScore,
                    Evidence      = System.Text.Json.JsonSerializer.Serialize(signals),
                    Action        = "Flagged",
                };
                _db.FraudDetectionLogs.Add(log);
                await _db.SaveChangesAsync();
                return MapLog(log, "User");
            }
            return null;
        }

        public async Task<FraudSummaryDto> GetSummaryAsync()
        {
            var logs = await _db.FraudDetectionLogs.Include(l => l.User).OrderByDescending(l => l.DetectedAt).ToListAsync();
            return new FraudSummaryDto
            {
                TotalFlagged  = logs.Count(l => l.Action == "Flagged"),
                TotalBlocked  = logs.Count(l => l.Action == "Blocked"),
                PendingReview = logs.Count(l => l.ReviewedAt == null),
                AvgRiskScore  = logs.Any() ? Math.Round(logs.Average(l => l.RiskScore), 1) : 0,
                Recent        = logs.Take(20).Select(l => MapLog(l, l.User?.FullName ?? "Unknown")).ToList(),
            };
        }

        public async Task<bool> UpdateActionAsync(int logId, string action, string? note = null)
        {
            var log = await _db.FraudDetectionLogs.FindAsync(logId);
            if (log == null) return false;
            log.Action     = action;
            log.AdminNote  = note;
            log.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static FraudLogDto MapLog(FraudDetectionLog l, string userName) => new()
        {
            Id=l.Id, UserName=userName, BookingId=l.BookingId,
            DetectionType=l.DetectionType, Severity=l.Severity, RiskScore=l.RiskScore,
            Action=l.Action, AdminNote=l.AdminNote, DetectedAt=l.DetectedAt.ToString("MMM dd, yyyy hh:mm tt"),
        };
    }
}
