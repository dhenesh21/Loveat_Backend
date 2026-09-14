namespace LovEat.API.DTOs
{
    // ── M145: Chef Business Suite — Tax Filing Assistant ────────────
    public class GenerateTaxEstimateRequestDto { public int ChefId { get; set; } public string FinancialYear { get; set; } = ""; public decimal PresumptiveIncomePercent { get; set; } = 8; }

    public class ChefTaxEstimateDto
    {
        public string FinancialYear { get; set; } = "";
        public decimal GrossIncome { get; set; }
        public decimal DeductibleExpenses { get; set; }
        public decimal PresumptiveIncomePercent { get; set; }
        public decimal EstimatedTaxableIncome { get; set; }
        public decimal EstimatedTaxLiability { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class LogBusinessExpenseRequestDto
    {
        public int ChefId { get; set; }
        public string Category { get; set; } = "Ingredients";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    }

    public class ChefBusinessExpenseDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
    }

    // ── M146: Chef Business Suite — Chef Loyalty Program ────────────
    public class CreateChefLoyaltyProgramRequestDto
    {
        public int ChefId { get; set; }
        public decimal PointsPerRupeeSpent { get; set; } = 1;
        public int RedemptionThresholdPoints { get; set; } = 100;
        public decimal RedemptionValueRupees { get; set; } = 50;
    }

    public class ChefLoyaltyProgramDto
    {
        public int Id { get; set; }
        public decimal PointsPerRupeeSpent { get; set; }
        public int RedemptionThresholdPoints { get; set; }
        public decimal RedemptionValueRupees { get; set; }
        public bool IsActive { get; set; }
    }

    public class EarnChefLoyaltyPointsRequestDto
    {
        public int ChefId { get; set; }
        public int CustomerId { get; set; }
        public decimal BookingAmount { get; set; }
        public int? BookingId { get; set; }
    }

    public class RedeemChefLoyaltyPointsRequestDto
    {
        public int ChefId { get; set; }
        public int CustomerId { get; set; }
        public decimal PointsToRedeem { get; set; }
    }

    public class ChefLoyaltyBalanceDto
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal PointsBalance { get; set; }
        public decimal LifetimePointsEarned { get; set; }
    }

    // ── M147: Chef Business Suite — Business Goals & Targets ────────
    public class CreateBusinessGoalRequestDto
    {
        public int ChefId { get; set; }
        public string PeriodKey { get; set; } = "";
        public string GoalType { get; set; } = "Revenue";
        public decimal TargetValue { get; set; }
    }

    public class ChefBusinessGoalDto
    {
        public int Id { get; set; }
        public string PeriodKey { get; set; } = "";
        public string GoalType { get; set; } = "";
        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; }
        public decimal ProgressPercent { get; set; }
        public bool IsAchieved { get; set; }
    }

    public class RefreshGoalProgressRequestDto { public int GoalId { get; set; } }

    // ── M148: Chef Business Suite — Multi-Outlet Management ─────────
    public class CreateChefOutletRequestDto
    {
        public int ChefId { get; set; }
        public string OutletName { get; set; } = "";
        public string OutletType { get; set; } = "Home";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int MaxDailyOrderCapacity { get; set; } = 20;
        public bool IsPrimary { get; set; } = false;
    }

    public class ChefOutletDto
    {
        public int Id { get; set; }
        public string OutletName { get; set; } = "";
        public string OutletType { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public int MaxDailyOrderCapacity { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }

    public class DeactivateOutletRequestDto { public int OutletId { get; set; } }
}
