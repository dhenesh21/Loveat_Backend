using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M67: Equipment Marketplace ─────────────────────────────────
    public class EquipmentListing
    {
        [Key] public int Id { get; set; }
        public int    ChefId          { get; set; }
        public User?  Chef            { get; set; }
        public string Title           { get; set; } = "";
        public string Description     { get; set; } = "";
        public string Category        { get; set; } = ""; // CookingWare/Appliance/Utensil/Bakeware/Other
        public string Condition       { get; set; } = ""; // New/LikeNew/Good/Fair
        public string ListingType     { get; set; } = ""; // Rent/Sell/Lend
        public decimal Price          { get; set; }       // per day for rent, fixed for sell
        public string PriceUnit       { get; set; } = ""; // per day / per week / fixed
        public string? ImageUrls      { get; set; } = "[]";
        public string? Brand          { get; set; }
        public bool   IsAvailable     { get; set; } = true;

        /// <summary>Where the equipment physically is. Defaults to the chef's ChefProfile.City
        /// at creation time (see EquipmentService.CreateListingAsync) but can differ from it.</summary>
        public string? City           { get; set; }

        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    }

    public class EquipmentRequest
    {
        [Key] public int Id { get; set; }
        public int    ListingId       { get; set; }
        public EquipmentListing? Listing{ get; set; }
        public int    RequestedByChefId{ get; set; }
        public User?  RequestedBy     { get; set; }
        public string Status          { get; set; } = "Pending"; // Pending/Approved/Rejected/Returned
        public DateTime? FromDate     { get; set; }
        public DateTime? ToDate       { get; set; }
        public decimal? TotalCost     { get; set; }
        public string? Message        { get; set; }
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    }

    // ── M68: Chef Certifications ────────────────────────────────────
    public class CertificationCourse
    {
        [Key] public int Id { get; set; }
        public string Title           { get; set; } = "";
        public string Description     { get; set; } = "";
        public string Category        { get; set; } = ""; // Hygiene/NutritionBasics/AdvancedCooking/Baking/Regional
        public string Level           { get; set; } = ""; // Beginner/Intermediate/Advanced
        public string Provider        { get; set; } = "LovEat Academy";
        public int    DurationHours   { get; set; }
        public decimal Fee            { get; set; } = 0;  // 0 = free
        public string? ContentUrl     { get; set; }
        public string BadgeIconUrl    { get; set; } = "";
        public bool   IsActive        { get; set; } = true;
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    }

    public class ChefCertification
    {
        [Key] public int Id { get; set; }
        public int    ChefId          { get; set; }
        public User?  Chef            { get; set; }
        public int    CourseId        { get; set; }
        public CertificationCourse? Course{ get; set; }
        public string Status          { get; set; } = "Enrolled"; // Enrolled/InProgress/Completed/Failed
        public decimal ProgressPercent{ get; set; } = 0;
        public string? CertificateUrl { get; set; }
        public string? CertificateNo  { get; set; }
        public DateTime EnrolledAt    { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt  { get; set; }
        public DateTime? ExpiresAt    { get; set; }
    }
}
