using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    // ── M58: Device & Security Service ────────────────────────────
    public class DeviceSecurityService
    {
        private readonly AppDbContext _db;
        public DeviceSecurityService(AppDbContext db) => _db = db;

        public async Task<SecurityOverviewDto> GetOverviewAsync(int userId)
        {
            var sessions = await _db.UserSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastSeenAt)
                .ToListAsync();

            var events = await _db.SecurityEvents
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.OccurredAt)
                .Take(20)
                .ToListAsync();

            return new SecurityOverviewDto
            {
                ActiveSessions = sessions.Count(s => s.IsActive),
                TotalDevices   = sessions.Select(s => s.DeviceId).Distinct().Count(),
                HasAlerts      = events.Any(e => e.IsAlert),
                LastLogin      = sessions.FirstOrDefault()?.LoginAt.ToString("MMM dd, yyyy hh:mm tt") ?? "",
                Sessions = sessions.Select(s => new UserSessionDto
                {
                    Id=s.Id, DeviceName=s.DeviceName, DeviceType=s.DeviceType, AppVersion=s.AppVersion,
                    IpAddress=s.IpAddress, Location=s.Location, IsActive=s.IsActive, IsCurrent=s.IsCurrent,
                    LoginAt=s.LoginAt.ToString("MMM dd, yyyy hh:mm tt"),
                    LastSeenAt=s.LastSeenAt.ToString("MMM dd, yyyy hh:mm tt"),
                }).ToList(),
                Events = events.Select(e => new SecurityEventDto
                {
                    Id=e.Id, EventType=e.EventType, DeviceName=e.DeviceName, IpAddress=e.IpAddress,
                    Location=e.Location, IsAlert=e.IsAlert, OccurredAt=e.OccurredAt.ToString("MMM dd, yyyy hh:mm tt"),
                }).ToList(),
            };
        }

        public async Task<bool> LogoutSessionAsync(int sessionId, int userId)
        {
            var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
            if (session == null) return false;
            session.IsActive    = false;
            session.LoggedOutAt = DateTime.UtcNow;
            _db.SecurityEvents.Add(new SecurityEvent { UserId=userId, EventType="ForcedLogout", DeviceName=session.DeviceName, IpAddress=session.IpAddress });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task LogoutAllAsync(int userId)
        {
            var sessions = await _db.UserSessions.Where(s => s.UserId == userId && s.IsActive && !s.IsCurrent).ToListAsync();
            sessions.ForEach(s => { s.IsActive=false; s.LoggedOutAt=DateTime.UtcNow; });
            _db.SecurityEvents.Add(new SecurityEvent { UserId=userId, EventType="LogoutAll", DeviceName="All devices", IsAlert=false });
            await _db.SaveChangesAsync();
        }

        public async Task RecordSessionAsync(int userId, string deviceId, string deviceName, string deviceType, string appVersion, string? ip, string? location)
        {
            var existing = await _db.UserSessions.FirstOrDefaultAsync(s => s.UserId==userId && s.DeviceId==deviceId && s.IsActive);
            if (existing != null) { existing.LastSeenAt=DateTime.UtcNow; }
            else
            {
                var isNew = !await _db.UserSessions.AnyAsync(s => s.UserId==userId && s.DeviceId==deviceId);
                _db.UserSessions.Add(new UserSession { UserId=userId, DeviceId=deviceId, DeviceName=deviceName, DeviceType=deviceType, AppVersion=appVersion, IpAddress=ip, Location=location, IsCurrent=true });
                if (isNew)
                    _db.SecurityEvents.Add(new SecurityEvent { UserId=userId, EventType="NewDevice", DeviceName=deviceName, IpAddress=ip, Location=location, IsAlert=true });
                else
                    _db.SecurityEvents.Add(new SecurityEvent { UserId=userId, EventType="Login", DeviceName=deviceName, IpAddress=ip, Location=location });
            }
            await _db.SaveChangesAsync();
        }
    }

    // ── M59: Corporate Plans Service ──────────────────────────────
    public class CorporatePlansService
    {
        private readonly AppDbContext _db;
        public CorporatePlansService(AppDbContext db) => _db = db;

        public async Task<List<CorporatePlanDto>> GetPlansAsync()
        {
            var plans = await _db.CorporatePlans.Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToListAsync();
            return plans.Select(Map).ToList();
        }

        public async Task<CorporateSubscriptionDto?> GetSubscriptionAsync(int companyId)
        {
            var sub = await _db.CorporateSubscriptions
                .Include(s => s.Plan)
                .Include(s => s.BillingRecords)
                .FirstOrDefaultAsync(s => s.CompanyId == companyId);
            return sub == null ? null : MapSub(sub);
        }

        public async Task<CorporateSubscriptionDto> CreateSubscriptionAsync(int companyId, int planId, dynamic req)
        {
            var plan = await _db.CorporatePlans.FindAsync(planId);
            if (plan == null) throw new Exception("Plan not found");
            var sub = new CorporateSubscription
            {
                CompanyId=companyId, CorporatePlanId=planId,
                CompanyName=(string)(req.companyName??""), ContactName=(string)(req.contactName??""),
                ContactPhone=(string)(req.contactPhone??""), ContactEmail=(string)(req.contactEmail??""),
                GSTNumber=(string)(req.gstNumber??""), BillingAddress=(string)(req.billingAddress??""),
                EmployeeCount=(int)(req.employeeCount??0),
                MonthlyAmount=plan.MonthlyPrice,
                NextBillingDate=DateTime.UtcNow.AddMonths(1),
            };
            _db.CorporateSubscriptions.Add(sub);
            // First billing record
            _db.CorporateBillingRecords.Add(new CorporateBillingRecord { Subscription=sub, Amount=plan.MonthlyPrice, Status="Pending", InvoiceNumber=$"CORP-{DateTime.UtcNow:yyyyMM}-001", Period=DateTime.UtcNow.ToString("MMM yyyy"), BilledAt=DateTime.UtcNow });
            await _db.SaveChangesAsync();
            return MapSub(sub);
        }

        public async Task<bool> UpgradePlanAsync(int subscriptionId, UpgradePlanDto dto)
        {
            var sub = await _db.CorporateSubscriptions.Include(s=>s.Plan).FirstOrDefaultAsync(s=>s.Id==subscriptionId);
            var newPlan = await _db.CorporatePlans.FindAsync(dto.NewPlanId);
            if (sub==null||newPlan==null) return false;
            sub.CorporatePlanId = dto.NewPlanId;
            sub.MonthlyAmount   = newPlan.MonthlyPrice;
            sub.Status          = "Active";
            sub.UpdatedAt       = DateTime.UtcNow;
            _db.CorporateBillingRecords.Add(new CorporateBillingRecord { SubscriptionId=subscriptionId, Amount=newPlan.MonthlyPrice, Status="Pending", InvoiceNumber=$"CORP-{DateTime.UtcNow:yyyyMM}-UPG", Period=DateTime.UtcNow.ToString("MMM yyyy"), BilledAt=DateTime.UtcNow });
            await _db.SaveChangesAsync();
            return true;
        }

        private static CorporatePlanDto Map(CorporatePlan p) => new()
        {
            Id=p.Id, PlanName=p.PlanName, MealsPerDay=p.MealsPerDay, PricePerMeal=p.PricePerMeal,
            MonthlyPrice=p.MonthlyPrice, IsActive=p.IsActive, IsPopular=p.PlanName=="Premium",
            Features=JsonSerializer.Deserialize<List<string>>(p.Features) ?? new(),
        };

        private static CorporateSubscriptionDto MapSub(CorporateSubscription s) => new()
        {
            Id=s.Id, CompanyName=s.CompanyName, ContactName=s.ContactName, ContactPhone=s.ContactPhone,
            ContactEmail=s.ContactEmail, GSTNumber=s.GSTNumber, PlanName=s.Plan?.PlanName ?? "",
            MealsPerDay=s.Plan?.MealsPerDay ?? 0, MonthlyAmount=s.MonthlyAmount, TotalPaid=s.TotalPaid,
            Status=s.Status, StartDate=s.StartDate.ToString("MMM dd, yyyy"),
            NextBillingDate=s.NextBillingDate?.ToString("MMM dd, yyyy"),
            BillingHistory=s.BillingRecords?.OrderByDescending(b=>b.BilledAt).Select(b=>new CorporateBillingDto
                { Id=b.Id, Amount=b.Amount, Status=b.Status, InvoiceNumber=b.InvoiceNumber, Period=b.Period,
                  BilledAt=b.BilledAt.ToString("MMM dd, yyyy"), PaidAt=b.PaidAt?.ToString("MMM dd, yyyy") }).ToList() ?? new(),
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/security"), Authorize]
    public class DeviceSecurityController : ControllerBase
    {
        private readonly Services.DeviceSecurityService _svc;
        public DeviceSecurityController(Services.DeviceSecurityService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("overview")]
        public async Task<IActionResult> Overview()
            => Ok(new { success=true, data=await _svc.GetOverviewAsync(UserId) });

        [HttpPost("logout-session/{id}")]
        public async Task<IActionResult> LogoutSession(int id)
            => Ok(new { success=await _svc.LogoutSessionAsync(id, UserId) });

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            await _svc.LogoutAllAsync(UserId);
            return Ok(new { success=true, message="All other sessions logged out" });
        }
    }

    [ApiController, Route("api/corporate-plans")]
    public class CorporatePlansController : ControllerBase
    {
        private readonly Services.CorporatePlansService _svc;
        public CorporatePlansController(Services.CorporatePlansService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetPlans()
            => Ok(new { success=true, data=await _svc.GetPlansAsync() });

        [HttpGet("my"), Authorize]
        public async Task<IActionResult> GetMy()
            => Ok(new { success=true, data=await _svc.GetSubscriptionAsync(UserId) });

        [HttpPost("subscribe/{planId}"), Authorize]
        public async Task<IActionResult> Subscribe(int planId, [FromBody] dynamic req)
            => Ok(new { success=true, data=await _svc.CreateSubscriptionAsync(UserId, planId, req) });

        [HttpPost("upgrade/{subscriptionId}"), Authorize]
        public async Task<IActionResult> Upgrade(int subscriptionId, [FromBody] UpgradePlanDto dto)
            => Ok(new { success=await _svc.UpgradePlanAsync(subscriptionId, dto) });
    }
}
