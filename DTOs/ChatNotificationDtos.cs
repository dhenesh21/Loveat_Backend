namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M17: Chat
    // ══════════════════════════════════════════════════════════════
    public class SendMessageRequestDto
    {
        public int ReceiverId { get; set; }
        public int? BookingId { get; set; }
        public string MessageText { get; set; } = "";
        public string MessageType { get; set; } = "Text"; // Text / Image
        public string? AttachmentUrl { get; set; }
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int? BookingId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = "";
        public int ReceiverId { get; set; }
        public string MessageType { get; set; } = "";
        public string MessageText { get; set; } = "";
        public string? AttachmentUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChatThreadDto
    {
        public int ThreadId { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = "";
        public string? OtherUserImageUrl { get; set; }
        public int? BookingId { get; set; }
        public string LastMessageText { get; set; } = "";
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M18: Notifications
    // ══════════════════════════════════════════════════════════════
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string Type { get; set; } = "";
        public string? DataJson { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
