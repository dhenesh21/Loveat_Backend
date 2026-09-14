using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M141: Chef Business Suite — Staff/Team Management ──────────
    // A chef running a bigger operation (event catering, multiple daily
    // orders) can have helper staff — assistants, delivery helpers, sous
    // chefs — tracked here for payroll/hours, separate from the platform's
    // own chef accounts.
    public class StaffMember
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(80)] public string Name { get; set; } = "";
        [MaxLength(30)] public string Role { get; set; } = "Assistant"; // Assistant / DeliveryHelper / SousChef / Cleaner
        [MaxLength(15)] public string? Phone { get; set; }
        public decimal HourlyWage { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class StaffShiftLog
    {
        [Key] public int Id { get; set; }
        public int StaffMemberId { get; set; }
        public StaffMember? StaffMember { get; set; }
        public DateTime ShiftDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal WagePaid { get; set; }
        [MaxLength(200)] public string? Notes { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M142: Chef Business Suite — Shift Scheduling ────────────────
    // Structured weekly working-hour blocks — distinct from the existing
    // simple on/off toggle in AvailabilityService (M17), which just flags
    // "visible in search right now." This is the chef's actual weekly
    // schedule, usable for capacity planning and for staff shift assignment.
    public class ChefShift
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public int DayOfWeek { get; set; } // 0=Sun..6=Sat, used when IsRecurring
        public bool IsRecurring { get; set; } = true;
        public DateTime? SpecificDate { get; set; } // set when IsRecurring = false (one-off shift)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? MaxBookingsThisShift { get; set; } // optional capacity cap
        [MaxLength(20)] public string Status { get; set; } = "Active"; // Active / Skipped / Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M143: Chef Business Suite — Service Packages / Bundles ──────
    // A bundled offering combining several ChefMenuItems at one package
    // price — e.g. "Weekly Meal Plan" or "Wedding Combo" — rather than a
    // customer booking each dish separately.
    public class ServicePackage
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(100)] public string PackageName { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int ValidityDays { get; set; } = 30; // how long a purchased package stays redeemable
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PackageMenuItem
    {
        [Key] public int Id { get; set; }
        public int ServicePackageId { get; set; }
        public ServicePackage? ServicePackage { get; set; }
        public int ChefMenuItemId { get; set; }
        public ChefMenuItem? ChefMenuItem { get; set; }
        public int Quantity { get; set; } = 1;
    }

    // ── M144: Chef Business Suite — Marketing / Promo Codes ─────────
    // Chef-level discount codes the chef sets up themselves for their own
    // repeat customers — distinct from the platform-wide M90 PromoCampaign,
    // which is run centrally by Super Admin.
    public class ChefPromoCode
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(20)] public string Code { get; set; } = "";
        public decimal DiscountPercent { get; set; }
        public int? MaxUses { get; set; } // null = unlimited
        public int UsedCount { get; set; } = 0;
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChefPromoRedemption
    {
        [Key] public int Id { get; set; }
        public int ChefPromoCodeId { get; set; }
        public ChefPromoCode? ChefPromoCode { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public int? BookingId { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
    }
}
