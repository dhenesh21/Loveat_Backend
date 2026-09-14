using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    public class AuditService
    {
        private readonly AppDbContext _db;
        public AuditService(AppDbContext db) => _db = db;

        public async Task LogAsync(int adminId, string action, string module, string entityType = "", int? entityId = null, string? oldValues = null, string? newValues = null, string? ip = null)
        {
            _db.AuditLogs.Add(new AuditLog { AdminUserId=adminId, Action=action, Module=module, EntityType=entityType, EntityId=entityId, OldValues=oldValues, NewValues=newValues, IpAddress=ip });
            await _db.SaveChangesAsync();
        }

        public async Task<List<AuditLogDto>> GetLogsAsync(string? module = null, string? action = null, int days = 30)
        {
            var from = DateTime.UtcNow.AddDays(-days);
            var q    = _db.AuditLogs.Include(l => l.AdminUser).Where(l => l.CreatedAt >= from);
            if (module != null) q = q.Where(l => l.Module == module);
            if (action != null) q = q.Where(l => l.Action == action);
            var list = await q.OrderByDescending(l => l.CreatedAt).Take(200).ToListAsync();
            return list.Select(l => new AuditLogDto { Id=l.Id, AdminName=l.AdminUser?.FullName ?? "System", Action=l.Action, Module=l.Module, EntityType=l.EntityType, EntityId=l.EntityId, OldValues=l.OldValues, NewValues=l.NewValues, IpAddress=l.IpAddress, CreatedAt=l.CreatedAt.ToString("MMM dd, yyyy hh:mm tt") }).ToList();
        }
    }

    public class CmsService
    {
        private readonly AppDbContext _db;
        public CmsService(AppDbContext db) => _db = db;

        public async Task<List<CmsBannerDto>> GetBannersAsync(string? target = null)
        {
            var q = _db.CmsBanners.Where(b => b.IsActive);
            if (target != null) q = q.Where(b => b.Target == target || b.Target == "All");
            return (await q.OrderBy(b => b.SortOrder).ToListAsync()).Select(MapBanner).ToList();
        }

        public async Task<List<CmsBannerDto>> GetAllBannersAsync()
            => (await _db.CmsBanners.OrderBy(b => b.SortOrder).ToListAsync()).Select(MapBanner).ToList();

        public async Task<CmsBannerDto> SaveBannerAsync(CmsBannerDto dto)
        {
            if (dto.Id == 0)
            {
                var b = new CmsBanner { Title=dto.Title, Subtitle=dto.Subtitle, ImageUrl=dto.ImageUrl, DeepLink=dto.DeepLink, Target=dto.Target, SortOrder=dto.SortOrder, IsActive=dto.IsActive, ValidFrom=dto.ValidFrom, ValidTo=dto.ValidTo };
                _db.CmsBanners.Add(b); await _db.SaveChangesAsync(); dto.Id = b.Id;
            }
            else
            {
                var b = await _db.CmsBanners.FindAsync(dto.Id);
                if (b != null) { b.Title=dto.Title; b.Subtitle=dto.Subtitle; b.ImageUrl=dto.ImageUrl; b.DeepLink=dto.DeepLink; b.Target=dto.Target; b.SortOrder=dto.SortOrder; b.IsActive=dto.IsActive; b.ValidFrom=dto.ValidFrom; b.ValidTo=dto.ValidTo; await _db.SaveChangesAsync(); }
            }
            return dto;
        }

        public async Task<bool> DeleteBannerAsync(int id) { var b = await _db.CmsBanners.FindAsync(id); if (b==null) return false; _db.CmsBanners.Remove(b); await _db.SaveChangesAsync(); return true; }

        public async Task<CmsPageDto?> GetPageAsync(string slug)
        {
            var p = await _db.CmsPages.FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
            return p == null ? null : new CmsPageDto { Id=p.Id, Slug=p.Slug, Title=p.Title, Content=p.Content, IsActive=p.IsActive, UpdatedAt=p.UpdatedAt.ToString("MMM dd, yyyy") };
        }

        public async Task<List<CmsPageDto>> GetAllPagesAsync()
            => (await _db.CmsPages.ToListAsync()).Select(p => new CmsPageDto { Id=p.Id, Slug=p.Slug, Title=p.Title, Content=p.Content, IsActive=p.IsActive, UpdatedAt=p.UpdatedAt.ToString("MMM dd, yyyy") }).ToList();

        public async Task<CmsPageDto> SavePageAsync(CmsPageDto dto)
        {
            var p = await _db.CmsPages.FirstOrDefaultAsync(p => p.Slug == dto.Slug);
            if (p == null) { p = new CmsPage { Slug=dto.Slug }; _db.CmsPages.Add(p); }
            p.Title=dto.Title; p.Content=dto.Content; p.IsActive=dto.IsActive; p.UpdatedAt=DateTime.UtcNow;
            await _db.SaveChangesAsync(); dto.Id=p.Id; return dto;
        }

        private static CmsBannerDto MapBanner(CmsBanner b) => new() { Id=b.Id, Title=b.Title, Subtitle=b.Subtitle, ImageUrl=b.ImageUrl, DeepLink=b.DeepLink, Target=b.Target, SortOrder=b.SortOrder, IsActive=b.IsActive, ValidFrom=b.ValidFrom, ValidTo=b.ValidTo };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/audit"), Authorize(Roles="Admin")]
    public class AuditController : ControllerBase
    {
        private readonly Services.AuditService _svc;
        public AuditController(Services.AuditService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] string? module, [FromQuery] string? action, [FromQuery] int days = 30)
        {
            var data = await _svc.GetLogsAsync(module, action, days);
            return Ok(new { success=true, data });
        }
    }

    [ApiController, Route("api/cms")]
    public class CmsController : ControllerBase
    {
        private readonly Services.CmsService _svc;
        public CmsController(Services.CmsService svc) => _svc = svc;

        [HttpGet("banners"), AllowAnonymous]
        public async Task<IActionResult> GetBanners([FromQuery] string? target)
            => Ok(new { success=true, data=await _svc.GetBannersAsync(target) });

        [HttpGet("banners/all"), Authorize(Roles="Admin")]
        public async Task<IActionResult> AllBanners()
            => Ok(new { success=true, data=await _svc.GetAllBannersAsync() });

        [HttpPost("banners"), Authorize(Roles="Admin")]
        public async Task<IActionResult> SaveBanner([FromBody] CmsBannerDto dto)
            => Ok(new { success=true, data=await _svc.SaveBannerAsync(dto) });

        [HttpDelete("banners/{id}"), Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
            => Ok(new { success=await _svc.DeleteBannerAsync(id) });

        [HttpGet("pages/{slug}"), AllowAnonymous]
        public async Task<IActionResult> GetPage(string slug)
        {
            var page = await _svc.GetPageAsync(slug);
            if (page == null) return NotFound();
            return Ok(new { success=true, data=page });
        }

        [HttpGet("pages"), Authorize(Roles="Admin")]
        public async Task<IActionResult> AllPages()
            => Ok(new { success=true, data=await _svc.GetAllPagesAsync() });

        [HttpPost("pages"), Authorize(Roles="Admin")]
        public async Task<IActionResult> SavePage([FromBody] CmsPageDto dto)
            => Ok(new { success=true, data=await _svc.SavePageAsync(dto) });
    }
}
