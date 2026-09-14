using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M87: Commission Engine ─────────────────────────────────────
    // A CommissionProfile is a versioned, approvable "commission plan" that the
    // Super Admin builds and publishes. Only one profile can be Active at a
    // time (enforced in CommissionEngineService). This is intentionally kept
    // separate from the M41 CommissionRule/CommissionLedger tables (Money &
    // Operations phase, per-booking settlement bookkeeping) — this engine is
    // the *policy* layer that decides what percentages apply, versioned and
    // audited the way a Super Admin needs.
    public class CommissionProfile
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(80)] public string Name { get; set; } = "";
        public string? Description { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Draft"; // Draft / PendingApproval / Active / Archived
        public decimal DefaultPlatformPercent { get; set; } = 15;
        public decimal DefaultChefPercent { get; set; } = 85;
        public int Version { get; set; } = 1;
        public int? ClonedFromProfileId { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        public int? ApprovedByUserId { get; set; }
        public User? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CommissionProfileRule> Rules { get; set; } = new List<CommissionProfileRule>();
    }

    public class CommissionRuleAuditLog
    {
        [Key] public int Id { get; set; }
        public int CommissionProfileId { get; set; }
        public CommissionProfile? CommissionProfile { get; set; }
        [MaxLength(30)] public string Action { get; set; } = ""; // Created / RuleAdded / RuleUpdated / RuleRemoved / SubmittedForApproval / Approved / Rejected / Activated / Archived
        public int ChangedByUserId { get; set; }
        public User? ChangedBy { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M88: Dynamic Commission Rules ──────────────────────────────
    // Each rule belongs to a profile and narrows/overrides the profile's
    // default percentage when its condition matches. Rules are evaluated in
    // Priority order (lowest number first); the highest-priority match wins,
    // except VolumeTier and PerformanceBonus rules which can additionally
    // stack as a delta on top of whichever base rule matched (see
    // DynamicCommissionRuleService.Evaluate for the exact precedence).
    public class CommissionProfileRule
    {
        [Key] public int Id { get; set; }
        public int CommissionProfileId { get; set; }
        public CommissionProfile? CommissionProfile { get; set; }

        [MaxLength(30)] public string RuleType { get; set; } = "CityOverride";
        // CityOverride / CategoryOverride / VolumeTier / SeasonalMultiplier / PerformanceBonus / NewChefIncentive

        public int Priority { get; set; } = 100; // lower evaluates first

        // Matching conditions — nullable/blank fields are wildcards ("applies to all")
        [MaxLength(60)] public string? City { get; set; }
        [MaxLength(60)] public string? Category { get; set; }        // service/menu category, e.g. "Home Cooking"
        public int? MinMonthlyBookingVolume { get; set; }            // for VolumeTier
        public int? MaxMonthlyBookingVolume { get; set; }
        public decimal? MinChefRating { get; set; }                  // for PerformanceBonus
        public DateTime? SeasonStart { get; set; }                   // for SeasonalMultiplier (festival/peak pricing windows)
        public DateTime? SeasonEnd { get; set; }
        public int? MaxChefTenureDays { get; set; }                  // for NewChefIncentive

        // Effect — either an absolute override or a +/- delta, never both
        public decimal? OverridePlatformPercent { get; set; }
        public decimal? DeltaPlatformPercent { get; set; }           // e.g. -2 gives chef 2% more during a festival push

        public bool IsActive { get; set; } = true;
        public bool IsStackable { get; set; } = false; // if true, applies as a delta on top of the matched base rule
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Audit trail of every dynamic evaluation — lets Support/Finance explain
    // to a chef exactly why a given booking settled at a given percentage.
    public class DynamicCommissionEvaluation
    {
        [Key] public int Id { get; set; }
        public int? BookingId { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public int CommissionProfileId { get; set; }
        public CommissionProfile? CommissionProfile { get; set; }
        public decimal BookingAmount { get; set; }
        public decimal ComputedPlatformPercent { get; set; }
        public decimal ComputedChefPercent { get; set; }
        [MaxLength(400)] public string AppliedRulesSummary { get; set; } = ""; // e.g. "Base 15% -> CityOverride(Chennai) 13% -> VolumeTier stack -1%"
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    }
}
