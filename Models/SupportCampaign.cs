using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── Module 31: Support & Help Center ──────────────────────────
    public class SupportTicket
    {
        [Key] public int Id { get; set; }

        [Required] public int UserId { get; set; }
        public User? User { get; set; }

        public string TicketNumber { get; set; } = ""; // e.g. TKT-00123
        [Required, MaxLength(200)]
        public string Subject     { get; set; } = "";
        public string Category    { get; set; } = "General"; // General / Booking / Payment / Chef / Technical / Other
        public string Priority    { get; set; } = "Medium";  // Low / Medium / High / Urgent
        public string Status      { get; set; } = "Open";    // Open / InProgress / Resolved / Closed
        public string Description { get; set; } = "";
        public int?   BookingId   { get; set; }
        public int?   AssignedAdminId { get; set; }
        public string? Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int?   Rating      { get; set; } // 1-5 satisfaction
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
    }

    public class SupportMessage
    {
        [Key] public int Id { get; set; }

        public int TicketId { get; set; }
        public SupportTicket? Ticket { get; set; }

        public int    SenderId  { get; set; }
        public string SenderRole{ get; set; } = "User"; // User / Admin
        public string Message   { get; set; } = "";
        public bool   IsInternal{ get; set; } = false;  // Internal admin note
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FAQ
    {
        [Key] public int Id { get; set; }

        public string Question  { get; set; } = "";
        public string Answer    { get; set; } = "";
        public string Category  { get; set; } = "General";
        public string Audience  { get; set; } = "All"; // All / Customer / Chef
        public int    SortOrder { get; set; } = 0;
        public bool   IsActive  { get; set; } = true;
        public int    HelpfulCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── Module 32: Push Notification Campaigns ────────────────────
    public class NotificationCampaign
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title       { get; set; } = "";
        public string Body        { get; set; } = "";
        public string Target      { get; set; } = "All";    // All / Customer / Chef / City
        public string? TargetCity { get; set; }
        public string Type        { get; set; } = "Promo";  // Promo / System / Reminder / Seasonal
        public string Status      { get; set; } = "Draft";  // Draft / Scheduled / Sent / Cancelled
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt    { get; set; }
        public int  TotalReach    { get; set; } = 0;
        public int  DeliveredCount{ get; set; } = 0;
        public int  OpenedCount   { get; set; } = 0;
        public int? CreatedByAdminId { get; set; }
        public string? DeepLink   { get; set; }             // e.g. loveat://loyalty
        public string? ImageUrl   { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
