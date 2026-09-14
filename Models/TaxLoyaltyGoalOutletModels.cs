using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M145: Chef Business Suite — Tax Filing Assistant ───────────
    // A personal estimator for the CHEF's own income tax (presumptive
    // taxation under Section 44AD for small businesses, ~6-8% of turnover)
    // — distinct from the platform-level M93 GST module, which handles the
    // platform's own GST compliance on bookings.
    public class ChefTaxEstimate
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(9)] public string FinancialYear { get; set; } = ""; // e.g. "2026-27"
        public decimal GrossIncome { get; set; }
        public decimal DeductibleExpenses { get; set; } // chef-logged business expenses (ingredients, equipment, staff wages)
        public decimal PresumptiveIncomePercent { get; set; } = 8; // Section 44AD default (6% digital receipts, 8% cash)
        public decimal EstimatedTaxableIncome { get; set; }
        public decimal EstimatedTaxLiability { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChefBusinessExpense
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(60)] public string Category { get; set; } = "Ingredients"; // Ingredients / Equipment / StaffWages / Transport / Other
        [MaxLength(200)] public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M146: Chef Business Suite — Chef Loyalty Program ───────────
    // A chef's own repeat-customer points program — distinct from the
    // platform-wide M20-era LoyaltyService, which is a single unified point
    // system across the whole platform. This lets an individual chef run
    // their own reward scheme for their regulars.
    public class ChefLoyaltyProgram
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public decimal PointsPerRupeeSpent { get; set; } = 1; // e.g. 1 point per ₹10 -> set as 0.1
        public int RedemptionThresholdPoints { get; set; } = 100;
        public decimal RedemptionValueRupees { get; set; } = 50; // value of RedemptionThresholdPoints when redeemed
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChefCustomerLoyaltyBalance
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public decimal PointsBalance { get; set; } = 0;
        public decimal LifetimePointsEarned { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChefLoyaltyTransaction
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public int CustomerId { get; set; }
        [MaxLength(20)] public string Type { get; set; } = "Earn"; // Earn / Redeem
        public decimal Points { get; set; }
        public int? BookingId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M147: Chef Business Suite — Business Goals & Targets ───────
    public class ChefBusinessGoal
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(7)] public string PeriodKey { get; set; } = ""; // yyyy-MM
        [MaxLength(20)] public string GoalType { get; set; } = "Revenue"; // Revenue / BookingCount / NewCustomers
        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; } = 0; // updated by RefreshProgressAsync
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRefreshedAt { get; set; }
    }

    // ── M148: Chef Business Suite — Multi-Outlet / Kitchen Management ──
    // For chefs operating from more than one physical kitchen (home kitchen
    // + rented cloud kitchen + event space) — each with its own address,
    // capacity, and operating hours, distinct from the single City/Lat/Lng
    // on ChefProfile which assumes one base location.
    public class ChefOutlet
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(80)] public string OutletName { get; set; } = ""; // e.g. "Home Kitchen", "OMR Cloud Kitchen"
        [MaxLength(20)] public string OutletType { get; set; } = "Home"; // Home / CloudKitchen / EventSpace
        [MaxLength(300)] public string Address { get; set; } = "";
        [MaxLength(60)] public string City { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int MaxDailyOrderCapacity { get; set; } = 20;
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
