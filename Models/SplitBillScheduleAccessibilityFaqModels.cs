using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M153: Customer Experience — Split Bill / Group Payment ──────
    public class BillSplit
    {
        [Key] public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public int InitiatedByCustomerId { get; set; }
        public User? InitiatedByCustomer { get; set; }
        public decimal TotalAmount { get; set; }
        [MaxLength(20)] public string SplitMethod { get; set; } = "Equal"; // Equal / Custom
        [MaxLength(20)] public string Status { get; set; } = "Pending"; // Pending / PartiallyPaid / Completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BillSplitParticipant
    {
        [Key] public int Id { get; set; }
        public int BillSplitId { get; set; }
        public BillSplit? BillSplit { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public decimal ShareAmount { get; set; }
        [MaxLength(20)] public string PaymentStatus { get; set; } = "Pending"; // Pending / Paid
        public DateTime? PaidAt { get; set; }
    }

    // ── M154: Customer Experience — Meal Scheduling Assistant ───────
    public class MealSchedule
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }
        [MaxLength(80)] public string ScheduleName { get; set; } = "";
        public int DayOfWeek { get; set; } // 0=Sun..6=Sat
        public TimeSpan PreferredTime { get; set; }
        public decimal EstimatedAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastGeneratedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M155: Customer Experience — Accessibility Settings ──────────
    public class AccessibilitySettings
    {
        [Key] public int Id { get; set; }
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        [MaxLength(10)] public string FontSize { get; set; } = "Medium"; // Small / Medium / Large / ExtraLarge
        public bool HighContrastMode { get; set; } = false;
        public bool ScreenReaderOptimized { get; set; } = false;
        public bool ReduceMotion { get; set; } = false;
        public bool VoiceGuidanceEnabled { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M156: Customer Experience — FAQ / Help Center ───────────────
    public class FaqCategory
    {
        [Key] public int Id { get; set; }
        [MaxLength(60)] public string Name { get; set; } = "";
        [MaxLength(40)] public string IconName { get; set; } = "help-circle";
        public int SortOrder { get; set; } = 0;
    }

    public class FaqArticle
    {
        [Key] public int Id { get; set; }
        public int FaqCategoryId { get; set; }
        public FaqCategory? FaqCategory { get; set; }
        [MaxLength(200)] public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        [MaxLength(20)] public string Audience { get; set; } = "All"; // All / Customer / Chef
        public int ViewCount { get; set; } = 0;
        public int HelpfulCount { get; set; } = 0;
        public int NotHelpfulCount { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
