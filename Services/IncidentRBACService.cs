using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M47: Safety Incident Service ──────────────────────────────
    public class SafetyIncidentService
    {
        private readonly AppDbContext _db;
        public SafetyIncidentService(AppDbContext db) => _db = db;

        public async Task<IncidentDto> CreateAsync(int userId, CreateIncidentDto dto)
        {
            var count    = await _db.SafetyIncidents.CountAsync() + 1;
            var incident = new SafetyIncident
            {
                IncidentNumber   = $"INC-{DateTime.UtcNow:yyyy}-{count:D5}",
                ReportedByUserId = userId,
                BookingId        = dto.BookingId,
                IncidentType     = dto.IncidentType,
                Severity         = dto.Severity,
                Description      = dto.Description,
                Latitude         = dto.Latitude,
                Longitude        = dto.Longitude,
            };
            _db.SafetyIncidents.Add(incident);
            await _db.SaveChangesAsync();
            var user = await _db.Users.FindAsync(userId);
            return Map(incident, user?.FullName ?? "");
        }

        public async Task<List<IncidentDto>> GetAllAsync(string? status = null)
        {
            var q = _db.SafetyIncidents.Include(i => i.ReportedBy).AsQueryable();
            if (status != null) q = q.Where(i => i.Status == status);
            var list = await q.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return list.Select(i => Map(i, i.ReportedBy?.FullName ?? "")).ToList();
        }

        public async Task<bool> UpdateAsync(int id, string status, string? notes, string? resolution)
        {
            var incident = await _db.SafetyIncidents.FindAsync(id);
            if (incident == null) return false;
            incident.Status     = status;
            incident.AdminNotes = notes ?? incident.AdminNotes;
            incident.Resolution = resolution ?? incident.Resolution;
            incident.UpdatedAt  = DateTime.UtcNow;
            if (status == "Resolved") incident.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static IncidentDto Map(SafetyIncident i, string reportedBy) => new()
        {
            Id=i.Id, IncidentNumber=i.IncidentNumber, ReportedBy=reportedBy,
            BookingId=i.BookingId, IncidentType=i.IncidentType, Severity=i.Severity,
            Description=i.Description, Status=i.Status, AdminNotes=i.AdminNotes,
            Resolution=i.Resolution, CreatedAt=i.CreatedAt.ToString("MMM dd, yyyy hh:mm tt"),
            UpdatedAt=i.UpdatedAt.ToString("MMM dd, yyyy hh:mm tt"),
        };
    }

    // ── M48: RBAC Service ──────────────────────────────────────────
    public class RBACService
    {
        private readonly AppDbContext _db;
        public RBACService(AppDbContext db) => _db = db;

        public async Task<List<AdminRoleDto>> GetRolesAsync()
        {
            var roles = await _db.AdminRoles.Include(r => r.AdminUsers).ToListAsync();
            return roles.Select(r => new AdminRoleDto
            {
                Id=r.Id, RoleName=r.RoleName, Description=r.Description, IsActive=r.IsActive,
                AdminCount=r.AdminUsers.Count(u => u.IsActive),
                Permissions=JsonSerializer.Deserialize<List<string>>(r.Permissions) ?? new(),
            }).ToList();
        }

        public async Task<List<AdminUserDto>> GetAdminUsersAsync()
        {
            var users = await _db.AdminUsers.Include(a => a.User).Include(a => a.AdminRole).ToListAsync();
            return users.Select(a => new AdminUserDto
            {
                Id=a.Id, FullName=a.User?.FullName ?? "", Email=a.User?.Email ?? "",
                Phone=a.User?.PhoneNumber ?? "", RoleName=a.AdminRole?.RoleName ?? "",
                IsActive=a.IsActive, LastLogin=a.LastLogin?.ToString("MMM dd, yyyy hh:mm tt"),
                CreatedAt=a.CreatedAt.ToString("MMM dd, yyyy"),
            }).ToList();
        }

        public async Task<bool> ToggleUserAsync(int adminUserId)
        {
            var au = await _db.AdminUsers.FindAsync(adminUserId);
            if (au == null) return false;
            au.IsActive = !au.IsActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task SeedDefaultRolesAsync()
        {
            if (await _db.AdminRoles.AnyAsync()) return;
            var roles = new[]
            {
                new AdminRole { RoleName="SuperAdmin",    Description="Full access to all modules", Permissions=JsonSerializer.Serialize(new[]{"*"}) },
                new AdminRole { RoleName="FinanceAdmin",  Description="Access to payments, settlements, tax", Permissions=JsonSerializer.Serialize(new[]{"commission","settlement","invoices","ledger"}) },
                new AdminRole { RoleName="SupportAdmin",  Description="Access to tickets and disputes", Permissions=JsonSerializer.Serialize(new[]{"support","disputes","quality"}) },
                new AdminRole { RoleName="ContentAdmin",  Description="Access to CMS, campaigns, FAQs", Permissions=JsonSerializer.Serialize(new[]{"cms","campaigns","faqs"}) },
            };
            _db.AdminRoles.AddRange(roles);
            await _db.SaveChangesAsync();
        }
    }
}

// ── Controllers ────────────────────────────────────────────────────
namespace LovEat.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using LovEat.API.DTOs;
    using LovEat.API.Services;

    [ApiController, Route("api/incidents"), Authorize]
    public class SafetyIncidentController : ControllerBase
    {
        private readonly Services.SafetyIncidentService _svc;
        public SafetyIncidentController(Services.SafetyIncidentService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIncidentDto dto)
        {
            var incident = await _svc.CreateAsync(UserId, dto);
            return Ok(new { success=true, data=incident, message=$"Incident {incident.IncidentNumber} reported" });
        }

        [HttpGet("admin/all"), Authorize(Roles="Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var data = await _svc.GetAllAsync(status);
            return Ok(new { success=true, data });
        }

        [HttpPost("admin/{id}/update"), Authorize(Roles="Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] dynamic req)
        {
            string status   = (string)(req.status ?? "InProgress");
            string? notes   = (string?)(req.adminNotes);
            string? res     = (string?)(req.resolution);
            var ok = await _svc.UpdateAsync(id, status, notes, res);
            return Ok(new { success=ok });
        }
    }

    [ApiController, Route("api/rbac"), Authorize(Roles="Admin")]
    public class RBACController : ControllerBase
    {
        private readonly Services.RBACService _svc;
        public RBACController(Services.RBACService svc) => _svc = svc;

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var data = await _svc.GetRolesAsync();
            return Ok(new { success=true, data });
        }

        [HttpGet("admin-users")]
        public async Task<IActionResult> GetAdminUsers()
        {
            var data = await _svc.GetAdminUsersAsync();
            return Ok(new { success=true, data });
        }

        [HttpPost("admin-users/{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var ok = await _svc.ToggleUserAsync(id);
            return Ok(new { success=ok });
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            await _svc.SeedDefaultRolesAsync();
            return Ok(new { success=true, message="Default roles seeded" });
        }
    }
}
