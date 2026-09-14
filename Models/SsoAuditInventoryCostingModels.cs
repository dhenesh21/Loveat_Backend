using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M137: Enterprise — SSO / Single Sign-On ────────────────────
    // Per-CorporateAccount identity federation config, so a company's
    // employees can log in via their own IdP instead of individual
    // phone/password accounts. Stores connection config only — actual
    // SAML/OIDC protocol handling would live in AuthService as a follow-up;
    // this batch delivers the configuration and domain-matching layer.
    public class SsoConnection
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        [MaxLength(20)] public string Protocol { get; set; } = "SAML"; // SAML / OIDC
        [MaxLength(120)] public string? IdpEntityId { get; set; }
        [MaxLength(500)] public string? IdpSsoUrl { get; set; }
        [MaxLength(500)] public string? IdpCertificate { get; set; } // PEM-encoded, or OIDC client secret reference
        [MaxLength(120)] public string EmailDomain { get; set; } = ""; // e.g. "acme.com" — users with this email domain are routed to SSO
        public bool IsActive { get; set; } = false; // starts false until an admin tests + activates it
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SsoLoginLog
    {
        [Key] public int Id { get; set; }
        public int SsoConnectionId { get; set; }
        public SsoConnection? SsoConnection { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        [MaxLength(100)] public string? Email { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Success"; // Success / Failed
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M138: Enterprise — Enterprise Audit Log ────────────────────
    // Dedicated audit trail for actions taken on/within a CorporateAccount
    // (member added/removed, billing viewed, SSO config changed, bulk order
    // approved) — distinct from the general-purpose AdminActivityLog used
    // platform-wide; this one is scoped and exportable per-account so a
    // corporate customer's own compliance team can review it.
    public class EnterpriseAuditEntry
    {
        [Key] public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public CorporateAccount? CorporateAccount { get; set; }
        [MaxLength(60)] public string Action { get; set; } = ""; // MemberAdded / MemberRemoved / BillingViewed / SsoConfigChanged / BulkOrderApproved / ...
        public int? ActorUserId { get; set; }
        public User? ActorUser { get; set; }
        [MaxLength(300)] public string? Details { get; set; }
        [MaxLength(45)] public string? IpAddress { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    // ── M139: Chef Business Suite — Inventory Management ───────────
    // Real stock tracking for a chef's kitchen — distinct from the existing
    // B2B IngredientListing (a marketplace for buying/selling between
    // chefs). This is "how much do I have on hand right now."
    public class InventoryItem
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(80)] public string IngredientName { get; set; } = "";
        [MaxLength(20)] public string Unit { get; set; } = "kg"; // kg / g / l / ml / pcs
        public decimal QuantityOnHand { get; set; } = 0;
        public decimal ReorderThreshold { get; set; } = 0; // triggers a low-stock flag when QuantityOnHand falls below this
        public DateTime? LastRestockedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InventoryTransaction
    {
        [Key] public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        [MaxLength(20)] public string ChangeType { get; set; } = "Restock"; // Restock / Usage / Waste / Adjustment
        public decimal QuantityChange { get; set; } // positive for Restock, negative for Usage/Waste
        [MaxLength(200)] public string? Reason { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M140: Chef Business Suite — Recipe Costing ─────────────────
    // Ingredient-level cost breakdown for a ChefMenuItem, so a chef can see
    // their true cost per dish and get a suggested price at a target margin
    // — rather than pricing by gut feel.
    public class RecipeIngredient
    {
        [Key] public int Id { get; set; }
        public int ChefMenuItemId { get; set; }
        public ChefMenuItem? ChefMenuItem { get; set; }
        [MaxLength(80)] public string IngredientName { get; set; } = "";
        public decimal QuantityUsed { get; set; }
        [MaxLength(20)] public string Unit { get; set; } = "g";
        public decimal UnitCost { get; set; } // cost per Unit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
