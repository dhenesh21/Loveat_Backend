using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M17: chat persistence, called both from ChatHub (live delivery) and ChatController (history/REST fallback for clients not connected to the hub yet).</summary>
    public class ChatService
    {
        private readonly AppDbContext _db;
        public ChatService(AppDbContext db) => _db = db;

        public async Task<ChatMessageDto?> SendMessageAsync(int senderId, SendMessageRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.MessageText) && string.IsNullOrWhiteSpace(req.AttachmentUrl))
                return null;

            var message = new ChatMessage
            {
                BookingId = req.BookingId,
                SenderId = senderId,
                ReceiverId = req.ReceiverId,
                MessageType = req.MessageType,
                MessageText = req.MessageText,
                AttachmentUrl = req.AttachmentUrl,
            };
            _db.ChatMessages.Add(message);

            await UpsertThreadAsync(senderId, req.ReceiverId, req.BookingId, message);

            await _db.SaveChangesAsync();

            var sender = await _db.Users.FindAsync(senderId);
            return new ChatMessageDto
            {
                Id = message.Id,
                BookingId = message.BookingId,
                SenderId = senderId,
                SenderName = sender?.FullName ?? "",
                ReceiverId = req.ReceiverId,
                MessageType = message.MessageType,
                MessageText = message.MessageText,
                AttachmentUrl = message.AttachmentUrl,
                IsRead = false,
                CreatedAt = message.CreatedAt,
            };
        }

        private async Task UpsertThreadAsync(int user1, int user2, int? bookingId, ChatMessage message)
        {
            var (a, b) = user1 < user2 ? (user1, user2) : (user2, user1);
            var thread = await _db.ChatThreads.FirstOrDefaultAsync(t => t.User1Id == a && t.User2Id == b);

            if (thread == null)
            {
                thread = new ChatThread { User1Id = a, User2Id = b, BookingId = bookingId };
                _db.ChatThreads.Add(thread);
            }

            thread.LastMessageAt = DateTime.UtcNow;
            if (message.ReceiverId == a) thread.UnreadCountUser1++;
            else thread.UnreadCountUser2++;
        }

        public async Task<List<ChatMessageDto>> GetHistoryAsync(int userId, int otherUserId, int take = 50)
        {
            var messages = await _db.ChatMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId)
                         || (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderByDescending(m => m.CreatedAt)
                .Take(take)
                .ToListAsync();

            messages.Reverse();

            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            var senders = await _db.Users.Where(u => senderIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName);

            return messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                BookingId = m.BookingId,
                SenderId = m.SenderId,
                SenderName = senders.GetValueOrDefault(m.SenderId, ""),
                ReceiverId = m.ReceiverId,
                MessageType = m.MessageType,
                MessageText = m.MessageText,
                AttachmentUrl = m.AttachmentUrl,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt,
            }).ToList();
        }

        public async Task<List<ChatThreadDto>> GetThreadsAsync(int userId)
        {
            var threads = await _db.ChatThreads
                .Where(t => t.User1Id == userId || t.User2Id == userId)
                .OrderByDescending(t => t.LastMessageAt)
                .ToListAsync();

            var result = new List<ChatThreadDto>();
            foreach (var t in threads)
            {
                var otherId = t.User1Id == userId ? t.User2Id : t.User1Id;
                var other = await _db.Users.FindAsync(otherId);
                var unread = t.User1Id == userId ? t.UnreadCountUser1 : t.UnreadCountUser2;

                var lastMessage = await _db.ChatMessages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == otherId) || (m.SenderId == otherId && m.ReceiverId == userId))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                result.Add(new ChatThreadDto
                {
                    ThreadId = t.Id,
                    OtherUserId = otherId,
                    OtherUserName = other?.FullName ?? "",
                    OtherUserImageUrl = other?.ProfileImageUrl,
                    BookingId = t.BookingId,
                    LastMessageText = lastMessage?.MessageText ?? "",
                    LastMessageAt = t.LastMessageAt,
                    UnreadCount = unread,
                });
            }

            return result;
        }

        public async Task MarkThreadReadAsync(int userId, int otherUserId)
        {
            var unread = await _db.ChatMessages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();
            foreach (var m in unread) m.IsRead = true;

            var (a, b) = userId < otherUserId ? (userId, otherUserId) : (otherUserId, userId);
            var thread = await _db.ChatThreads.FirstOrDefaultAsync(t => t.User1Id == a && t.User2Id == b);
            if (thread != null)
            {
                if (thread.User1Id == userId) thread.UnreadCountUser1 = 0;
                else thread.UnreadCountUser2 = 0;
            }

            await _db.SaveChangesAsync();
        }
    }
}
