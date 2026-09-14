using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M91: Feature Flag Management ───────────────────────────────
    public class FeatureFlagService
    {
        private readonly AppDbContext _db;
        public FeatureFlagService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, FeatureFlagDto? Flag)> CreateAsync(int adminUserId, CreateFeatureFlagRequestDto req)
        {
            if (await _db.FeatureFlags.AnyAsync(f => f.Key == req.Key)) return (false, $"A flag with key '{req.Key}' already exists.", null);
            var flag = new FeatureFlag
            {
                Key = req.Key, Name = req.Name, Description = req.Description, TargetPlatform = req.TargetPlatform,
                TargetAudience = req.TargetAudience, TargetCity = req.TargetCity, Environment = req.Environment,
                CreatedByUserId = adminUserId,
            };
            _db.FeatureFlags.Add(flag);
            await _db.SaveChangesAsync();
            await LogAsync(flag.Id, "Created", adminUserId, null);
            return (true, "Flag created (disabled by default).", await ToDtoAsync(flag));
        }

        public async Task<(bool Success, string Message, FeatureFlagDto? Flag)> UpdateAsync(int adminUserId, UpdateFeatureFlagRequestDto req)
        {
            var flag = await _db.FeatureFlags.FindAsync(req.FlagId);
            if (flag == null) return (false, "Flag not found.", null);

            if (req.RolloutPercentage.HasValue && (req.RolloutPercentage < 0 || req.RolloutPercentage > 100))
                return (false, "RolloutPercentage must be between 0 and 100.", null);

            if (req.IsEnabled.HasValue && req.IsEnabled.Value != flag.IsEnabled)
            {
                flag.IsEnabled = req.IsEnabled.Value;
                await LogAsync(flag.Id, flag.IsEnabled ? "Enabled" : "Disabled", adminUserId, req.Notes);
            }
            if (req.RolloutPercentage.HasValue && req.RolloutPercentage != flag.RolloutPercentage)
            {
                flag.RolloutPercentage = req.RolloutPercentage.Value;
                await LogAsync(flag.Id, "RolloutChanged", adminUserId, $"Rollout -> {flag.RolloutPercentage}%");
            }
            var targetingChanged = false;
            if (req.TargetPlatform != null && req.TargetPlatform != flag.TargetPlatform) { flag.TargetPlatform = req.TargetPlatform; targetingChanged = true; }
            if (req.TargetAudience != null && req.TargetAudience != flag.TargetAudience) { flag.TargetAudience = req.TargetAudience; targetingChanged = true; }
            if (req.TargetCity != null && req.TargetCity != flag.TargetCity) { flag.TargetCity = req.TargetCity; targetingChanged = true; }
            if (targetingChanged) await LogAsync(flag.Id, "TargetingChanged", adminUserId, req.Notes);

            flag.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Flag updated.", await ToDtoAsync(flag));
        }

        public async Task<List<FeatureFlagDto>> GetAllAsync()
        {
            var flags = await _db.FeatureFlags.Include(f => f.CreatedBy).OrderBy(f => f.Key).ToListAsync();
            var result = new List<FeatureFlagDto>();
            foreach (var f in flags) result.Add(await ToDtoAsync(f));
            return result;
        }

        public async Task<List<FeatureFlagAuditEntryDto>> GetHistoryAsync(int flagId)
        {
            var log = await _db.FeatureFlagAuditLogs.Include(l => l.ChangedBy).Where(l => l.FeatureFlagId == flagId).OrderByDescending(l => l.ChangedAt).Take(50).ToListAsync();
            return log.Select(l => new FeatureFlagAuditEntryDto { Action = l.Action, ChangedByName = l.ChangedBy?.FullName, Notes = l.Notes, ChangedAt = l.ChangedAt }).ToList();
        }

        // Deterministic bucketing: same user always lands on the same side of
        // the rollout percentage line for a given flag, so their experience
        // doesn't flicker between requests as the flag ramps up.
        public async Task<EvaluateFlagResultDto> EvaluateAsync(EvaluateFlagRequestDto req)
        {
            var flag = await _db.FeatureFlags.FirstOrDefaultAsync(f => f.Key == req.Key);
            if (flag == null) return new EvaluateFlagResultDto { Key = req.Key, IsActive = false, Reason = "Flag not found — defaulting to off." };
            if (!flag.IsEnabled) return new EvaluateFlagResultDto { Key = req.Key, IsActive = false, Reason = "Flag disabled." };
            if (flag.TargetPlatform != "All" && req.Platform != null && flag.TargetPlatform != req.Platform)
                return new EvaluateFlagResultDto { Key = req.Key, IsActive = false, Reason = "Platform not targeted." };
            if (flag.TargetAudience != "All" && req.Audience != null && flag.TargetAudience != req.Audience)
                return new EvaluateFlagResultDto { Key = req.Key, IsActive = false, Reason = "Audience not targeted." };
            if (flag.TargetCity != null && req.City != null && flag.TargetCity != req.City)
                return new EvaluateFlagResultDto { Key = req.Key, IsActive = false, Reason = "City not targeted." };

            var bucket = Math.Abs((req.Key + ":" + req.UserId).GetHashCode()) % 100;
            var active = bucket < flag.RolloutPercentage;
            return new EvaluateFlagResultDto { Key = req.Key, IsActive = active, Reason = active ? $"In rollout bucket ({flag.RolloutPercentage}%)." : $"Outside rollout bucket ({flag.RolloutPercentage}%)." };
        }

        private async Task LogAsync(int flagId, string action, int changedByUserId, string? notes)
        {
            _db.FeatureFlagAuditLogs.Add(new FeatureFlagAuditLog { FeatureFlagId = flagId, Action = action, ChangedByUserId = changedByUserId, Notes = notes });
            await _db.SaveChangesAsync();
        }

        private async Task<FeatureFlagDto> ToDtoAsync(FeatureFlag f)
        {
            var createdBy = f.CreatedBy ?? await _db.Users.FindAsync(f.CreatedByUserId);
            return new FeatureFlagDto
            {
                Id = f.Id, Key = f.Key, Name = f.Name, Description = f.Description, IsEnabled = f.IsEnabled,
                RolloutPercentage = f.RolloutPercentage, TargetPlatform = f.TargetPlatform, TargetAudience = f.TargetAudience,
                TargetCity = f.TargetCity, Environment = f.Environment, CreatedByName = createdBy?.FullName, UpdatedAt = f.UpdatedAt,
            };
        }
    }

    // ── M92: Announcement Management ───────────────────────────────
    public class AnnouncementService
    {
        private readonly AppDbContext _db;
        public AnnouncementService(AppDbContext db) => _db = db;

        public async Task<AnnouncementDto> CreateAsync(int adminUserId, CreateAnnouncementRequestDto req)
        {
            var a = new Announcement
            {
                Title = req.Title, Body = req.Body, AnnouncementType = req.AnnouncementType, TargetAudience = req.TargetAudience,
                TargetCity = req.TargetCity, Priority = req.Priority, IsDismissible = req.IsDismissible,
                StartAt = req.StartAt, EndAt = req.EndAt, CreatedByUserId = adminUserId,
                Status = req.StartAt > DateTime.UtcNow ? "Scheduled" : "Live",
            };
            _db.Announcements.Add(a);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(a);
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdateAnnouncementStatusRequestDto req)
        {
            var valid = new[] { "Draft", "Scheduled", "Live", "Ended" };
            if (!valid.Contains(req.Status)) return (false, $"Invalid status. Must be one of: {string.Join(", ", valid)}.");
            var a = await _db.Announcements.FindAsync(req.AnnouncementId);
            if (a == null) return (false, "Announcement not found.");
            a.Status = req.Status;
            a.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"Announcement '{a.Title}' set to {req.Status}.");
        }

        public async Task<List<AnnouncementDto>> GetAllAsync()
        {
            var list = await _db.Announcements.Include(a => a.CreatedBy).OrderByDescending(a => a.CreatedAt).ToListAsync();
            var result = new List<AnnouncementDto>();
            foreach (var a in list) result.Add(await ToDtoAsync(a));
            return result;
        }

        public async Task<List<AnnouncementDto>> GetActiveForUserAsync(ActiveAnnouncementRequestDto req)
        {
            var now = DateTime.UtcNow;
            var q = _db.Announcements.Where(a => a.Status == "Live" && a.StartAt <= now && a.EndAt >= now);
            if (req.Audience != "All") q = q.Where(a => a.TargetAudience == "All" || a.TargetAudience == req.Audience);
            if (!string.IsNullOrEmpty(req.City)) q = q.Where(a => a.TargetCity == null || a.TargetCity == req.City);
            var candidates = await q.OrderBy(a => a.Priority).ToListAsync();

            var dismissedIds = await _db.AnnouncementReceipts.Where(r => r.UserId == req.UserId && r.DismissedAt != null).Select(r => r.AnnouncementId).ToListAsync();
            var visible = candidates.Where(a => !dismissedIds.Contains(a.Id)).ToList();

            var result = new List<AnnouncementDto>();
            foreach (var a in visible) result.Add(await ToDtoAsync(a));
            return result;
        }

        public async Task<(bool Success, string Message)> MarkAsync(MarkAnnouncementRequestDto req)
        {
            if (!await _db.Announcements.AnyAsync(a => a.Id == req.AnnouncementId)) return (false, "Announcement not found.");
            var receipt = await _db.AnnouncementReceipts.FirstOrDefaultAsync(r => r.AnnouncementId == req.AnnouncementId && r.UserId == req.UserId);
            if (receipt == null)
            {
                receipt = new AnnouncementReceipt { AnnouncementId = req.AnnouncementId, UserId = req.UserId };
                _db.AnnouncementReceipts.Add(receipt);
            }
            if (req.Action == "Dismiss") receipt.DismissedAt = DateTime.UtcNow;
            else receipt.ReadAt ??= DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Recorded.");
        }

        private async Task<AnnouncementDto> ToDtoAsync(Announcement a)
        {
            var createdBy = a.CreatedBy ?? await _db.Users.FindAsync(a.CreatedByUserId);
            var readCount = await _db.AnnouncementReceipts.CountAsync(r => r.AnnouncementId == a.Id && r.ReadAt != null);
            var dismissCount = await _db.AnnouncementReceipts.CountAsync(r => r.AnnouncementId == a.Id && r.DismissedAt != null);
            return new AnnouncementDto
            {
                Id = a.Id, Title = a.Title, Body = a.Body, AnnouncementType = a.AnnouncementType, TargetAudience = a.TargetAudience,
                TargetCity = a.TargetCity, Priority = a.Priority, IsDismissible = a.IsDismissible, Status = a.Status,
                StartAt = a.StartAt, EndAt = a.EndAt, CreatedByName = createdBy?.FullName,
                ReadCount = readCount, DismissCount = dismissCount, CreatedAt = a.CreatedAt,
            };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/feature-flags"), Authorize(Roles = "Admin")]
    public class FeatureFlagController : ControllerBase
    {
        private readonly Services.FeatureFlagService _svc;
        public FeatureFlagController(Services.FeatureFlagService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeatureFlagRequestDto req)
        {
            var (success, message, flag) = await _svc.CreateAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = flag });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateFeatureFlagRequestDto req)
        {
            var (success, message, flag) = await _svc.UpdateAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = flag });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(int id) => Ok(new { success = true, data = await _svc.GetHistoryAsync(id) });
    }

    // Public/authenticated-user surface: apps call this on startup to decide what's on for this user.
    [ApiController, Route("api/feature-flags/evaluate"), AllowAnonymous]
    public class FeatureFlagEvaluateController : ControllerBase
    {
        private readonly Services.FeatureFlagService _svc;
        public FeatureFlagEvaluateController(Services.FeatureFlagService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Evaluate([FromBody] EvaluateFlagRequestDto req) => Ok(new { success = true, data = await _svc.EvaluateAsync(req) });
    }

    [ApiController, Route("api/announcements"), Authorize(Roles = "Admin")]
    public class AnnouncementController : ControllerBase
    {
        private readonly Services.AnnouncementService _svc;
        public AnnouncementController(Services.AnnouncementService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(UserId, req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateAnnouncementStatusRequestDto req)
        {
            var (success, message) = await _svc.UpdateStatusAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    // Public/authenticated-user surface for CustomerApp/ChefApp to fetch and dismiss announcements.
    [ApiController, Route("api/announcements/inbox"), Authorize]
    public class AnnouncementInboxController : ControllerBase
    {
        private readonly Services.AnnouncementService _svc;
        public AnnouncementInboxController(Services.AnnouncementService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetActive([FromQuery] int userId, [FromQuery] string audience = "All", [FromQuery] string? city = null)
            => Ok(new { success = true, data = await _svc.GetActiveForUserAsync(new ActiveAnnouncementRequestDto { UserId = userId, Audience = audience, City = city }) });

        [HttpPost("mark")]
        public async Task<IActionResult> Mark([FromBody] MarkAnnouncementRequestDto req)
        {
            var (success, message) = await _svc.MarkAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
