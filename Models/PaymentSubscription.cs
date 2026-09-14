using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>M14: a single payment attempt/transaction against a booking.</summary>
    public class Payment
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(20)]
        public string Method { get; set; } = "UPI"; // UPI / Card / NetBanking / Wallet

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending / Success / Failed / Refunded

        /// <summary>Gateway transaction reference (Razorpay/Stripe order/payment id). Populated once G1 is wired.</summary>
        [MaxLength(100)]
        public string? GatewayTransactionRef { get; set; }

        [MaxLength(50)]
        public string Gateway { get; set; } = "Razorpay";

        public string? GatewayResponseJson { get; set; }

        [MaxLength(300)]
        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>M15: in-app wallet ledger. User.WalletBalance is the running total; every change must write a row here.</summary>
    public class WalletTransaction
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(20)]
        public string Type { get; set; } = "Credit"; // Credit / Debit

        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }

        [MaxLength(30)]
        public string Reason { get; set; } = ""; // BookingRefund / TopUp / Payout / PromoCredit / BookingPayment

        public int? RelatedBookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>M19: customer subscription plans (weekly meal plans, not the corporate ones in DeviceSecurityCorporateModels.cs).</summary>
    public class Subscription
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(50)]
        public string PlanName { get; set; } = "";

        public int MealsPerWeek { get; set; }
        public decimal PricePerMonth { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active / Paused / Cancelled

        // ── M182: Dedicated/Same-Chef Monthly Assignment ────────────
        public int? PreferredChefId { get; set; }
        public User? PreferredChef { get; set; }
        public bool AutoAssignSameChef { get; set; } = false; // defaults true client-side once PreferredChefId is set
        [MaxLength(20)]
        public string FallbackPolicy { get; set; } = "NotifyCustomer"; // NotifyCustomer / AutoAssignAlternate

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime NextBillingDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
