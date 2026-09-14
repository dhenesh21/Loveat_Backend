using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M141: Chef Business Suite — Staff/Team Management ───────────
    public class StaffManagementService
    {
        private readonly AppDbContext _db;
        public StaffManagementService(AppDbContext db) => _db = db;

        public async Task<StaffMemberDto> CreateAsync(CreateStaffMemberRequestDto req)
        {
            var staff = new StaffMember { ChefId = req.ChefId, Name = req.Name, Role = req.Role, Phone = req.Phone, HourlyWage = req.HourlyWage };
            _db.StaffMembers.Add(staff);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(staff);
        }

        public async Task<List<StaffMemberDto>> GetForChefAsync(int chefId)
        {
            var staff = await _db.StaffMembers.Where(s => s.ChefId == chefId).ToListAsync();
            var result = new List<StaffMemberDto>();
            foreach (var s in staff) result.Add(await ToDtoAsync(s));
            return result;
        }

        public async Task<(bool Success, string Message, StaffShiftLogDto? Log)> LogShiftAsync(LogStaffShiftRequestDto req)
        {
            var staff = await _db.StaffMembers.FindAsync(req.StaffMemberId);
            if (staff == null) return (false, "Staff member not found.", null);
            var wage = Math.Round(req.HoursWorked * staff.HourlyWage, 2);
            var log = new StaffShiftLog { StaffMemberId = req.StaffMemberId, ShiftDate = req.ShiftDate, HoursWorked = req.HoursWorked, WagePaid = wage, Notes = req.Notes };
            _db.StaffShiftLogs.Add(log);
            await _db.SaveChangesAsync();
            return (true, "Shift logged.", new StaffShiftLogDto { ShiftDate = log.ShiftDate, HoursWorked = log.HoursWorked, WagePaid = log.WagePaid, Notes = log.Notes });
        }

        public async Task<List<StaffShiftLogDto>> GetShiftHistoryAsync(int staffMemberId)
            => (await _db.StaffShiftLogs.Where(l => l.StaffMemberId == staffMemberId).OrderByDescending(l => l.ShiftDate).ToListAsync())
                .Select(l => new StaffShiftLogDto { ShiftDate = l.ShiftDate, HoursWorked = l.HoursWorked, WagePaid = l.WagePaid, Notes = l.Notes }).ToList();

        public async Task<(bool Success, string Message)> DeactivateAsync(int staffMemberId)
        {
            var staff = await _db.StaffMembers.FindAsync(staffMemberId);
            if (staff == null) return (false, "Staff member not found.");
            staff.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Staff member deactivated.");
        }

        private async Task<StaffMemberDto> ToDtoAsync(StaffMember s)
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var thisMonth = await _db.StaffShiftLogs.Where(l => l.StaffMemberId == s.Id && l.ShiftDate >= monthStart).ToListAsync();
            return new StaffMemberDto
            {
                Id = s.Id, Name = s.Name, Role = s.Role, Phone = s.Phone, HourlyWage = s.HourlyWage, IsActive = s.IsActive,
                TotalHoursThisMonth = thisMonth.Sum(l => l.HoursWorked), TotalWagesThisMonth = thisMonth.Sum(l => l.WagePaid),
            };
        }
    }

    // ── M142: Chef Business Suite — Shift Scheduling ────────────────
    public class ChefShiftService
    {
        private readonly AppDbContext _db;
        public ChefShiftService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, ChefShiftDto? Shift)> CreateAsync(CreateShiftRequestDto req)
        {
            if (!TimeSpan.TryParse(req.StartTime, out var start) || !TimeSpan.TryParse(req.EndTime, out var end))
                return (false, "StartTime/EndTime must be in HH:mm format.", null);
            if (end <= start) return (false, "EndTime must be after StartTime.", null);
            if (!req.IsRecurring && req.SpecificDate == null) return (false, "SpecificDate is required for a non-recurring shift.", null);

            var shift = new ChefShift
            {
                ChefId = req.ChefId, DayOfWeek = req.DayOfWeek, IsRecurring = req.IsRecurring, SpecificDate = req.SpecificDate,
                StartTime = start, EndTime = end, MaxBookingsThisShift = req.MaxBookingsThisShift,
            };
            _db.ChefShifts.Add(shift);
            await _db.SaveChangesAsync();
            return (true, "Shift created.", ToDto(shift));
        }

        public async Task<List<ChefShiftDto>> GetForChefAsync(int chefId)
            => (await _db.ChefShifts.Where(s => s.ChefId == chefId && s.Status != "Cancelled").OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToListAsync()).Select(ToDto).ToList();

        public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdateShiftStatusRequestDto req)
        {
            var valid = new[] { "Active", "Skipped", "Cancelled" };
            if (!valid.Contains(req.Status)) return (false, $"Invalid status. Must be one of: {string.Join(", ", valid)}.");
            var shift = await _db.ChefShifts.FindAsync(req.ShiftId);
            if (shift == null) return (false, "Shift not found.");
            shift.Status = req.Status;
            await _db.SaveChangesAsync();
            return (true, $"Shift set to {req.Status}.");
        }

        private static ChefShiftDto ToDto(ChefShift s) => new()
        {
            Id = s.Id, DayOfWeek = s.DayOfWeek, IsRecurring = s.IsRecurring, SpecificDate = s.SpecificDate,
            StartTime = s.StartTime.ToString(@"hh\:mm"), EndTime = s.EndTime.ToString(@"hh\:mm"), MaxBookingsThisShift = s.MaxBookingsThisShift, Status = s.Status,
        };
    }

    // ── M143: Chef Business Suite — Service Packages ─────────────────
    public class ServicePackageService
    {
        private readonly AppDbContext _db;
        public ServicePackageService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, ServicePackageDto? Package)> CreateAsync(CreateServicePackageRequestDto req)
        {
            if (req.Items.Count == 0) return (false, "A package needs at least one menu item.", null);
            var menuItemIds = req.Items.Select(i => i.ChefMenuItemId).ToList();
            var validCount = await _db.ChefMenuItems.CountAsync(m => menuItemIds.Contains(m.Id) && m.ChefProfile != null && m.ChefProfile.UserId == req.ChefId);
            if (validCount != menuItemIds.Distinct().Count()) return (false, "One or more menu items don't belong to this chef.", null);

            var package = new ServicePackage { ChefId = req.ChefId, PackageName = req.PackageName, Description = req.Description, Price = req.Price, ValidityDays = req.ValidityDays };
            _db.ServicePackages.Add(package);
            await _db.SaveChangesAsync();

            foreach (var item in req.Items)
                _db.PackageMenuItems.Add(new PackageMenuItem { ServicePackageId = package.Id, ChefMenuItemId = item.ChefMenuItemId, Quantity = item.Quantity });
            await _db.SaveChangesAsync();

            return (true, "Package created.", await ToDtoAsync(package));
        }

        public async Task<List<ServicePackageDto>> GetForChefAsync(int chefId)
        {
            var packages = await _db.ServicePackages.Where(p => p.ChefId == chefId).ToListAsync();
            var result = new List<ServicePackageDto>();
            foreach (var p in packages) result.Add(await ToDtoAsync(p));
            return result;
        }

        public async Task<(bool Success, string Message)> DeactivateAsync(int servicePackageId)
        {
            var package = await _db.ServicePackages.FindAsync(servicePackageId);
            if (package == null) return (false, "Package not found.");
            package.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Package deactivated.");
        }

        private async Task<ServicePackageDto> ToDtoAsync(ServicePackage p)
        {
            var items = await _db.PackageMenuItems.Include(i => i.ChefMenuItem).Where(i => i.ServicePackageId == p.Id).ToListAsync();
            return new ServicePackageDto
            {
                Id = p.Id, ChefId = p.ChefId, PackageName = p.PackageName, Description = p.Description, Price = p.Price,
                ValidityDays = p.ValidityDays, IsActive = p.IsActive,
                Items = items.Select(i => new PackageItemDto { ChefMenuItemId = i.ChefMenuItemId, DishName = i.ChefMenuItem?.DishName, Quantity = i.Quantity }).ToList(),
            };
        }
    }

    // ── M144: Chef Business Suite — Marketing / Promo Codes ──────────
    public class ChefPromoCodeService
    {
        private readonly AppDbContext _db;
        public ChefPromoCodeService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, ChefPromoCodeDto? Promo)> CreateAsync(CreateChefPromoCodeRequestDto req)
        {
            var code = req.Code.Trim().ToUpperInvariant();
            if (await _db.ChefPromoCodes.AnyAsync(p => p.ChefId == req.ChefId && p.Code == code))
                return (false, "You already have a promo code with this name.", null);
            if (req.DiscountPercent <= 0 || req.DiscountPercent > 90) return (false, "DiscountPercent must be between 1 and 90.", null);

            var promo = new ChefPromoCode { ChefId = req.ChefId, Code = code, DiscountPercent = req.DiscountPercent, MaxUses = req.MaxUses, ValidTo = req.ValidTo };
            _db.ChefPromoCodes.Add(promo);
            await _db.SaveChangesAsync();
            return (true, "Promo code created.", ToDto(promo));
        }

        public async Task<List<ChefPromoCodeDto>> GetForChefAsync(int chefId)
            => (await _db.ChefPromoCodes.Where(p => p.ChefId == chefId).OrderByDescending(p => p.CreatedAt).ToListAsync()).Select(ToDto).ToList();

        // Checks the code is active, within its validity window, and under
        // its max-use cap before computing the discount — does not consume
        // a use (that happens explicitly via RedeemAsync at booking time).
        public async Task<ChefPromoValidationResultDto> ValidateAsync(ValidateChefPromoRequestDto req)
        {
            var promo = await _db.ChefPromoCodes.FirstOrDefaultAsync(p => p.ChefId == req.ChefId && p.Code == req.Code.Trim().ToUpperInvariant());
            if (promo == null) return new ChefPromoValidationResultDto { IsValid = false, Reason = "Promo code not found." };
            if (!promo.IsActive) return new ChefPromoValidationResultDto { IsValid = false, Reason = "This code is no longer active." };
            var now = DateTime.UtcNow;
            if (now < promo.ValidFrom || now > promo.ValidTo) return new ChefPromoValidationResultDto { IsValid = false, Reason = "This code has expired." };
            if (promo.MaxUses.HasValue && promo.UsedCount >= promo.MaxUses) return new ChefPromoValidationResultDto { IsValid = false, Reason = "This code has reached its usage limit." };

            var discount = Math.Round(req.BookingAmount * promo.DiscountPercent / 100, 2);
            return new ChefPromoValidationResultDto { IsValid = true, DiscountAmount = discount, FinalAmount = req.BookingAmount - discount };
        }

        public async Task<(bool Success, string Message)> RedeemAsync(RedeemChefPromoRequestDto req)
        {
            var promo = await _db.ChefPromoCodes.FindAsync(req.ChefPromoCodeId);
            if (promo == null) return (false, "Promo code not found.");
            if (promo.MaxUses.HasValue && promo.UsedCount >= promo.MaxUses) return (false, "This code has reached its usage limit.");

            _db.ChefPromoRedemptions.Add(new ChefPromoRedemption { ChefPromoCodeId = req.ChefPromoCodeId, CustomerId = req.CustomerId, BookingId = req.BookingId, DiscountAmount = req.DiscountAmount });
            promo.UsedCount += 1;
            await _db.SaveChangesAsync();
            return (true, "Redeemed.");
        }

        private static ChefPromoCodeDto ToDto(ChefPromoCode p) => new()
        {
            Id = p.Id, Code = p.Code, DiscountPercent = p.DiscountPercent, MaxUses = p.MaxUses, UsedCount = p.UsedCount,
            ValidFrom = p.ValidFrom, ValidTo = p.ValidTo, IsActive = p.IsActive,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/staff-management")]
    public class StaffManagementController : ControllerBase
    {
        private readonly Services.StaffManagementService _svc;
        public StaffManagementController(Services.StaffManagementService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateStaffMemberRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet("chef/{chefId}"), Authorize]
        public async Task<IActionResult> GetForChef(int chefId) => Ok(new { success = true, data = await _svc.GetForChefAsync(chefId) });

        [HttpPost("shifts"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> LogShift([FromBody] LogStaffShiftRequestDto req)
        {
            var (success, message, log) = await _svc.LogShiftAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = log });
        }

        [HttpGet("{staffMemberId}/shifts"), Authorize]
        public async Task<IActionResult> GetShiftHistory(int staffMemberId) => Ok(new { success = true, data = await _svc.GetShiftHistoryAsync(staffMemberId) });

        [HttpPost("deactivate"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Deactivate([FromBody] DeactivateStaffMemberRequestDto req)
        {
            var (success, message) = await _svc.DeactivateAsync(req.StaffMemberId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/chef-shifts")]
    public class ChefShiftController : ControllerBase
    {
        private readonly Services.ChefShiftService _svc;
        public ChefShiftController(Services.ChefShiftService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateShiftRequestDto req)
        {
            var (success, message, shift) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = shift });
        }

        [HttpGet("chef/{chefId}")]
        public async Task<IActionResult> GetForChef(int chefId) => Ok(new { success = true, data = await _svc.GetForChefAsync(chefId) });

        [HttpPost("status"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateShiftStatusRequestDto req)
        {
            var (success, message) = await _svc.UpdateStatusAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/service-packages")]
    public class ServicePackageController : ControllerBase
    {
        private readonly Services.ServicePackageService _svc;
        public ServicePackageController(Services.ServicePackageService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateServicePackageRequestDto req)
        {
            var (success, message, package) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = package });
        }

        [HttpGet("chef/{chefId}")]
        public async Task<IActionResult> GetForChef(int chefId) => Ok(new { success = true, data = await _svc.GetForChefAsync(chefId) });

        [HttpPost("deactivate"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Deactivate([FromBody] DeactivatePackageRequestDto req)
        {
            var (success, message) = await _svc.DeactivateAsync(req.ServicePackageId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/chef-promo-codes")]
    public class ChefPromoCodeController : ControllerBase
    {
        private readonly Services.ChefPromoCodeService _svc;
        public ChefPromoCodeController(Services.ChefPromoCodeService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateChefPromoCodeRequestDto req)
        {
            var (success, message, promo) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = promo });
        }

        [HttpGet("chef/{chefId}"), Authorize]
        public async Task<IActionResult> GetForChef(int chefId) => Ok(new { success = true, data = await _svc.GetForChefAsync(chefId) });

        [HttpPost("validate"), Authorize]
        public async Task<IActionResult> Validate([FromBody] ValidateChefPromoRequestDto req) => Ok(new { success = true, data = await _svc.ValidateAsync(req) });

        [HttpPost("redeem"), Authorize]
        public async Task<IActionResult> Redeem([FromBody] RedeemChefPromoRequestDto req)
        {
            var (success, message) = await _svc.RedeemAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
