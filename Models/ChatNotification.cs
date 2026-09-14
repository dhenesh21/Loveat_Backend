using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>M17: real-time chat between a customer and chef, usually scoped to a booking. Persisted here; delivered live via ChatHub (Phase 5, SignalR) once built.</summary>
    public class ChatMessage
    {
        [Key] public int Id { get; set; }

        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int SenderId { get; set; }
        public User? Sender { get; set; }

        public int ReceiverId { get; set; }
        public User? Receiver { get; set; }

        [MaxLength(20)]
        public string MessageType { get; set; } = "Text"; // Text / Image / System

        [MaxLength(2000)]
        public string MessageText { get; set; } = "";

        public string? AttachmentUrl { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Lightweight thread summary so ChatListScreen doesn't need to scan every message.</summary>
    public class ChatThread
    {
        [Key] public int Id { get; set; }

        public int User1Id { get; set; }
        public User? User1 { get; set; }

        public int User2Id { get; set; }
        public User? User2 { get; set; }

        public int? BookingId { get; set; }

        public int LastMessageId { get; set; }
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public int UnreadCountUser1 { get; set; } = 0;
        public int UnreadCountUser2 { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>M18: push/in-app notification feed. Actual FCM delivery is a separate infra gap (G2) — this table is the source of truth regardless of delivery channel.</summary>
    public class Notification
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(100)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string Body { get; set; } = "";

        [MaxLength(30)]
        public string Type { get; set; } = "General"; // BookingUpdate / Payment / Chat / Promo / System

        /// <summary>JSON payload for deep-linking, e.g. {"bookingId": 42}</summary>
        public string? DataJson { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsPushSent { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
