using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LovEat.API.Models
{
    // ── Loyalty Points ─────────────────────────────────────────────
    public class LoyaltyPoints
    {
        [Key] public int Id { get; set; }

        [Required] public int UserId { get; set; }
        public User? User { get; set; }

        public int TotalPoints     { get; set; } = 0;
        public int RedeemedPoints  { get; set; } = 0;
        public int PendingPoints   { get; set; } = 0;

        [NotMapped]
        public int AvailablePoints => TotalPoints - RedeemedPoints;

        public string Tier { get; set; } = "Bronze"; // Bronze / Silver / Gold / Platinum
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LoyaltyTransaction
    {
        [Key] public int Id { get; set; }

        [Required] public int UserId { get; set; }
        public User? User { get; set; }

        public int    Points      { get; set; }            // positive = earned, negative = redeemed
        public string Type        { get; set; } = "Earned"; // Earned / Redeemed / Expired / Bonus
        public string Reason      { get; set; } = "";       // "Booking #123", "Welcome Bonus", etc.
        public int?   BookingId   { get; set; }
        public int    BalanceAfter{ get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── Referral System ────────────────────────────────────────────
    public class ReferralCode
    {
        [Key] public int Id { get; set; }

        [Required] public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = "";       // e.g. ARJUN123

        public int  TotalReferrals    { get; set; } = 0;
        public int  SuccessfulReferrals{ get; set; } = 0;
        public int  PointsEarned      { get; set; } = 0;

        public bool IsActive  { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ReferralUse> Uses { get; set; } = new List<ReferralUse>();
    }

    public class ReferralUse
    {
        [Key] public int Id { get; set; }

        public int ReferralCodeId { get; set; }
        public ReferralCode? ReferralCode { get; set; }

        public int ReferredUserId { get; set; }
        public User? ReferredUser { get; set; }

        public string Status    { get; set; } = "Pending";  // Pending / Completed / Invalid
        public int    Points    { get; set; } = 0;          // Points awarded on completion
        public DateTime UsedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
