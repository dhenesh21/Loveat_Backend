using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>
    /// Chef-specific profile (M4). This is the entity that ChefPortfolioItem,
    /// ChefMenuItem, ChefServiceOffering, and PricingRule/ChefMenuItem (batches
    /// 18/20/30) already reference via `ChefProfileId` + `ChefProfile?` nav.
    ///
    /// Note this is separate from the `ChefId` (int, pointing straight at
    /// User.Id) used by earnings/commission/settlement/invoice/dispute in
    /// batches 17/21/22/23 — that's an existing inconsistency in the codebase
    /// (some modules key off the User row directly, others off this profile
    /// row). Both are left as-is here since changing it would mean touching
    /// already-working batch code; ChefProfile.UserId is how you cross-reference
    /// between the two conventions when needed.
    /// </summary>
    public class ChefProfile
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        /// <summary>JSON array, e.g. ["South Indian","North Indian","Chinese"]</summary>
        public string Cuisines { get; set; } = "[]";

        /// <summary>JSON array of skill tags, e.g. ["Baking","Tandoor","Vegan cooking"]</summary>
        public string Skills { get; set; } = "[]";

        public int ExperienceYears { get; set; } = 0;
        public decimal HourlyRate { get; set; } = 0;

        [MaxLength(100)]
        public string? City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool IsVerified { get; set; } = false;
        public bool IsAvailable { get; set; } = true;

        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public int TotalBookingsCompleted { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ──
        public ICollection<ChefPortfolioItem> PortfolioItems { get; set; } = new List<ChefPortfolioItem>();
        public ICollection<ChefMenuItem> MenuItems { get; set; } = new List<ChefMenuItem>();
    }
}
