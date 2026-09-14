namespace LovEat.API.DTOs
{
    // ── M87: Commission Engine ─────────────────────────────────────
    public class CreateCommissionProfileRequestDto
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal DefaultPlatformPercent { get; set; } = 15;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int? CloneFromProfileId { get; set; } // start from an existing profile's rules
    }

    public class CommissionProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string Status { get; set; } = "";
        public decimal DefaultPlatformPercent { get; set; }
        public decimal DefaultChefPercent { get; set; }
        public int Version { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? CreatedByName { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int RuleCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CommissionProfileDetailDto : CommissionProfileDto
    {
        public List<CommissionProfileRuleDto> Rules { get; set; } = new();
        public List<CommissionAuditEntryDto> AuditLog { get; set; } = new();
    }

    public class SubmitForApprovalRequestDto { public int ProfileId { get; set; } }

    public class ApproveCommissionProfileRequestDto
    {
        public int ProfileId { get; set; }
        public bool Approve { get; set; } = true; // false = reject
        public string? Notes { get; set; }
    }

    public class CommissionAuditEntryDto
    {
        public string Action { get; set; } = "";
        public string? ChangedByName { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    // ── M88: Dynamic Commission Rules ──────────────────────────────
    public class AddCommissionRuleRequestDto
    {
        public int CommissionProfileId { get; set; }
        public string RuleType { get; set; } = "CityOverride";
        public int Priority { get; set; } = 100;
        public string? City { get; set; }
        public string? Category { get; set; }
        public int? MinMonthlyBookingVolume { get; set; }
        public int? MaxMonthlyBookingVolume { get; set; }
        public decimal? MinChefRating { get; set; }
        public DateTime? SeasonStart { get; set; }
        public DateTime? SeasonEnd { get; set; }
        public int? MaxChefTenureDays { get; set; }
        public decimal? OverridePlatformPercent { get; set; }
        public decimal? DeltaPlatformPercent { get; set; }
        public bool IsStackable { get; set; } = false;
    }

    public class CommissionProfileRuleDto
    {
        public int Id { get; set; }
        public string RuleType { get; set; } = "";
        public int Priority { get; set; }
        public string? City { get; set; }
        public string? Category { get; set; }
        public int? MinMonthlyBookingVolume { get; set; }
        public int? MaxMonthlyBookingVolume { get; set; }
        public decimal? MinChefRating { get; set; }
        public DateTime? SeasonStart { get; set; }
        public DateTime? SeasonEnd { get; set; }
        public int? MaxChefTenureDays { get; set; }
        public decimal? OverridePlatformPercent { get; set; }
        public decimal? DeltaPlatformPercent { get; set; }
        public bool IsActive { get; set; }
        public bool IsStackable { get; set; }
    }

    public class EvaluateCommissionRequestDto
    {
        public int ChefId { get; set; }
        public decimal BookingAmount { get; set; }
        public string? City { get; set; }
        public string? Category { get; set; }
        public int? BookingId { get; set; }
    }

    public class CommissionEvaluationResultDto
    {
        public int CommissionProfileId { get; set; }
        public string CommissionProfileName { get; set; } = "";
        public decimal PlatformPercent { get; set; }
        public decimal ChefPercent { get; set; }
        public decimal PlatformAmount { get; set; }
        public decimal ChefAmount { get; set; }
        public string AppliedRulesSummary { get; set; } = "";
    }
}
