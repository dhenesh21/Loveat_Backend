using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M87: Commission Engine ─────────────────────────────────────
    public class CommissionEngineService
    {
        private readonly AppDbContext _db;
        public CommissionEngineService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CommissionProfileDetailDto? Profile)> CreateProfileAsync(int adminUserId, CreateCommissionProfileRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Name)) return (false, "Name is required.", null);

            var profile = new CommissionProfile
            {
                Name = req.Name,
                Description = req.Description,
                DefaultPlatformPercent = req.DefaultPlatformPercent,
                DefaultChefPercent = 100 - req.DefaultPlatformPercent,
                EffectiveFrom = req.EffectiveFrom,
                EffectiveTo = req.EffectiveTo,
                CreatedByUserId = adminUserId,
            };
            _db.CommissionProfiles.Add(profile);
            await _db.SaveChangesAsync();

            if (req.CloneFromProfileId.HasValue)
            {
                var sourceRules = await _db.CommissionProfileRules.Where(r => r.CommissionProfileId == req.CloneFromProfileId.Value).ToListAsync();
                foreach (var r in sourceRules)
                {
                    _db.CommissionProfileRules.Add(new CommissionProfileRule
                    {
                        CommissionProfileId = profile.Id,
                        RuleType = r.RuleType,
                        Priority = r.Priority,
                        City = r.City,
                        Category = r.Category,
                        MinMonthlyBookingVolume = r.MinMonthlyBookingVolume,
                        MaxMonthlyBookingVolume = r.MaxMonthlyBookingVolume,
                        MinChefRating = r.MinChefRating,
                        SeasonStart = r.SeasonStart,
                        SeasonEnd = r.SeasonEnd,
                        MaxChefTenureDays = r.MaxChefTenureDays,
                        OverridePlatformPercent = r.OverridePlatformPercent,
                        DeltaPlatformPercent = r.DeltaPlatformPercent,
                        IsStackable = r.IsStackable,
                    });
                }
                profile.ClonedFromProfileId = req.CloneFromProfileId;
                await _db.SaveChangesAsync();
            }

            await LogAsync(profile.Id, "Created", adminUserId, req.CloneFromProfileId.HasValue ? $"Cloned from profile #{req.CloneFromProfileId}" : null);
            return (true, "Commission profile created as Draft.", await GetDetailAsync(profile.Id));
        }

        public async Task<(bool Success, string Message)> SubmitForApprovalAsync(int adminUserId, int profileId)
        {
            var profile = await _db.CommissionProfiles.FindAsync(profileId);
            if (profile == null) return (false, "Profile not found.");
            if (profile.Status != "Draft") return (false, $"Only Draft profiles can be submitted (current status: {profile.Status}).");
            var ruleCount = await _db.CommissionProfileRules.CountAsync(r => r.CommissionProfileId == profileId);
            profile.Status = "PendingApproval";
            profile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await LogAsync(profileId, "SubmittedForApproval", adminUserId, $"{ruleCount} rule(s) attached.");
            return (true, "Submitted for approval.");
        }

        public async Task<(bool Success, string Message)> ApproveAsync(int adminUserId, ApproveCommissionProfileRequestDto req)
        {
            var profile = await _db.CommissionProfiles.FindAsync(req.ProfileId);
            if (profile == null) return (false, "Profile not found.");
            if (profile.Status != "PendingApproval") return (false, $"Profile is not pending approval (current status: {profile.Status}).");

            if (!req.Approve)
            {
                profile.Status = "Draft";
                await _db.SaveChangesAsync();
                await LogAsync(profile.Id, "Rejected", adminUserId, req.Notes);
                return (true, "Profile rejected and returned to Draft.");
            }

            // Only one Active profile at a time — archive whichever is currently live.
            var currentlyActive = await _db.CommissionProfiles.Where(p => p.Status == "Active").ToListAsync();
            foreach (var p in currentlyActive)
            {
                p.Status = "Archived";
                p.UpdatedAt = DateTime.UtcNow;
                await LogAsync(p.Id, "Archived", adminUserId, $"Superseded by profile #{profile.Id} ({profile.Name}).");
            }

            profile.Status = "Active";
            profile.ApprovedByUserId = adminUserId;
            profile.ApprovedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await LogAsync(profile.Id, "Approved", adminUserId, req.Notes);
            return (true, $"Profile '{profile.Name}' approved and activated.");
        }

        public async Task<List<CommissionProfileDto>> GetAllAsync()
        {
            var profiles = await _db.CommissionProfiles.Include(p => p.CreatedBy).Include(p => p.ApprovedBy).OrderByDescending(p => p.CreatedAt).ToListAsync();
            var ruleCounts = await _db.CommissionProfileRules.GroupBy(r => r.CommissionProfileId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count);
            return profiles.Select(p => ToDto(p, ruleCounts.GetValueOrDefault(p.Id))).ToList();
        }

        public async Task<CommissionProfileDetailDto?> GetDetailAsync(int profileId)
        {
            var profile = await _db.CommissionProfiles.Include(p => p.CreatedBy).Include(p => p.ApprovedBy).FirstOrDefaultAsync(p => p.Id == profileId);
            if (profile == null) return null;
            var rules = await _db.CommissionProfileRules.Where(r => r.CommissionProfileId == profileId).OrderBy(r => r.Priority).ToListAsync();
            var log = await _db.CommissionRuleAuditLogs.Include(l => l.ChangedBy).Where(l => l.CommissionProfileId == profileId).OrderByDescending(l => l.ChangedAt).Take(50).ToListAsync();

            var dto = ToDto(profile, rules.Count);
            var detail = new CommissionProfileDetailDto
            {
                Id = dto.Id, Name = dto.Name, Description = dto.Description, Status = dto.Status,
                DefaultPlatformPercent = dto.DefaultPlatformPercent, DefaultChefPercent = dto.DefaultChefPercent,
                Version = dto.Version, EffectiveFrom = dto.EffectiveFrom, EffectiveTo = dto.EffectiveTo,
                CreatedByName = dto.CreatedByName, ApprovedByName = dto.ApprovedByName, ApprovedAt = dto.ApprovedAt,
                RuleCount = dto.RuleCount, CreatedAt = dto.CreatedAt,
                Rules = rules.Select(ToRuleDto).ToList(),
                AuditLog = log.Select(l => new CommissionAuditEntryDto { Action = l.Action, ChangedByName = l.ChangedBy?.FullName, Notes = l.Notes, ChangedAt = l.ChangedAt }).ToList(),
            };
            return detail;
        }

        private static CommissionProfileDto ToDto(CommissionProfile p, int ruleCount) => new()
        {
            Id = p.Id, Name = p.Name, Description = p.Description, Status = p.Status,
            DefaultPlatformPercent = p.DefaultPlatformPercent, DefaultChefPercent = p.DefaultChefPercent,
            Version = p.Version, EffectiveFrom = p.EffectiveFrom, EffectiveTo = p.EffectiveTo,
            CreatedByName = p.CreatedBy?.FullName, ApprovedByName = p.ApprovedBy?.FullName, ApprovedAt = p.ApprovedAt,
            RuleCount = ruleCount, CreatedAt = p.CreatedAt,
        };

        private static CommissionProfileRuleDto ToRuleDto(CommissionProfileRule r) => new()
        {
            Id = r.Id, RuleType = r.RuleType, Priority = r.Priority, City = r.City, Category = r.Category,
            MinMonthlyBookingVolume = r.MinMonthlyBookingVolume, MaxMonthlyBookingVolume = r.MaxMonthlyBookingVolume,
            MinChefRating = r.MinChefRating, SeasonStart = r.SeasonStart, SeasonEnd = r.SeasonEnd,
            MaxChefTenureDays = r.MaxChefTenureDays, OverridePlatformPercent = r.OverridePlatformPercent,
            DeltaPlatformPercent = r.DeltaPlatformPercent, IsActive = r.IsActive, IsStackable = r.IsStackable,
        };

        internal async Task LogAsync(int profileId, string action, int changedByUserId, string? notes)
        {
            _db.CommissionRuleAuditLogs.Add(new CommissionRuleAuditLog { CommissionProfileId = profileId, Action = action, ChangedByUserId = changedByUserId, Notes = notes });
            await _db.SaveChangesAsync();
        }
    }

    // ── M88: Dynamic Commission Rules ──────────────────────────────
    public class DynamicCommissionRuleService
    {
        private readonly AppDbContext _db;
        private readonly CommissionEngineService _engine;
        public DynamicCommissionRuleService(AppDbContext db, CommissionEngineService engine) { _db = db; _engine = engine; }

        public async Task<(bool Success, string Message, CommissionProfileRuleDto? Rule)> AddRuleAsync(int adminUserId, AddCommissionRuleRequestDto req)
        {
            var profile = await _db.CommissionProfiles.FindAsync(req.CommissionProfileId);
            if (profile == null) return (false, "Commission profile not found.", null);
            if (profile.Status == "Active" || profile.Status == "Archived")
                return (false, "Cannot edit a rule on an Active or Archived profile — clone it into a new Draft first.", null);

            if (req.OverridePlatformPercent == null && req.DeltaPlatformPercent == null)
                return (false, "Provide either OverridePlatformPercent or DeltaPlatformPercent.", null);

            var rule = new CommissionProfileRule
            {
                CommissionProfileId = req.CommissionProfileId,
                RuleType = req.RuleType,
                Priority = req.Priority,
                City = req.City,
                Category = req.Category,
                MinMonthlyBookingVolume = req.MinMonthlyBookingVolume,
                MaxMonthlyBookingVolume = req.MaxMonthlyBookingVolume,
                MinChefRating = req.MinChefRating,
                SeasonStart = req.SeasonStart,
                SeasonEnd = req.SeasonEnd,
                MaxChefTenureDays = req.MaxChefTenureDays,
                OverridePlatformPercent = req.OverridePlatformPercent,
                DeltaPlatformPercent = req.DeltaPlatformPercent,
                IsStackable = req.IsStackable,
            };
            _db.CommissionProfileRules.Add(rule);
            await _db.SaveChangesAsync();
            await _engine.LogAsync(req.CommissionProfileId, "RuleAdded", adminUserId, $"{req.RuleType} (priority {req.Priority})");
            return (true, "Rule added.", new CommissionProfileRuleDto
            {
                Id = rule.Id, RuleType = rule.RuleType, Priority = rule.Priority, City = rule.City, Category = rule.Category,
                MinMonthlyBookingVolume = rule.MinMonthlyBookingVolume, MaxMonthlyBookingVolume = rule.MaxMonthlyBookingVolume,
                MinChefRating = rule.MinChefRating, SeasonStart = rule.SeasonStart, SeasonEnd = rule.SeasonEnd,
                MaxChefTenureDays = rule.MaxChefTenureDays, OverridePlatformPercent = rule.OverridePlatformPercent,
                DeltaPlatformPercent = rule.DeltaPlatformPercent, IsActive = rule.IsActive, IsStackable = rule.IsStackable,
            });
        }

        public async Task<(bool Success, string Message)> RemoveRuleAsync(int adminUserId, int ruleId)
        {
            var rule = await _db.CommissionProfileRules.FindAsync(ruleId);
            if (rule == null) return (false, "Rule not found.");
            var profileId = rule.CommissionProfileId;
            _db.CommissionProfileRules.Remove(rule);
            await _db.SaveChangesAsync();
            await _engine.LogAsync(profileId, "RuleRemoved", adminUserId, $"{rule.RuleType} (priority {rule.Priority})");
            return (true, "Rule removed.");
        }

        // Evaluates the currently Active commission profile against a specific
        // booking's context (chef, city, category, this month's volume) and
        // returns the final platform/chef split plus a human-readable trail.
        public async Task<(bool Success, string Message, CommissionEvaluationResultDto? Result)> EvaluateAsync(EvaluateCommissionRequestDto req)
        {
            var profile = await _db.CommissionProfiles.FirstOrDefaultAsync(p => p.Status == "Active");
            if (profile == null) return (false, "No Active commission profile configured.", null);

            var chef = await _db.ChefProfiles.FirstOrDefaultAsync(c => c.UserId == req.ChefId);
            var rules = await _db.CommissionProfileRules.Where(r => r.CommissionProfileId == profile.Id && r.IsActive).OrderBy(r => r.Priority).ToListAsync();

            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthlyVolume = await _db.Bookings.CountAsync(b => b.ChefId == req.ChefId && b.CreatedAt >= monthStart && b.Status == "Completed");
            var chefTenureDays = chef == null ? int.MaxValue : (int)(DateTime.UtcNow - chef.CreatedAt).TotalDays;
            var chefRating = chef?.AverageRating ?? 0;
            var now = DateTime.UtcNow;

            decimal platformPercent = profile.DefaultPlatformPercent;
            var summary = new List<string> { $"Base {platformPercent}%" };

            bool Matches(CommissionProfileRule r) => r.RuleType switch
            {
                "CityOverride" => string.IsNullOrEmpty(r.City) || string.Equals(r.City, req.City, StringComparison.OrdinalIgnoreCase),
                "CategoryOverride" => string.IsNullOrEmpty(r.Category) || string.Equals(r.Category, req.Category, StringComparison.OrdinalIgnoreCase),
                "VolumeTier" => monthlyVolume >= (r.MinMonthlyBookingVolume ?? 0) && monthlyVolume <= (r.MaxMonthlyBookingVolume ?? int.MaxValue),
                "SeasonalMultiplier" => (!r.SeasonStart.HasValue || now >= r.SeasonStart) && (!r.SeasonEnd.HasValue || now <= r.SeasonEnd),
                "PerformanceBonus" => chefRating >= (r.MinChefRating ?? 0),
                "NewChefIncentive" => chefTenureDays <= (r.MaxChefTenureDays ?? 0),
                _ => false,
            };

            // First pass: highest-priority non-stackable match sets the base.
            var baseMatch = rules.Where(r => !r.IsStackable).Where(Matches).OrderBy(r => r.Priority).FirstOrDefault();
            if (baseMatch != null)
            {
                if (baseMatch.OverridePlatformPercent.HasValue) platformPercent = baseMatch.OverridePlatformPercent.Value;
                else if (baseMatch.DeltaPlatformPercent.HasValue) platformPercent += baseMatch.DeltaPlatformPercent.Value;
                summary.Add($"{baseMatch.RuleType} -> {platformPercent}%");
            }

            // Second pass: stackable matches (e.g. volume tiers, performance bonuses) layer on top.
            foreach (var stack in rules.Where(r => r.IsStackable).Where(Matches).OrderBy(r => r.Priority))
            {
                if (stack.OverridePlatformPercent.HasValue) platformPercent = stack.OverridePlatformPercent.Value;
                else if (stack.DeltaPlatformPercent.HasValue) platformPercent += stack.DeltaPlatformPercent.Value;
                summary.Add($"+{stack.RuleType} stack -> {platformPercent}%");
            }

            platformPercent = Math.Clamp(platformPercent, 0, 100);
            var chefPercent = 100 - platformPercent;

            var evaluation = new DynamicCommissionEvaluation
            {
                BookingId = req.BookingId,
                ChefId = req.ChefId,
                CommissionProfileId = profile.Id,
                BookingAmount = req.BookingAmount,
                ComputedPlatformPercent = platformPercent,
                ComputedChefPercent = chefPercent,
                AppliedRulesSummary = string.Join(" | ", summary),
            };
            _db.DynamicCommissionEvaluations.Add(evaluation);
            await _db.SaveChangesAsync();

            return (true, "Evaluated.", new CommissionEvaluationResultDto
            {
                CommissionProfileId = profile.Id,
                CommissionProfileName = profile.Name,
                PlatformPercent = platformPercent,
                ChefPercent = chefPercent,
                PlatformAmount = Math.Round(req.BookingAmount * platformPercent / 100, 2),
                ChefAmount = Math.Round(req.BookingAmount * chefPercent / 100, 2),
                AppliedRulesSummary = evaluation.AppliedRulesSummary,
            });
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/commission-engine"), Authorize(Roles = "Admin")]
    public class CommissionEngineController : ControllerBase
    {
        private readonly Services.CommissionEngineService _svc;
        public CommissionEngineController(Services.CommissionEngineService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("profiles")]
        public async Task<IActionResult> Create([FromBody] CreateCommissionProfileRequestDto req)
        {
            var (success, message, profile) = await _svc.CreateProfileAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = profile });
        }

        [HttpGet("profiles")]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpGet("profiles/{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var profile = await _svc.GetDetailAsync(id);
            if (profile == null) return NotFound(new { success = false, message = "Profile not found." });
            return Ok(new { success = true, data = profile });
        }

        [HttpPost("profiles/{id}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            var (success, message) = await _svc.SubmitForApprovalAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("profiles/approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveCommissionProfileRequestDto req)
        {
            var (success, message) = await _svc.ApproveAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/commission-engine/rules"), Authorize(Roles = "Admin")]
    public class CommissionRulesController : ControllerBase
    {
        private readonly Services.DynamicCommissionRuleService _svc;
        public CommissionRulesController(Services.DynamicCommissionRuleService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddCommissionRuleRequestDto req)
        {
            var (success, message, rule) = await _svc.AddRuleAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = rule });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var (success, message) = await _svc.RemoveRuleAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("evaluate")]
        public async Task<IActionResult> Evaluate([FromBody] EvaluateCommissionRequestDto req)
        {
            var (success, message, result) = await _svc.EvaluateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = result });
        }
    }
}
