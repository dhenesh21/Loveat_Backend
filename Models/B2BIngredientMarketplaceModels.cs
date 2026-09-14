using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M81: B2B API for hotels/co-living spaces to embed LovEat booking
    // ══════════════════════════════════════════════════════════════
    public class B2BPartner
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string CompanyName { get; set; } = "";

        [MaxLength(100)]
        public string ContactEmail { get; set; } = "";

        [Required, MaxLength(64)]
        public string ApiKey { get; set; } = "b2b_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();

        /// <summary>Percentage of each booking's value the partner earns for bringing the guest — distinct from the platform/chef commission split.</summary>
        public decimal CommissionRate { get; set; } = 5;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>One booking made on behalf of a partner's guest — links back to the real Booking row created via the normal BookingService, plus which guest and which partner it came from.</summary>
    public class B2BBookingRequest
    {
        [Key] public int Id { get; set; }

        public int PartnerId { get; set; }
        public B2BPartner? Partner { get; set; }

        [MaxLength(100)]
        public string GuestName { get; set; } = "";

        [MaxLength(15)]
        public string GuestPhone { get; set; } = "";

        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Created"; // Created / Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M82: Chef-to-chef marketplace (sourcing, ingredient swaps)
    //
    // Distinct from M67's EquipmentListing (batch 34): this is for
    // consumable ingredients — a chef with surplus tomatoes lists them for
    // sale or swap, another chef requests them.
    // ══════════════════════════════════════════════════════════════
    public class IngredientListing
    {
        [Key] public int Id { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [Required, MaxLength(80)]
        public string IngredientName { get; set; } = "";

        public decimal Quantity { get; set; }

        [MaxLength(10)]
        public string Unit { get; set; } = "kg"; // kg / g / litre / piece

        /// <summary>0 for a free swap/give-away listing.</summary>
        public decimal Price { get; set; } = 0;

        [MaxLength(10)]
        public string ListingType { get; set; } = "Sell"; // Sell / Swap / Request (a chef looking to BUY this ingredient)

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;

        [MaxLength(100)]
        public string? City { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class IngredientSwapRequest
    {
        [Key] public int Id { get; set; }

        public int ListingId { get; set; }
        public IngredientListing? Listing { get; set; }

        public int RequestedByChefId { get; set; }
        public User? RequestedBy { get; set; }

        /// <summary>What the requesting chef is offering in exchange, for Swap-type listings. Null for a plain purchase.</summary>
        [MaxLength(200)]
        public string? OfferedItem { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending / Accepted / Declined / Completed

        [MaxLength(300)]
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
