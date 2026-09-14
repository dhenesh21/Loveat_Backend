using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M75: Additional language localization
    //
    // User.PreferredLanguage (added in Phase 2) already stores which
    // language a user wants. This adds the actual translation dictionary
    // both mobile apps can fetch to localize their UI text, plus the list
    // of languages the platform actually supports — rather than hardcoding
    // translated strings into every screen's JSX, which doesn't scale past
    // a handful of languages and can't be updated without a new app build.
    // ══════════════════════════════════════════════════════════════
    public class SupportedLanguage
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(5)]
        public string Code { get; set; } = ""; // en / ta / hi / te / kn / ml

        [MaxLength(40)]
        public string Name { get; set; } = ""; // English display name, e.g. "Tamil"

        [MaxLength(40)]
        public string NativeName { get; set; } = ""; // e.g. "தமிழ்"

        public bool IsActive { get; set; } = true;
    }

    /// <summary>One translated string for one key in one language. E.g. Key="welcome_title", LanguageCode="ta", Value="வணக்கம்".</summary>
    public class TranslationString
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = "";

        [Required, MaxLength(5)]
        public string LanguageCode { get; set; } = "en";

        [MaxLength(1000)]
        public string Value { get; set; } = "";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M76: Subscription meal box delivery (no chef visit — pre-cooked,
    // pre-packaged weekly/bi-weekly box, distinct from M19's Subscription
    // which is a recurring "book the same chef" arrangement)
    // ══════════════════════════════════════════════════════════════
    public class MealBoxPlan
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(60)]
        public string Name { get; set; } = "";

        public int MealsPerBox { get; set; }
        public decimal PricePerBox { get; set; }

        [MaxLength(20)]
        public string Frequency { get; set; } = "Weekly"; // Weekly / BiWeekly

        [MaxLength(30)]
        public string? CuisineType { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class MealBoxSubscription
    {
        [Key] public int Id { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        public int PlanId { get; set; }
        public MealBoxPlan? Plan { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active / Paused / Cancelled

        public DateTime NextDeliveryDate { get; set; }

        [MaxLength(300)]
        public string DeliveryAddress { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }
    }

    public class MealBoxDelivery
    {
        [Key] public int Id { get; set; }

        public int SubscriptionId { get; set; }
        public MealBoxSubscription? Subscription { get; set; }

        public DateTime ScheduledDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled / OutForDelivery / Delivered / Skipped

        public DateTime? DeliveredAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
