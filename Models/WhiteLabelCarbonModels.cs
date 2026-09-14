using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M79: White-label platform licensing for other regions/brands
    //
    // A licensed partner gets an API key that lets their own branded
    // app/website call into LovEat's booking engine without becoming a
    // full LovEat customer-facing brand themselves. This is genuinely a
    // B2B partnership feature — a real rollout also needs contracts,
    // billing, and a partner-facing dashboard, none of which is code; this
    // builds the technical piece (API key issuance + usage logging).
    // ══════════════════════════════════════════════════════════════
    public class WhiteLabelClient
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string BrandName { get; set; } = "";

        [MaxLength(100)]
        public string ContactEmail { get; set; } = "";

        [MaxLength(100)]
        public string? Domain { get; set; }

        [MaxLength(10)]
        public string? PrimaryColor { get; set; } // hex, e.g. #1E3A5F

        public string? LogoUrl { get; set; }

        [Required, MaxLength(64)]
        public string ApiKey { get; set; } = GenerateKey();

        [MaxLength(20)]
        public string PlanTier { get; set; } = "Basic"; // Basic / Pro / Enterprise

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public static string GenerateKey() => "wl_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
    }

    public class WhiteLabelUsageLog
    {
        [Key] public int Id { get; set; }

        public int ClientId { get; set; }
        public WhiteLabelClient? Client { get; set; }

        [MaxLength(100)]
        public string Endpoint { get; set; } = "";

        public int ResponseStatus { get; set; }
        public DateTime CalledAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M80: Carbon footprint tracker for sourced ingredients
    //
    // Honest framing: emission factors below are representative published
    // averages (broadly consistent with sources like Our World in Data's
    // food carbon-footprint tables), not a certified life-cycle-assessment
    // for this specific platform's supply chain. Good enough to show a
    // customer a relative comparison ("this meal's footprint is lower than
    // that one"), not a regulatory-grade sustainability claim.
    // ══════════════════════════════════════════════════════════════
    public class IngredientCarbonFactor
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(60)]
        public string IngredientName { get; set; } = "";

        /// <summary>kg of CO2-equivalent emitted per kg of this ingredient produced.</summary>
        public decimal CarbonKgPerKg { get; set; }

        [MaxLength(20)]
        public string Category { get; set; } = ""; // Meat / Dairy / Vegetable / Grain / Other
    }

    public class BookingCarbonEstimate
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public decimal EstimatedCarbonKg { get; set; }

        /// <summary>JSON breakdown per ingredient category contributing to the estimate.</summary>
        public string? BreakdownJson { get; set; }

        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }
}
