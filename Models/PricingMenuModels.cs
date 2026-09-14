using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── Module 35: Dynamic Pricing ─────────────────────────────────
    public class PricingRule
    {
        [Key] public int Id { get; set; }

        public string RuleName    { get; set; } = "";
        public string RuleType    { get; set; } = ""; // PeakHour / Weekend / Holiday / Emergency / Surge / Discount
        public string AppliesTo   { get; set; } = "All"; // All / BookingType / City
        public string? TargetValue{ get; set; }  // e.g. "Emergency" or "Coimbatore"

        public decimal MultiplierPercent { get; set; } = 0; // +ve = surcharge, -ve = discount
        public string  Description       { get; set; } = "";
        public bool    IsActive          { get; set; } = true;

        // When it applies
        public string? DaysOfWeek   { get; set; } // "Sat,Sun" or null for all
        public int?    StartHour    { get; set; } // 0–23
        public int?    EndHour      { get; set; } // 0–23
        public string? ValidFrom    { get; set; } // ISO date
        public string? ValidTo      { get; set; } // ISO date

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PricingHistory
    {
        [Key] public int Id { get; set; }

        public int    BookingId      { get; set; }
        public decimal BaseRate      { get; set; }
        public decimal FinalRate     { get; set; }
        public decimal SurchargeAmt  { get; set; }
        public string  RulesApplied  { get; set; } = "[]"; // JSON array of rule names
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── Module 36: Menu & Cuisine Management ──────────────────────
    public class ChefMenuItem
    {
        [Key] public int Id { get; set; }

        [Required] public int ChefProfileId { get; set; }
        public ChefProfile? ChefProfile { get; set; }

        public string Cuisine     { get; set; } = "";
        public string DishName    { get; set; } = "";
        public string? Description{ get; set; }
        public string? ImageUrl   { get; set; }
        public decimal Price      { get; set; }          // per serving / per head
        public string  PriceUnit  { get; set; } = "per head";
        public bool    IsVeg      { get; set; } = true;
        public bool    IsAvailable{ get; set; } = true;
        public int     SortOrder  { get; set; } = 0;
        public string? Tags       { get; set; }          // "spicy,popular,festive"
        public int     OrderCount { get; set; } = 0;     // how many times booked
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // ── Dynamic Pricing DTOs ───────────────────────────────────────
    public class PricingRuleDto
    {
        public int     Id                { get; set; }
        public string  RuleName          { get; set; } = "";
        public string  RuleType          { get; set; } = "";
        public string  AppliesTo         { get; set; } = "";
        public string? TargetValue       { get; set; }
        public decimal MultiplierPercent { get; set; }
        public string  Description       { get; set; } = "";
        public bool    IsActive          { get; set; }
        public string? DaysOfWeek        { get; set; }
        public int?    StartHour         { get; set; }
        public int?    EndHour           { get; set; }
        public string? ValidFrom         { get; set; }
        public string? ValidTo           { get; set; }
    }

    public class CreatePricingRuleDto
    {
        public string  RuleName          { get; set; } = "";
        public string  RuleType          { get; set; } = "";
        public string  AppliesTo         { get; set; } = "All";
        public string? TargetValue       { get; set; }
        public decimal MultiplierPercent { get; set; }
        public string  Description       { get; set; } = "";
        public string? DaysOfWeek        { get; set; }
        public int?    StartHour         { get; set; }
        public int?    EndHour           { get; set; }
        public string? ValidFrom         { get; set; }
        public string? ValidTo           { get; set; }
    }

    public class PriceCalculationRequestDto
    {
        public int      ChefId      { get; set; }
        public string   BookingType { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public int      StartHour   { get; set; }
        public int      DurationHours{ get; set; } = 1;
        public string?  City        { get; set; }
    }

    public class PriceCalculationResultDto
    {
        public decimal BaseRate        { get; set; }
        public decimal FinalRate       { get; set; }
        public decimal TotalSurcharge  { get; set; }
        public decimal SurchargePercent{ get; set; }
        public List<AppliedRuleDto> AppliedRules { get; set; } = new();
        public decimal EstimatedTotal  { get; set; }
    }

    public class AppliedRuleDto
    {
        public string  RuleName    { get; set; } = "";
        public string  RuleType    { get; set; } = "";
        public decimal Percent     { get; set; }
        public decimal AmountAdded { get; set; }
    }

    public class AdminPricingStatsDto
    {
        public int     TotalRules   { get; set; }
        public int     ActiveRules  { get; set; }
        public decimal AvgSurcharge { get; set; }
        public decimal TotalSurchargeCollected { get; set; }
        public List<PricingRuleDto> Rules { get; set; } = new();
    }

    // ── Menu DTOs ──────────────────────────────────────────────────
    public class ChefMenuItemDto
    {
        public int     Id          { get; set; }
        public string  Cuisine     { get; set; } = "";
        public string  DishName    { get; set; } = "";
        public string? Description { get; set; }
        public string? ImageUrl    { get; set; }
        public decimal Price       { get; set; }
        public string  PriceUnit   { get; set; } = "";
        public bool    IsVeg       { get; set; }
        public bool    IsAvailable { get; set; }
        public string? Tags        { get; set; }
        public int     OrderCount  { get; set; }
        public int     SortOrder   { get; set; }
    }

    public class CreateMenuItemDto
    {
        public string  Cuisine     { get; set; } = "";
        public string  DishName    { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price       { get; set; }
        public string  PriceUnit   { get; set; } = "per head";
        public bool    IsVeg       { get; set; } = true;
        public string? Tags        { get; set; }
    }

    public class ChefMenuDto
    {
        public int ChefProfileId { get; set; }
        public string ChefName   { get; set; } = "";
        public List<string>          Cuisines { get; set; } = new();
        public List<ChefMenuItemDto> Items    { get; set; } = new();
    }
}
