using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    public class CrmService
    {
        private readonly AppDbContext _db;
        public CrmService(AppDbContext db) => _db = db;

        public async Task<CrmSummaryDto> GetSummaryAsync()
        {
            var now        = DateTime.UtcNow;
            var users      = await _db.Users.Where(u => u.Role == "Customer").ToListAsync();
            var bookings   = await _db.Bookings.ToListAsync();
            var thisMonth  = now.AddDays(-30);
            var activeIds  = bookings.Where(b => b.CreatedAt >= thisMonth).Select(b => b.CustomerId).Distinct().ToHashSet();
            var newUsers   = users.Count(u => u.CreatedAt >= thisMonth);
            var churnRisk  = users.Count(u => !activeIds.Contains(u.Id) && u.CreatedAt < now.AddDays(-60));

            var segments = await _db.UserSegments.Where(s => s.IsActive).ToListAsync();

            return new CrmSummaryDto
            {
                TotalUsers      = users.Count,
                ActiveThisMonth = activeIds.Count,
                NewThisMonth    = newUsers,
                ChurnRisk       = churnRisk,
                RetentionRate   = users.Count > 0 ? Math.Round(activeIds.Count * 100.0 / users.Count, 1) : 0,
                Segments        = segments.Select(s => new UserSegmentDto { Id=s.Id, Name=s.Name, Description=s.Description, UserCount=s.UserCount, IsActive=s.IsActive, CreatedAt=s.CreatedAt.ToString("MMM dd, yyyy") }).ToList(),
            };
        }

        public async Task<List<CrmUserDto>> GetUsersAsync(string? segment = null, int page = 1, int size = 50)
        {
            var users    = await _db.Users.Where(u => u.Role == "Customer").Skip((page-1)*size).Take(size).ToListAsync();
            var bookings = await _db.Bookings.ToListAsync();
            var loyalty  = await _db.LoyaltyPoints.ToListAsync();
            var profiles = await _db.UserProfiles.ToListAsync();

            return users.Select(u => {
                var ub     = bookings.Where(b => b.CustomerId == u.Id).ToList();
                var lp     = loyalty.FirstOrDefault(l => l.UserId == u.Id);
                var prof   = profiles.FirstOrDefault(p => p.UserId == u.Id);
                return new CrmUserDto
                {
                    UserId        = u.Id,
                    FullName      = u.FullName ?? "",
                    Phone         = u.PhoneNumber ?? "",
                    City          = prof?.City ?? "",
                    TotalBookings = ub.Count,
                    TotalSpend    = ub.Sum(b => b.TotalAmount),
                    LastActive    = ub.Any() ? ub.Max(b => b.CreatedAt).ToString("MMM dd, yyyy") : "Never",
                    JoinDate      = u.CreatedAt.ToString("MMM dd, yyyy"),
                    Segment       = ub.Count >= 10 ? "VIP" : ub.Count >= 3 ? "Regular" : ub.Count >= 1 ? "New" : "Inactive",
                    LoyaltyTier   = lp?.Tier ?? "Bronze",
                    Notes         = new(),
                };
            }).ToList();
        }

        public async Task<CrmNoteDto> AddNoteAsync(int userId, int adminId, string note, string noteType)
        {
            var crmNote = new CrmNote { UserId=userId, AdminId=adminId, Note=note, NoteType=noteType };
            _db.CrmNotes.Add(crmNote);
            await _db.SaveChangesAsync();
            var admin = await _db.Users.FindAsync(adminId);
            return new CrmNoteDto { Id=crmNote.Id, Note=note, NoteType=noteType, AdminName=admin?.FullName ?? "Admin", CreatedAt=crmNote.CreatedAt.ToString("MMM dd, yyyy hh:mm tt") };
        }

        public async Task<UserSegmentDto> CreateSegmentAsync(string name, string description, string filterJson)
        {
            var seg = new UserSegment { Name=name, Description=description, FilterJson=filterJson };
            _db.UserSegments.Add(seg);
            await _db.SaveChangesAsync();
            return new UserSegmentDto { Id=seg.Id, Name=seg.Name, Description=seg.Description, UserCount=0, IsActive=true, CreatedAt=seg.CreatedAt.ToString("MMM dd, yyyy") };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/crm"), Authorize(Roles="Admin")]
    public class CrmController : ControllerBase
    {
        private readonly Services.CrmService _svc;
        public CrmController(Services.CrmService svc) => _svc = svc;
        private int AdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("summary")]
        public async Task<IActionResult> Summary() => Ok(new { success=true, data=await _svc.GetSummaryAsync() });

        [HttpGet("users")]
        public async Task<IActionResult> Users([FromQuery] string? segment, [FromQuery] int page=1)
            => Ok(new { success=true, data=await _svc.GetUsersAsync(segment, page) });

        [HttpPost("users/{userId}/notes")]
        public async Task<IActionResult> AddNote(int userId, [FromBody] dynamic req)
        {
            string note = (string)(req.note ?? "");
            string type = (string)(req.noteType ?? "General");
            var data = await _svc.AddNoteAsync(userId, AdminId, note, type);
            return Ok(new { success=true, data });
        }

        [HttpPost("segments")]
        public async Task<IActionResult> CreateSegment([FromBody] dynamic req)
        {
            var data = await _svc.CreateSegmentAsync((string)(req.name??""), (string)(req.description??""), (string)(req.filterJson??"{}"));
            return Ok(new { success=true, data });
        }
    }
}
