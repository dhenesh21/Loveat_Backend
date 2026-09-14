using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M30: Quality Check ─────────────────────────────────────────
    public class QualityCheck
    {
        [Key] public int Id { get; set; }
        public int    BookingId    { get; set; }
        public int    CustomerId   { get; set; }
        public User?  Customer     { get; set; }
        public int    OverallScore { get; set; }   // 1–5
        public int    TasteScore   { get; set; }
        public int    HygieneScore { get; set; }
        public int    PresentationScore { get; set; }
        public int    PortionScore { get; set; }
        public string? Comments    { get; set; }
        public bool   WouldReorder { get; set; } = true;
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    }

    // ── M36: Smart Safety ─────────────────────────────────────────
    public class SafetyAlert
    {
        [Key] public int Id { get; set; }
        public int    UserId      { get; set; }
        public User?  User        { get; set; }
        public string AlertType   { get; set; } = "SOS"; // SOS / LateArrival / RouteDeviation
        public string Status      { get; set; } = "Active"; // Active / Resolved
        public decimal Latitude   { get; set; }
        public decimal Longitude  { get; set; }
        public int?   BookingId   { get; set; }
        public string? Message    { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }

    // ── M37: Grocery Auto-List ────────────────────────────────────
    public class GroceryList
    {
        [Key] public int Id { get; set; }
        public int    BookingId   { get; set; }
        public int    ChefId      { get; set; }
        public string ItemsJson   { get; set; } = "[]"; // JSON array of GroceryItem
        public bool   IsConfirmed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M38/M39: Meal Planning ────────────────────────────────────
    public class MealPlan
    {
        [Key] public int Id { get; set; }
        public int    UserId      { get; set; }
        public User?  User        { get; set; }
        public string PlanType    { get; set; } = "Weekly"; // Weekly / Monthly
        public string WeekStart   { get; set; } = "";       // ISO date of Monday
        public string EntriesJson { get; set; } = "{}";    // JSON { "Mon_Breakfast": {...} }
        public int?   PreferredChefId { get; set; }
        public string Preferences { get; set; } = "{}";   // dietary prefs JSON
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M40: Chef Portfolio ───────────────────────────────────────
    public class ChefPortfolioItem
    {
        [Key] public int Id { get; set; }
        public int    ChefProfileId { get; set; }
        public ChefProfile? ChefProfile { get; set; }
        public string ImageUrl    { get; set; } = "";
        public string Caption     { get; set; } = "";
        public string Cuisine     { get; set; } = "";
        public string DishName    { get; set; } = "";
        public int    LikeCount   { get; set; } = 0;
        public int    SortOrder   { get; set; } = 0;
        public bool   IsActive    { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // M30
    public class CreateQualityCheckDto
    {
        public int    BookingId           { get; set; }
        public int    OverallScore        { get; set; }
        public int    TasteScore          { get; set; }
        public int    HygieneScore        { get; set; }
        public int    PresentationScore   { get; set; }
        public int    PortionScore        { get; set; }
        public string? Comments           { get; set; }
        public bool   WouldReorder        { get; set; } = true;
    }

    public class QualityCheckDto
    {
        public int    Id                  { get; set; }
        public int    BookingId           { get; set; }
        public int    OverallScore        { get; set; }
        public int    TasteScore          { get; set; }
        public int    HygieneScore        { get; set; }
        public int    PresentationScore   { get; set; }
        public int    PortionScore        { get; set; }
        public string? Comments           { get; set; }
        public bool   WouldReorder        { get; set; }
        public string CreatedAt           { get; set; } = "";
    }

    // M36
    public class CreateSafetyAlertDto
    {
        public string  AlertType  { get; set; } = "SOS";
        public decimal Latitude   { get; set; }
        public decimal Longitude  { get; set; }
        public int?    BookingId  { get; set; }
        public string? Message    { get; set; }
    }

    // M37
    public class GroceryItemDto
    {
        public string Name      { get; set; } = "";
        public string Quantity  { get; set; } = "";
        public string Unit      { get; set; } = "";
        public string Category  { get; set; } = "";
        public bool   Checked   { get; set; } = false;
    }

    public class GroceryListDto
    {
        public int    BookingId  { get; set; }
        public string Cuisine    { get; set; } = "";
        public string DishName   { get; set; } = "";
        public bool   IsConfirmed{ get; set; }
        public List<GroceryItemDto> Items { get; set; } = new();
    }

    // M38/M39
    public class MealEntryDto
    {
        public string MealType  { get; set; } = ""; // Breakfast/Lunch/Dinner/Snack
        public string DishName  { get; set; } = "";
        public string Cuisine   { get; set; } = "";
        public int?   ChefId    { get; set; }
        public bool   IsBooked  { get; set; } = false;
    }

    // M40
    public class ChefPortfolioItemDto
    {
        public int    Id        { get; set; }
        public string ImageUrl  { get; set; } = "";
        public string Caption   { get; set; } = "";
        public string Cuisine   { get; set; } = "";
        public string DishName  { get; set; } = "";
        public int    LikeCount { get; set; }
        public int    SortOrder { get; set; }
    }

    public class AddPortfolioItemDto
    {
        public string ImageUrl  { get; set; } = "";
        public string Caption   { get; set; } = "";
        public string Cuisine   { get; set; } = "";
        public string DishName  { get; set; } = "";
    }
}
