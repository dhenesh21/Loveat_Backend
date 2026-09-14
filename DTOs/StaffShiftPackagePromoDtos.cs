namespace LovEat.API.DTOs
{
    // ── M141: Chef Business Suite — Staff/Team Management ───────────
    public class CreateStaffMemberRequestDto
    {
        public int ChefId { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "Assistant";
        public string? Phone { get; set; }
        public decimal HourlyWage { get; set; } = 0;
    }

    public class StaffMemberDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string? Phone { get; set; }
        public decimal HourlyWage { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalHoursThisMonth { get; set; }
        public decimal TotalWagesThisMonth { get; set; }
    }

    public class LogStaffShiftRequestDto
    {
        public int StaffMemberId { get; set; }
        public DateTime ShiftDate { get; set; }
        public decimal HoursWorked { get; set; }
        public string? Notes { get; set; }
    }

    public class StaffShiftLogDto
    {
        public DateTime ShiftDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal WagePaid { get; set; }
        public string? Notes { get; set; }
    }

    public class DeactivateStaffMemberRequestDto { public int StaffMemberId { get; set; } }

    // ── M142: Chef Business Suite — Shift Scheduling ─────────────────
    public class CreateShiftRequestDto
    {
        public int ChefId { get; set; }
        public int DayOfWeek { get; set; }
        public bool IsRecurring { get; set; } = true;
        public DateTime? SpecificDate { get; set; }
        public string StartTime { get; set; } = "09:00"; // HH:mm
        public string EndTime { get; set; } = "17:00";
        public int? MaxBookingsThisShift { get; set; }
    }

    public class ChefShiftDto
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime? SpecificDate { get; set; }
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public int? MaxBookingsThisShift { get; set; }
        public string Status { get; set; } = "";
    }

    public class UpdateShiftStatusRequestDto { public int ShiftId { get; set; } public string Status { get; set; } = "Active"; }

    // ── M143: Chef Business Suite — Service Packages ─────────────────
    public class PackageItemRequestDto { public int ChefMenuItemId { get; set; } public int Quantity { get; set; } = 1; }

    public class CreateServicePackageRequestDto
    {
        public int ChefId { get; set; }
        public string PackageName { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int ValidityDays { get; set; } = 30;
        public List<PackageItemRequestDto> Items { get; set; } = new();
    }

    public class PackageItemDto
    {
        public int ChefMenuItemId { get; set; }
        public string? DishName { get; set; }
        public int Quantity { get; set; }
    }

    public class ServicePackageDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string PackageName { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int ValidityDays { get; set; }
        public bool IsActive { get; set; }
        public List<PackageItemDto> Items { get; set; } = new();
    }

    public class DeactivatePackageRequestDto { public int ServicePackageId { get; set; } }

    // ── M144: Chef Business Suite — Marketing / Promo Codes ──────────
    public class CreateChefPromoCodeRequestDto
    {
        public int ChefId { get; set; }
        public string Code { get; set; } = "";
        public decimal DiscountPercent { get; set; }
        public int? MaxUses { get; set; }
        public DateTime ValidTo { get; set; }
    }

    public class ChefPromoCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public decimal DiscountPercent { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class ValidateChefPromoRequestDto
    {
        public int ChefId { get; set; }
        public string Code { get; set; } = "";
        public decimal BookingAmount { get; set; }
    }

    public class ChefPromoValidationResultDto
    {
        public bool IsValid { get; set; }
        public string? Reason { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class RedeemChefPromoRequestDto
    {
        public int ChefPromoCodeId { get; set; }
        public int CustomerId { get; set; }
        public int? BookingId { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
