using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M149: Customer Experience — Wishlist/Favorites ──────────────
    public class WishlistItem
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        [MaxLength(20)] public string ItemType { get; set; } = "Chef"; // Chef / MenuItem
        public int ItemId { get; set; } // ChefProfile.Id or ChefMenuItem.Id, depending on ItemType
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M150: Customer Experience — Gamification / Badges ───────────
    // Achievement badges for customer engagement (first order, 10 orders,
    // referred 5 friends, tried 5 cuisines, etc.) — distinct from the M20
    // LoyaltyPoints system, which is a spend-based points/rewards ledger.
    // This is purely recognition/status, no monetary value.
    public class BadgeDefinition
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string Key { get; set; } = ""; // e.g. "first_order", "ten_orders", "five_cuisines"
        [MaxLength(80)] public string Name { get; set; } = "";
        [MaxLength(200)] public string Description { get; set; } = "";
        [MaxLength(40)] public string IconName { get; set; } = "star"; // maps to a client-side icon set
        [MaxLength(20)] public string Tier { get; set; } = "Bronze"; // Bronze / Silver / Gold / Platinum
        public bool IsActive { get; set; } = true;
    }

    public class CustomerBadge
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public int BadgeDefinitionId { get; set; }
        public BadgeDefinition? BadgeDefinition { get; set; }
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M151: Customer Experience — Preferences Center ──────────────
    // Centralized app/notification/display settings — distinct from
    // UserProfile.DietaryPreference (a single field on the profile) and
    // DietarySocialModels' goal-based matching. This is the general
    // "Settings" screen backing store: notification toggles, app theme,
    // default booking behavior.
    public class CustomerPreferences
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public bool PushNotificationsEnabled { get; set; } = true;
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool SmsNotificationsEnabled { get; set; } = true;
        public bool PromoNotificationsEnabled { get; set; } = true;
        [MaxLength(10)] public string Theme { get; set; } = "Light"; // Light / Dark / System
        [MaxLength(10)] public string Language { get; set; } = "en";
        [MaxLength(20)] public string? DefaultPaymentMethod { get; set; }
        public bool ShowSpicyWarnings { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M152: Customer Experience — Reorder & Quick Actions ─────────
    // Saved "quick reorder" combos — a customer's own snapshot of a past
    // booking's items so they can repeat it in one tap, rather than
    // rebuilding the order from scratch. References the original booking
    // for provenance but stores its own copy of what was ordered so it
    // still works if the original booking's line items change later.
    public class QuickReorderCombo
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        public int? SourceBookingId { get; set; }
        [MaxLength(80)] public string ComboName { get; set; } = ""; // e.g. "Usual Friday order"
        public string ItemsJson { get; set; } = "[]"; // snapshot: [{ menuItemId, dishName, quantity, unitPrice }]
        public int UseCount { get; set; } = 0;
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
