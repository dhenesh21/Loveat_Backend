namespace LovEat.API.DTOs
{
    // ── M137: Enterprise — SSO / Single Sign-On ────────────────────
    public class CreateSsoConnectionRequestDto
    {
        public int CorporateAccountId { get; set; }
        public string Protocol { get; set; } = "SAML";
        public string? IdpEntityId { get; set; }
        public string? IdpSsoUrl { get; set; }
        public string? IdpCertificate { get; set; }
        public string EmailDomain { get; set; } = "";
    }

    public class SsoConnectionDto
    {
        public int Id { get; set; }
        public int CorporateAccountId { get; set; }
        public string? CompanyName { get; set; }
        public string Protocol { get; set; } = "";
        public string EmailDomain { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ActivateSsoConnectionRequestDto { public int SsoConnectionId { get; set; } public bool Activate { get; set; } = true; }

    public class LookupSsoByDomainRequestDto { public string Email { get; set; } = ""; }

    public class SsoLookupResultDto
    {
        public bool SsoAvailable { get; set; }
        public int? SsoConnectionId { get; set; }
        public string? Protocol { get; set; }
        public string? IdpSsoUrl { get; set; }
    }

    // ── M138: Enterprise — Enterprise Audit Log ────────────────────
    public class LogEnterpriseAuditRequestDto
    {
        public int CorporateAccountId { get; set; }
        public string Action { get; set; } = "";
        public int? ActorUserId { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
    }

    public class EnterpriseAuditEntryDto
    {
        public string Action { get; set; } = "";
        public string? ActorName { get; set; }
        public string? Details { get; set; }
        public DateTime OccurredAt { get; set; }
    }

    // ── M139: Chef Business Suite — Inventory Management ───────────
    public class CreateInventoryItemRequestDto
    {
        public int ChefId { get; set; }
        public string IngredientName { get; set; } = "";
        public string Unit { get; set; } = "kg";
        public decimal ReorderThreshold { get; set; } = 0;
    }

    public class RecordInventoryTransactionRequestDto
    {
        public int InventoryItemId { get; set; }
        public string ChangeType { get; set; } = "Restock";
        public decimal QuantityChange { get; set; }
        public string? Reason { get; set; }
    }

    public class InventoryItemDto
    {
        public int Id { get; set; }
        public string IngredientName { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal QuantityOnHand { get; set; }
        public decimal ReorderThreshold { get; set; }
        public bool IsLowStock { get; set; }
        public DateTime? LastRestockedAt { get; set; }
    }

    public class InventoryTransactionDto
    {
        public string ChangeType { get; set; } = "";
        public decimal QuantityChange { get; set; }
        public string? Reason { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    // ── M140: Chef Business Suite — Recipe Costing ─────────────────
    public class AddRecipeIngredientRequestDto
    {
        public int ChefMenuItemId { get; set; }
        public string IngredientName { get; set; } = "";
        public decimal QuantityUsed { get; set; }
        public string Unit { get; set; } = "g";
        public decimal UnitCost { get; set; }
    }

    public class RecipeIngredientDto
    {
        public int Id { get; set; }
        public string IngredientName { get; set; } = "";
        public decimal QuantityUsed { get; set; }
        public string Unit { get; set; } = "";
        public decimal UnitCost { get; set; }
        public decimal LineCost { get; set; }
    }

    public class RecipeCostBreakdownRequestDto { public int ChefMenuItemId { get; set; } public decimal TargetMarginPercent { get; set; } = 40; }

    public class RecipeCostBreakdownDto
    {
        public int ChefMenuItemId { get; set; }
        public string? DishName { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal TotalIngredientCost { get; set; }
        public decimal CurrentMarginPercent { get; set; }
        public decimal SuggestedPriceAtTargetMargin { get; set; }
        public List<RecipeIngredientDto> Ingredients { get; set; } = new();
    }
}
