namespace LovEat.API.DTOs
{
    // ── Business Suite Overview ─────────────────────────────────────
    public class LowStockItemDto
    {
        public int Id { get; set; }
        public string ChefName { get; set; } = "";
        public string ItemName { get; set; } = "";
        public int QuantityOnHand { get; set; }
        public int LowStockThreshold { get; set; }
        public string Unit { get; set; } = "";
    }

    public class StaffSummaryDto
    {
        public string ChefName { get; set; } = "";
        public int ActiveStaffCount { get; set; }
        public int TotalStaffCount { get; set; }
    }

    public class ChefPackageDto
    {
        public int Id { get; set; }
        public string ChefName { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int TimesBooked { get; set; }
        public bool Active { get; set; }
    }

    public class TaxEstimateDto
    {
        public string ChefName { get; set; } = "";
        public string FinancialYear { get; set; } = "";
        public decimal GrossEarnings { get; set; }
        public decimal EstimatedTax { get; set; }
    }

    public class GoalsAchievedSummaryDto
    {
        public int TotalGoalsSet { get; set; }
        public int Achieved { get; set; }
        public double AchievedPercent { get; set; }
    }

    public class ChefBusinessSuiteOverviewDto
    {
        public List<LowStockItemDto> LowStock { get; set; } = new();
        public List<StaffSummaryDto> StaffSummary { get; set; } = new();
        public List<ChefPackageDto> Packages { get; set; } = new();
        public List<PromoUsageDto> PromoUsage { get; set; } = new();
        public List<TaxEstimateDto> TaxEstimates { get; set; } = new();
        public GoalsAchievedSummaryDto GoalsAchieved { get; set; } = new();
        public List<AdminOpsChefOutletDto> Outlets { get; set; } = new();
    }

    public class CorporateClientDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Phone { get; set; } = "";
        public int Employees { get; set; }
        public string Plan { get; set; } = "";
        public int MealsPerDay { get; set; }
        public string Status { get; set; } = "";
        public decimal TotalSpend { get; set; }
        public DateTime StartDate { get; set; }
        public string ChefName { get; set; } = "";
    }

    public class CorporatePlanSummaryDto
    {
        public string Name { get; set; } = "";
        public int MealsPerDay { get; set; }
        public decimal PricePerMeal { get; set; }
        public List<string> Features { get; set; } = new();
        public int ActiveSubscribers { get; set; }
    }

    public class UpdateStatusRequestDto
    {
        public string Status { get; set; } = "";
    }

    // ── AdminDietarySocialPages ──────────────────────────────────────
    public class AdminUserDietaryRowDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string DietType { get; set; } = "";
        public List<string> Allergies { get; set; } = new();
        public List<string> HealthGoals { get; set; } = new();
        public string TopMatchChefName { get; set; } = "";
        public double TopMatchScore { get; set; }
        public int TotalMatches { get; set; }
    }

    public class AdminMealPostDto
    {
        public int Id { get; set; }
        public string ChefName { get; set; } = "";
        public string Cuisine { get; set; } = "";
        public string Caption { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminChefFollowDto
    {
        public string FollowerName { get; set; } = "";
        public string ChefName { get; set; } = "";
        public DateTime FollowedAt { get; set; }
    }

    // ── AdminGroupBookingMLMatching ──────────────────────────────────
    public class AdminGroupBookingRowDto
    {
        public int Id { get; set; }
        public string Customer { get; set; } = "";
        public int TotalGuests { get; set; }
        public int ChefsUsed { get; set; }
        public string Status { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public DateTime ScheduledAt { get; set; }
    }

    public class AdminMLMatchStatsDto
    {
        public int TotalMatchesComputed { get; set; }
        public double AvgScore { get; set; }
        public string TopReason { get; set; } = "";
    }

    // ── AdminTrackingWeatherContractMenuBuild ────────────────────────
    public class AdminTrackingRowDto
    {
        public int BookingId { get; set; }
        public string CurrentStage { get; set; } = "";
        public DateTime LastUpdate { get; set; }
    }

    public class AdminContractRowDto
    {
        public int BookingId { get; set; }
        public bool CustomerAccepted { get; set; }
        public bool ChefAccepted { get; set; }
        public string Status { get; set; } = "";
    }

    public class AdminMenuBuildRowDto
    {
        public string CustomerName { get; set; } = "";
        public string ChefName { get; set; } = "";
        public string BuildName { get; set; } = "";
        public decimal EstimatedTotal { get; set; }
        public string Status { get; set; } = "";
    }

    // ── AdminWishlistBadgePreferencesReorder ─────────────────────────
    public class AdminWishlistTopChefDto
    {
        public string ChefName { get; set; } = "";
        public int WishlistCount { get; set; }
    }

    public class AdminWishlistStatsDto
    {
        public int TotalItems { get; set; }
        public List<AdminWishlistTopChefDto> TopChefs { get; set; } = new();
    }

    public class AdminBadgeRowDto
    {
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public string Tier { get; set; } = "";
        public int AwardedCount { get; set; }
    }

    public class AdminPreferencesStatsDto
    {
        public double PushEnabledPercent { get; set; }
        public double EmailEnabledPercent { get; set; }
        public double PromoOptOutPercent { get; set; }
        public double DarkModePercent { get; set; }
    }

    public class AdminReorderComboRowDto
    {
        public string CustomerName { get; set; } = "";
        public string ComboName { get; set; } = "";
        public int UseCount { get; set; }
    }

    public class AdminOpsChefOutletDto
    {
        public string ChefName { get; set; } = "";
        public string OutletName { get; set; } = "";
        public string OutletType { get; set; } = "";
        public bool IsPrimary { get; set; }
    }

    public class PromoUsageDto
    {
        public string Code { get; set; } = "";
        public int TimesUsed { get; set; }
        public decimal TotalDiscountGiven { get; set; }
    }

    // ── Chef Performance Analytics ──────────────────────────────────
    public class AdminOpsChefPerformanceDto
    {
        public int ChefUserId { get; set; }
        public string ChefName { get; set; } = "";
        public string City { get; set; } = "";
        public double AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public double CompletionRate { get; set; }
        public double AvgResponseTimeMins { get; set; }
        public decimal TotalEarnings { get; set; }
        public double TotalHoursWorked { get; set; }
        public int UniqueCustomers { get; set; }
        public int RepeatCustomers { get; set; }
        public double RepeatRate { get; set; }
        public string PerformanceGrade { get; set; } = "C"; // A+/A/B/C/D, computed from rating+completion
        public string Trend { get; set; } = "Stable";        // Improving/Stable/Declining — last 15 vs prior 15 bookings
        public int EarningsRank { get; set; }
        public int RatingRank { get; set; }
        public int CompletionRank { get; set; }
        public int TotalChefsInCity { get; set; }
    }

    // ── Equipment & Certifications ──────────────────────────────────
    public class AdminOpsEquipmentListingDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string ChefName { get; set; } = "";
        public string Category { get; set; } = "";
        public string ListingType { get; set; } = "";
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? City { get; set; }
    }

    public class CreateEquipmentListingRequestDto
    {
        public int ChefUserId { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "Utensil";
        public string ListingType { get; set; } = "Sell";
        public decimal Price { get; set; }
        public string? City { get; set; }
    }

    public class AdminOpsCertificationCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Level { get; set; } = "";
        public decimal Fee { get; set; }
        public int Enrolled { get; set; }
        public int Completed { get; set; }
    }
}
