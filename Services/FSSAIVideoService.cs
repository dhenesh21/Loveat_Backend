using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    // ── M63: FSSAI Service ────────────────────────────────────────
    public class FSSAIService
    {
        private readonly AppDbContext _db;
        public FSSAIService(AppDbContext db) => _db = db;

        public async Task<FSSAILicenseDto?> GetChefLicenseAsync(int chefId)
        {
            var lic = await _db.FSSAILicenses
                .Where(l => l.ChefId == chefId)
                .OrderByDescending(l => l.UpdatedAt)
                .FirstOrDefaultAsync();
            return lic == null ? null : Map(lic);
        }

        public async Task<FSSAILicenseDto> SubmitLicenseAsync(int chefId, SubmitFSSAIDto dto)
        {
            var existing = await _db.FSSAILicenses.FirstOrDefaultAsync(l => l.ChefId == chefId);
            if (existing != null)
            {
                existing.LicenseNumber = dto.LicenseNumber;
                existing.LicenseType   = dto.LicenseType;
                existing.BusinessName  = dto.BusinessName;
                existing.Address       = dto.Address;
                existing.LicenseDocUrl = dto.LicenseDocUrl;
                existing.IssueDate     = dto.IssueDate;
                existing.ExpiryDate    = dto.ExpiryDate;
                existing.Status        = "Pending";
                existing.UpdatedAt     = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Map(existing);
            }
            var lic = new FSSAILicense
            {
                ChefId        = chefId,
                LicenseNumber = dto.LicenseNumber,
                LicenseType   = dto.LicenseType,
                BusinessName  = dto.BusinessName,
                Address       = dto.Address,
                LicenseDocUrl = dto.LicenseDocUrl,
                IssueDate     = dto.IssueDate,
                ExpiryDate    = dto.ExpiryDate,
            };
            _db.FSSAILicenses.Add(lic);
            await _db.SaveChangesAsync();
            return Map(lic);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, string? reason = null)
        {
            var lic = await _db.FSSAILicenses.FindAsync(id);
            if (lic == null) return false;
            lic.Status          = status;
            lic.RejectionReason = reason;
            lic.UpdatedAt       = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<FSSAIAdminSummaryDto> GetAdminSummaryAsync()
        {
            var all    = await _db.FSSAILicenses.Include(l => l.Chef).ToListAsync();
            var chefs  = await _db.Users.Where(u => u.Role == "Chef").ToListAsync();
            var now    = DateTime.UtcNow;

            var licensed  = all.ToDictionary(l => l.ChefId);
            var licDtos   = chefs.Select(c => new FSSAILicenseChefDto
            {
                ChefId   = c.Id,
                ChefName = c.FullName ?? "",
                Phone    = c.PhoneNumber ?? "",
                License  = licensed.TryGetValue(c.Id, out var l) ? Map(l) : null,
            }).OrderBy(c => c.ChefName).ToList();

            return new FSSAIAdminSummaryDto
            {
                TotalLicenses = all.Count,
                Active        = all.Count(l => l.Status == "Active"),
                ExpiringSoon  = all.Count(l => l.Status == "Active" && (l.ExpiryDate - now).TotalDays <= 60),
                Expired       = all.Count(l => l.Status == "Expired" || (l.Status == "Active" && l.ExpiryDate < now)),
                PendingReview = all.Count(l => l.Status == "Pending"),
                Licenses      = licDtos,
            };
        }

        private static FSSAILicenseDto Map(FSSAILicense l)
        {
            var daysLeft = (int)(l.ExpiryDate - DateTime.UtcNow).TotalDays;
            return new FSSAILicenseDto
            {
                Id=l.Id, LicenseNumber=l.LicenseNumber, LicenseType=l.LicenseType,
                Status=l.Status, BusinessName=l.BusinessName, LicenseDocUrl=l.LicenseDocUrl,
                IssueDate=l.IssueDate.ToString("MMM dd, yyyy"),
                ExpiryDate=l.ExpiryDate.ToString("MMM dd, yyyy"),
                RenewalDate=l.RenewalDate?.ToString("MMM dd, yyyy"),
                RejectionReason=l.RejectionReason,
                DaysToExpiry=daysLeft,
                IsExpiringSoon=daysLeft is >= 0 and <= 60,
            };
        }
    }

    // ── M64: Video Consultation Service ───────────────────────────
    public class VideoConsultationService
    {
        private readonly AppDbContext _db;
        public VideoConsultationService(AppDbContext db) => _db = db;

        public async Task<VideoConsultationDto> BookAsync(int customerId, BookConsultationDto dto)
        {
            // Generate a Jitsi meeting room (free, no server needed)
            var roomId   = $"loveat-{Guid.NewGuid():N}".Substring(0, 20);
            var meetLink = $"https://meet.jit.si/{roomId}";

            var consult = new VideoConsultation
            {
                CustomerId   = customerId,
                ChefId       = dto.ChefId,
                BookingId    = dto.BookingId,
                ConsultType  = dto.ConsultType,
                ScheduledAt  = dto.ScheduledAt,
                DurationMins = dto.DurationMins,
                Fee          = dto.ConsultType == "PreBooking" ? 0 : 299,
                MeetingLink  = meetLink,
                MeetingId    = roomId,
            };
            _db.VideoConsultations.Add(consult);
            await _db.SaveChangesAsync();

            var customer = await _db.Users.FindAsync(customerId);
            var chef     = await _db.Users.FindAsync(dto.ChefId);
            return Map(consult, customer?.FullName ?? "", chef?.FullName ?? "");
        }

        public async Task<List<VideoConsultationDto>> GetMyConsultationsAsync(int userId, string role)
        {
            var q = _db.VideoConsultations
                .Include(c => c.Customer)
                .Include(c => c.Chef)
                .AsQueryable();

            if (role == "Customer") q = q.Where(c => c.CustomerId == userId);
            else if (role == "Chef") q = q.Where(c => c.ChefId == userId);

            var list = await q.OrderByDescending(c => c.ScheduledAt).ToListAsync();
            return list.Select(c => Map(c, c.Customer?.FullName ?? "", c.Chef?.FullName ?? "")).ToList();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, string? notes = null)
        {
            var c = await _db.VideoConsultations.FindAsync(id);
            if (c == null) return false;
            c.Status = status;
            c.Notes  = notes ?? c.Notes;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RateAsync(int id, int rating, string feedback)
        {
            var c = await _db.VideoConsultations.FindAsync(id);
            if (c == null) return false;
            c.Rating   = rating;
            c.Feedback = feedback;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<VideoConsultationDto>> GetAllAdminAsync()
        {
            var list = await _db.VideoConsultations
                .Include(c => c.Customer).Include(c => c.Chef)
                .OrderByDescending(c => c.ScheduledAt).ToListAsync();
            return list.Select(c => Map(c, c.Customer?.FullName ?? "", c.Chef?.FullName ?? "")).ToList();
        }

        private static VideoConsultationDto Map(VideoConsultation c, string custName, string chefName) => new()
        {
            Id=c.Id, CustomerName=custName, ChefName=chefName, ConsultType=c.ConsultType,
            Status=c.Status, DurationMins=c.DurationMins, Fee=c.Fee,
            MeetingLink=c.MeetingLink, MeetingId=c.MeetingId, Notes=c.Notes,
            Rating=c.Rating, Feedback=c.Feedback,
            ScheduledAt=c.ScheduledAt.ToString("MMM dd, yyyy hh:mm tt"),
            CreatedAt=c.CreatedAt.ToString("MMM dd, yyyy"),
        };
    }
}

namespace LovEat.API.Controllers
{
    // ── M63 Controller ─────────────────────────────────────────────
    [ApiController, Route("api/fssai"), Authorize]
    public class FSSAIController : ControllerBase
    {
        private readonly Services.FSSAIService _svc;
        public FSSAIController(Services.FSSAIService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my"), Authorize(Roles = "Chef")]
        public async Task<IActionResult> GetMy()
            => Ok(new { success=true, data=await _svc.GetChefLicenseAsync(UserId) });

        [HttpPost("submit"), Authorize(Roles = "Chef")]
        public async Task<IActionResult> Submit([FromBody] SubmitFSSAIDto dto)
        {
            var data = await _svc.SubmitLicenseAsync(UserId, dto);
            return Ok(new { success=true, data, message="FSSAI license submitted for review." });
        }

        [HttpGet("admin/summary"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Summary()
            => Ok(new { success=true, data=await _svc.GetAdminSummaryAsync() });

        [HttpPost("admin/{id}/update"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] dynamic req)
        {
            string status   = (string)(req.status ?? "");
            string? reason  = (string?)(req.rejectionReason);
            var ok = await _svc.UpdateStatusAsync(id, status, reason);
            return Ok(new { success=ok });
        }
    }

    // ── M64 Controller ─────────────────────────────────────────────
    [ApiController, Route("api/consultations"), Authorize]
    public class VideoConsultationController : ControllerBase
    {
        private readonly Services.VideoConsultationService _svc;
        public VideoConsultationController(Services.VideoConsultationService svc) => _svc = svc;
        private int    UserId   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
            => Ok(new { success=true, data=await _svc.GetMyConsultationsAsync(UserId, UserRole) });

        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] BookConsultationDto dto)
        {
            var data = await _svc.BookAsync(UserId, dto);
            return Ok(new { success=true, data, message="Consultation booked! Meeting link sent to both parties." });
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] dynamic req)
        {
            string status  = (string)(req.status ?? "");
            string? notes  = (string?)(req.notes);
            var ok = await _svc.UpdateStatusAsync(id, status, notes);
            return Ok(new { success=ok });
        }

        [HttpPost("{id}/rate")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Rate(int id, [FromBody] dynamic req)
        {
            int    rating   = (int)(req.rating ?? 5);
            string feedback = (string)(req.feedback ?? "");
            var ok = await _svc.RateAsync(id, rating, feedback);
            return Ok(new { success=ok });
        }

        [HttpGet("admin/all"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAll()
            => Ok(new { success=true, data=await _svc.GetAllAdminAsync() });
    }
}
