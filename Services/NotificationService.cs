using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// M18: notification feed. CreateAsync is called internally by other
    /// services (booking status changes, payment confirmations, etc.) to
    /// write a row here. Push delivery via FCM — P1 hardening, replacing the
    /// previous Console.WriteLine stub in PushToDeviceAsync.
    /// </summary>
    public class NotificationService
    {
        private readonly AppDbContext _db;
        private readonly IPushNotificationService _push;
        public NotificationService(AppDbContext db, IPushNotificationService push) { _db = db; _push = push; }

        public async Task CreateAsync(int userId, string title, string body, string type, string? dataJson = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Type = type,
                DataJson = dataJson,
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await PushToDeviceAsync(userId, title, body);
        }

        public async Task<List<NotificationDto>> GetMyNotificationsAsync(int userId, bool unreadOnly = false, int take = 50)
        {
            var query = _db.Notifications.Where(n => n.UserId == userId);
            if (unreadOnly) query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    Type = n.Type,
                    DataJson = n.DataJson,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
            => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task MarkReadAsync(int userId, int notificationId)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
            if (n == null) return;
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync(int userId)
        {
            var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _db.SaveChangesAsync();
        }

        public async Task RegisterDeviceTokenAsync(int userId, string token, string platform)
        {
            var existing = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.UserId == userId && d.Token == token);
            if (existing != null)
            {
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.DeviceTokens.Add(new DeviceToken { UserId = userId, Token = token, Platform = platform });
            }
            await _db.SaveChangesAsync();
        }

        /// <summary>Delivers to every active device the user has registered. Failures for individual devices are logged but don't block the others — the in-app feed row (already written by CreateAsync) is always the source of truth regardless of push delivery outcome.</summary>
        private async Task PushToDeviceAsync(int userId, string title, string body)
        {
            var tokens = await _db.DeviceTokens.Where(t => t.UserId == userId && t.IsActive).ToListAsync();
            foreach (var t in tokens)
            {
                var (sent, _) = await _push.SendAsync(t.Token, title, body);
                if (!sent) continue; // logged inside IPushNotificationService; a failed push shouldn't throw and block the caller's flow (e.g. booking confirmation)
            }
        }
    }
}
