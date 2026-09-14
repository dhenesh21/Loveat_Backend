using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M89: CMS Management ────────────────────────────────────────
    public class CmsManagementService
    {
        private readonly AppDbContext _db;
        public CmsManagementService(AppDbContext db) => _db = db;

        public async Task<ContentBlockDto> SaveDraftAsync(int adminUserId, SaveContentBlockDraftRequestDto req)
        {
            var block = await _db.CmsContentBlocks.FirstOrDefaultAsync(b => b.Key == req.Key && b.Locale == req.Locale);
            if (block == null)
            {
                block = new CmsContentBlock { Key = req.Key, Locale = req.Locale, ContentType = req.ContentType };
                _db.CmsContentBlocks.Add(block);
            }
            block.ContentType = req.ContentType;
            block.DraftContent = req.DraftContent;
            block.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _db.CmsContentBlockVersions.Add(new CmsContentBlockVersion
            {
                CmsContentBlockId = block.Id,
                Version = block.Version + 1, // draft versions preview as version+1, only committed on Publish
                Content = req.DraftContent,
                SavedByUserId = adminUserId,
                Note = req.Note ?? "Draft saved",
            });
            await _db.SaveChangesAsync();
            return ToDto(block);
        }

        public async Task<(bool Success, string Message, ContentBlockDto? Block)> PublishAsync(int adminUserId, int contentBlockId)
        {
            var block = await _db.CmsContentBlocks.FindAsync(contentBlockId);
            if (block == null) return (false, "Content block not found.", null);
            if (string.IsNullOrWhiteSpace(block.DraftContent)) return (false, "Nothing to publish — draft content is empty.", null);

            block.PublishedContent = block.DraftContent;
            block.Version += 1;
            block.Status = "Published";
            block.PublishedByUserId = adminUserId;
            block.PublishedAt = DateTime.UtcNow;
            block.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"'{block.Key}' published as v{block.Version}.", ToDto(block));
        }

        public async Task<List<ContentBlockDto>> GetAllBlocksAsync(string? locale = null)
        {
            var q = _db.CmsContentBlocks.Include(b => b.PublishedBy).AsQueryable();
            if (!string.IsNullOrEmpty(locale)) q = q.Where(b => b.Locale == locale);
            return (await q.OrderBy(b => b.Key).ToListAsync()).Select(ToDto).ToList();
        }

        public async Task<ContentBlockDto?> GetPublishedAsync(string key, string locale = "en")
        {
            var block = await _db.CmsContentBlocks.FirstOrDefaultAsync(b => b.Key == key && b.Locale == locale && b.Status == "Published");
            return block == null ? null : ToDto(block);
        }

        public async Task<List<ContentBlockVersionDto>> GetHistoryAsync(int contentBlockId)
        {
            var versions = await _db.CmsContentBlockVersions.Include(v => v.SavedBy).Where(v => v.CmsContentBlockId == contentBlockId).OrderByDescending(v => v.Version).ToListAsync();
            return versions.Select(v => new ContentBlockVersionDto { Version = v.Version, Content = v.Content, SavedByName = v.SavedBy?.FullName, Note = v.Note, SavedAt = v.SavedAt }).ToList();
        }

        // Saves the existing M50 CmsPage and additionally snapshots a revision row.
        public async Task<CmsPageDto> SavePageWithRevisionAsync(int adminUserId, SaveCmsPageRequestDto req)
        {
            var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Slug == req.Slug);
            if (page == null) { page = new CmsPage { Slug = req.Slug }; _db.CmsPages.Add(page); }
            page.Title = req.Title;
            page.Content = req.Content;
            page.IsActive = req.IsActive;
            page.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _db.CmsPageRevisions.Add(new CmsPageRevision { CmsPageId = page.Id, Title = req.Title, Content = req.Content, EditedByUserId = adminUserId });
            await _db.SaveChangesAsync();

            return new CmsPageDto { Id = page.Id, Slug = page.Slug, Title = page.Title, Content = page.Content, IsActive = page.IsActive, UpdatedAt = page.UpdatedAt.ToString("MMM dd, yyyy") };
        }

        public async Task<List<CmsPageRevisionDto>> GetPageRevisionsAsync(int cmsPageId)
        {
            var revisions = await _db.CmsPageRevisions.Include(r => r.EditedBy).Where(r => r.CmsPageId == cmsPageId).OrderByDescending(r => r.EditedAt).Take(50).ToListAsync();
            return revisions.Select(r => new CmsPageRevisionDto { Id = r.Id, Title = r.Title, Content = r.Content, EditedByName = r.EditedBy?.FullName, EditedAt = r.EditedAt }).ToList();
        }

        private static ContentBlockDto ToDto(CmsContentBlock b) => new()
        {
            Id = b.Id, Key = b.Key, Locale = b.Locale, ContentType = b.ContentType, Status = b.Status,
            DraftContent = b.DraftContent, PublishedContent = b.PublishedContent, Version = b.Version,
            PublishedByName = b.PublishedBy?.FullName, PublishedAt = b.PublishedAt, UpdatedAt = b.UpdatedAt,
        };
    }

    // ── M90: Banner/Promotion Management ───────────────────────────
    public class PromoCampaignService
    {
        private readonly AppDbContext _db;
        private static readonly Random _rng = new();
        public PromoCampaignService(AppDbContext db) => _db = db;

        public async Task<PromoCampaignDto> CreateAsync(int adminUserId, CreatePromoCampaignRequestDto req)
        {
            var campaign = new PromoCampaign
            {
                Name = req.Name, CampaignType = req.CampaignType, TargetAudience = req.TargetAudience,
                TargetCity = req.TargetCity, LinkedCouponCode = req.LinkedCouponCode, Priority = req.Priority,
                StartAt = req.StartAt, EndAt = req.EndAt, CreatedByUserId = adminUserId,
                Status = req.StartAt > DateTime.UtcNow ? "Scheduled" : "Draft",
            };
            _db.PromoCampaigns.Add(campaign);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(campaign);
        }

        public async Task<(bool Success, string Message, PromoVariantDto? Variant)> AddVariantAsync(AddPromoVariantRequestDto req)
        {
            var campaign = await _db.PromoCampaigns.FindAsync(req.PromoCampaignId);
            if (campaign == null) return (false, "Campaign not found.", null);
            var variant = new PromoCampaignVariant
            {
                PromoCampaignId = req.PromoCampaignId, VariantName = req.VariantName, ImageUrl = req.ImageUrl,
                HeadlineText = req.HeadlineText, SubText = req.SubText, DeepLink = req.DeepLink, Weight = req.Weight,
            };
            _db.PromoCampaignVariants.Add(variant);
            await _db.SaveChangesAsync();
            return (true, "Variant added.", ToVariantDto(variant));
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdatePromoCampaignStatusRequestDto req)
        {
            var valid = new[] { "Draft", "Scheduled", "Live", "Paused", "Ended" };
            if (!valid.Contains(req.Status)) return (false, $"Invalid status. Must be one of: {string.Join(", ", valid)}.");
            var campaign = await _db.PromoCampaigns.FindAsync(req.CampaignId);
            if (campaign == null) return (false, "Campaign not found.");
            if (req.Status == "Live" && await _db.PromoCampaignVariants.CountAsync(v => v.PromoCampaignId == campaign.Id && v.IsActive) == 0)
                return (false, "Cannot go Live — add at least one active variant first.");
            campaign.Status = req.Status;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"Campaign '{campaign.Name}' set to {req.Status}.");
        }

        public async Task<List<PromoCampaignDto>> GetAllAsync()
        {
            var campaigns = await _db.PromoCampaigns.Include(c => c.CreatedBy).OrderByDescending(c => c.CreatedAt).ToListAsync();
            var result = new List<PromoCampaignDto>();
            foreach (var c in campaigns) result.Add(await ToDtoAsync(c));
            return result;
        }

        // Picks one live, in-window, audience/city-matching campaign (highest
        // priority number = lowest wins ties broken by StartAt) and a
        // weighted-random variant from it — what CustomerApp/ChefApp calls to
        // decide what to show on open.
        public async Task<ServedPromoDto?> GetActiveAsync(ActivePromoRequestDto req)
        {
            var now = DateTime.UtcNow;
            var q = _db.PromoCampaigns.Include(c => c.Variants).Where(c => c.Status == "Live" && c.StartAt <= now && c.EndAt >= now);
            if (req.Audience != "All") q = q.Where(c => c.TargetAudience == "All" || c.TargetAudience == req.Audience);
            if (!string.IsNullOrEmpty(req.City)) q = q.Where(c => c.TargetCity == null || c.TargetCity == req.City);
            if (!string.IsNullOrEmpty(req.Placement)) q = q.Where(c => c.CampaignType == req.Placement);

            var campaign = await q.OrderBy(c => c.Priority).ThenBy(c => c.StartAt).FirstOrDefaultAsync();
            if (campaign == null) return null;
            var variants = campaign.Variants.Where(v => v.IsActive).ToList();
            if (variants.Count == 0) return null;

            var totalWeight = variants.Sum(v => v.Weight);
            var roll = _rng.Next(1, Math.Max(totalWeight, 1) + 1);
            var running = 0;
            var chosen = variants[0];
            foreach (var v in variants)
            {
                running += v.Weight;
                if (roll <= running) { chosen = v; break; }
            }

            return new ServedPromoDto
            {
                CampaignId = campaign.Id, VariantId = chosen.Id, CampaignType = campaign.CampaignType,
                ImageUrl = chosen.ImageUrl, HeadlineText = chosen.HeadlineText, SubText = chosen.SubText, DeepLink = chosen.DeepLink,
            };
        }

        public async Task<(bool Success, string Message)> RecordEventAsync(RecordPromoEventRequestDto req)
        {
            var variant = await _db.PromoCampaignVariants.FindAsync(req.VariantId);
            if (variant == null) return (false, "Variant not found.");

            if (req.EventType == "Click") variant.ClickCount += 1; else variant.ImpressionCount += 1;

            var dateKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var stat = await _db.PromoCampaignDailyStats.FirstOrDefaultAsync(s => s.PromoCampaignVariantId == variant.Id && s.DateKey == dateKey);
            if (stat == null)
            {
                stat = new PromoCampaignDailyStat { PromoCampaignId = variant.PromoCampaignId, PromoCampaignVariantId = variant.Id, DateKey = dateKey };
                _db.PromoCampaignDailyStats.Add(stat);
            }
            if (req.EventType == "Click") stat.Clicks += 1; else stat.Impressions += 1;

            await _db.SaveChangesAsync();
            return (true, "Recorded.");
        }

        public async Task<PromoStatsDto?> GetStatsAsync(int campaignId, int days = 14)
        {
            var campaign = await _db.PromoCampaigns.FindAsync(campaignId);
            if (campaign == null) return null;
            var since = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");
            var stats = await _db.PromoCampaignDailyStats.Where(s => s.PromoCampaignId == campaignId && string.Compare(s.DateKey, since) >= 0).ToListAsync();
            var byDay = stats.GroupBy(s => s.DateKey).OrderBy(g => g.Key)
                .Select(g => new PromoDailyStatDto { DateKey = g.Key, Impressions = g.Sum(x => x.Impressions), Clicks = g.Sum(x => x.Clicks) }).ToList();
            var totalImp = byDay.Sum(d => d.Impressions);
            var totalClk = byDay.Sum(d => d.Clicks);
            return new PromoStatsDto
            {
                CampaignId = campaignId, CampaignName = campaign.Name, TotalImpressions = totalImp, TotalClicks = totalClk,
                ClickThroughRate = totalImp == 0 ? 0 : Math.Round((decimal)totalClk / totalImp * 100, 2),
                DailyBreakdown = byDay,
            };
        }

        private async Task<PromoCampaignDto> ToDtoAsync(PromoCampaign c)
        {
            var variants = await _db.PromoCampaignVariants.Where(v => v.PromoCampaignId == c.Id).ToListAsync();
            var createdBy = c.CreatedBy ?? await _db.Users.FindAsync(c.CreatedByUserId);
            return new PromoCampaignDto
            {
                Id = c.Id, Name = c.Name, CampaignType = c.CampaignType, Status = c.Status, TargetAudience = c.TargetAudience,
                TargetCity = c.TargetCity, LinkedCouponCode = c.LinkedCouponCode, Priority = c.Priority,
                StartAt = c.StartAt, EndAt = c.EndAt, CreatedByName = createdBy?.FullName,
                Variants = variants.Select(ToVariantDto).ToList(),
                TotalImpressions = variants.Sum(v => v.ImpressionCount), TotalClicks = variants.Sum(v => v.ClickCount),
                CreatedAt = c.CreatedAt,
            };
        }

        private static PromoVariantDto ToVariantDto(PromoCampaignVariant v) => new()
        {
            Id = v.Id, VariantName = v.VariantName, ImageUrl = v.ImageUrl, HeadlineText = v.HeadlineText, SubText = v.SubText,
            DeepLink = v.DeepLink, Weight = v.Weight, ImpressionCount = v.ImpressionCount, ClickCount = v.ClickCount,
            ClickThroughRate = v.ImpressionCount == 0 ? 0 : Math.Round((decimal)v.ClickCount / v.ImpressionCount * 100, 2),
            IsActive = v.IsActive,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/cms-management"), Authorize(Roles = "Admin")]
    public class CmsManagementController : ControllerBase
    {
        private readonly Services.CmsManagementService _svc;
        public CmsManagementController(Services.CmsManagementService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("blocks/draft")]
        public async Task<IActionResult> SaveDraft([FromBody] SaveContentBlockDraftRequestDto req)
            => Ok(new { success = true, data = await _svc.SaveDraftAsync(UserId, req) });

        [HttpPost("blocks/publish")]
        public async Task<IActionResult> Publish([FromBody] PublishContentBlockRequestDto req)
        {
            var (success, message, block) = await _svc.PublishAsync(UserId, req.ContentBlockId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = block });
        }

        [HttpGet("blocks")]
        public async Task<IActionResult> GetAllBlocks([FromQuery] string? locale) => Ok(new { success = true, data = await _svc.GetAllBlocksAsync(locale) });

        [HttpGet("blocks/{id}/history")]
        public async Task<IActionResult> GetHistory(int id) => Ok(new { success = true, data = await _svc.GetHistoryAsync(id) });

        [HttpGet("pages/{pageId}/revisions")]
        public async Task<IActionResult> GetPageRevisions(int pageId) => Ok(new { success = true, data = await _svc.GetPageRevisionsAsync(pageId) });

        [HttpPost("pages")]
        public async Task<IActionResult> SavePage([FromBody] SaveCmsPageRequestDto req) => Ok(new { success = true, data = await _svc.SavePageWithRevisionAsync(UserId, req) });
    }

    [ApiController, Route("api/cms-management/content"), AllowAnonymous]
    public class CmsContentPublicController : ControllerBase
    {
        private readonly Services.CmsManagementService _svc;
        public CmsContentPublicController(Services.CmsManagementService svc) => _svc = svc;

        [HttpGet("{key}")]
        public async Task<IActionResult> GetPublished(string key, [FromQuery] string locale = "en")
        {
            var block = await _svc.GetPublishedAsync(key, locale);
            if (block == null) return NotFound(new { success = false, message = "No published content for this key/locale." });
            return Ok(new { success = true, data = block });
        }
    }

    [ApiController, Route("api/promo-campaigns"), Authorize(Roles = "Admin")]
    public class PromoCampaignController : ControllerBase
    {
        private readonly Services.PromoCampaignService _svc;
        public PromoCampaignController(Services.PromoCampaignService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePromoCampaignRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(UserId, req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("variants")]
        public async Task<IActionResult> AddVariant([FromBody] AddPromoVariantRequestDto req)
        {
            var (success, message, variant) = await _svc.AddVariantAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = variant });
        }

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdatePromoCampaignStatusRequestDto req)
        {
            var (success, message) = await _svc.UpdateStatusAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id, [FromQuery] int days = 14)
        {
            var stats = await _svc.GetStatsAsync(id, days);
            if (stats == null) return NotFound(new { success = false, message = "Campaign not found." });
            return Ok(new { success = true, data = stats });
        }
    }

    // Public/authenticated-user surface for CustomerApp/ChefApp to fetch and track promos.
    [ApiController, Route("api/promo")]
    public class PromoServingController : ControllerBase
    {
        private readonly Services.PromoCampaignService _svc;
        public PromoServingController(Services.PromoCampaignService svc) => _svc = svc;

        [HttpGet("active"), AllowAnonymous]
        public async Task<IActionResult> GetActive([FromQuery] string audience = "All", [FromQuery] string? city = null, [FromQuery] string? placement = null)
        {
            var promo = await _svc.GetActiveAsync(new ActivePromoRequestDto { Audience = audience, City = city, Placement = placement });
            return Ok(new { success = true, data = promo });
        }

        [HttpPost("event"), AllowAnonymous]
        public async Task<IActionResult> RecordEvent([FromBody] RecordPromoEventRequestDto req)
        {
            var (success, message) = await _svc.RecordEventAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
