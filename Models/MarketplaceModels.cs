using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M60: Marketplace Expansion ─────────────────────────────────

    // Multiple service types beyond cooking
    public class MarketplaceService
    {
        [Key] public int Id { get; set; }
        public string  ServiceName   { get; set; } = ""; // HomeCooking / EventCatering / MealPrep / NutritionConsult / CookingClass
        public string  Description   { get; set; } = "";
        public string  IconName      { get; set; } = ""; // Ionicon name
        public decimal BasePrice     { get; set; }
        public string  PriceUnit     { get; set; } = ""; // per hour / per session / per head
        public bool    IsActive      { get; set; } = true;
        public bool    IsNew         { get; set; } = false;
        public bool    IsComingSoon  { get; set; } = false;
        public int     SortOrder     { get; set; } = 0;
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
    }

    // Chef's offered services (which marketplace services they provide)
    public class ChefServiceOffering
    {
        [Key] public int Id { get; set; }
        public int    ChefProfileId    { get; set; }
        public ChefProfile? ChefProfile{ get; set; }
        public int    MarketplaceServiceId { get; set; }
        public MarketplaceService? Service { get; set; }
        public decimal CustomPrice     { get; set; }
        public string  PriceUnit       { get; set; } = "";
        public bool    IsActive        { get; set; } = true;
        public string? Description     { get; set; }
        public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
    }

    // Multi-city expansion tracking
    public class ExpansionCity
    {
        [Key] public int Id { get; set; }
        public string  CityName         { get; set; } = "";
        public string  State            { get; set; } = "";
        public string  Country          { get; set; } = "India";
        public decimal Latitude         { get; set; }
        public decimal Longitude        { get; set; }
        public string  LaunchStatus     { get; set; } = "Planned"; // Planned / Soft Launch / Live / Paused
        public DateTime? PlannedLaunch  { get; set; }
        public DateTime? ActualLaunch   { get; set; }
        public int     TargetChefs      { get; set; }
        public int     CurrentChefs     { get; set; }
        public int     WaitlistCount    { get; set; }
        public string? LaunchNotes      { get; set; }
        public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    }

    // User waitlist for new cities
    public class CityWaitlist
    {
        [Key] public int Id { get; set; }
        public int    UserId        { get; set; }
        public string CityName      { get; set; } = "";
        public string UserType      { get; set; } = "Customer"; // Customer / Chef
        public string? Phone        { get; set; }
        public string? Email        { get; set; }
        public DateTime JoinedAt    { get; set; } = DateTime.UtcNow;
        public bool   IsNotified    { get; set; } = false;
    }
}

namespace LovEat.API.DTOs
{
    public class MarketplaceServiceDto
    {
        public int     Id           { get; set; }
        public string  ServiceName  { get; set; } = "";
        public string  Description  { get; set; } = "";
        public string  IconName     { get; set; } = "";
        public decimal BasePrice    { get; set; }
        public string  PriceUnit    { get; set; } = "";
        public bool    IsActive     { get; set; }
        public bool    IsNew        { get; set; }
        public bool    IsComingSoon { get; set; }
        public int     ChefCount    { get; set; }
    }

    public class ExpansionCityDto
    {
        public int     Id             { get; set; }
        public string  CityName       { get; set; } = "";
        public string  State          { get; set; } = "";
        public string  Country        { get; set; } = "";
        public decimal Latitude       { get; set; }
        public decimal Longitude      { get; set; }
        public string  LaunchStatus   { get; set; } = "";
        public string? PlannedLaunch  { get; set; }
        public string? ActualLaunch   { get; set; }
        public int     TargetChefs    { get; set; }
        public int     CurrentChefs   { get; set; }
        public int     WaitlistCount  { get; set; }
        public double  ReadinessPercent{ get; set; }
        public string? LaunchNotes    { get; set; }
    }

    public class MarketplaceOverviewDto
    {
        public int     TotalServices    { get; set; }
        public int     ActiveServices   { get; set; }
        public int     ComingSoonServices{ get; set; }
        public int     LiveCities       { get; set; }
        public int     PlannedCities    { get; set; }
        public int     TotalWaitlist    { get; set; }
        public List<MarketplaceServiceDto> Services { get; set; } = new();
        public List<ExpansionCityDto>      Cities   { get; set; } = new();
    }

    public class JoinWaitlistDto
    {
        public string CityName  { get; set; } = "";
        public string UserType  { get; set; } = "Customer";
        public string? Phone    { get; set; }
        public string? Email    { get; set; }
    }
}
