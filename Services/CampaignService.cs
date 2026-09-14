using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class CampaignService
    {
        private readonly AppDbContext _db;
        public CampaignService(AppDbContext db) => _db = db;

        // ── Create campaign ────────────────────────────────────────
        public async Task<CampaignDto> CreateAsync(int adminId, CreateCampaignDto dto)
        {
            // Estimate reach based on target
            var reach = dto.Target switch
            {
                "Customer" => await _db.Users.CountAsync(u => u.Role == "Customer"),
                "Chef"     => await _db.Users.CountAsync(u => u.Role == "Chef"),
                "City"     => await _db.UserProfiles.CountAsync(p => p.City == dto.TargetCity),
                _          => await _db.Users.CountAsync(),
            };

            var campaign = new NotificationCampaign
            {
                Title           = dto.Title,
                Body            = dto.Body,
                Target          = dto.Target,
                TargetCity      = dto.TargetCity,
                Type            = dto.Type,
                Status          = dto.ScheduledAt.HasValue ? "Scheduled" : "Draft",
                ScheduledAt     = dto.ScheduledAt,
                TotalReach      = reach,
                DeepLink        = dto.DeepLink,
                ImageUrl        = dto.ImageUrl,
                CreatedByAdminId= adminId,
            };
            _db.NotificationCampaigns.Add(campaign);
            await _db.SaveChangesAsync();
            return MapCampaign(campaign);
        }

        // ── Send campaign immediately ──────────────────────────────
        public async Task<CampaignDto?> SendNowAsync(int campaignId)
        {
            var campaign = await _db.NotificationCampaigns.FindAsync(campaignId);
            if (campaign == null) return null;

            // In production: call FCM / Expo Push / OneSignal here
            campaign.Status         = "Sent";
            campaign.SentAt         = DateTime.UtcNow;
            campaign.DeliveredCount = (int)(campaign.TotalReach * 0.97); // 97% delivery sim
            campaign.OpenedCount    = (int)(campaign.DeliveredCount * 0.68); // 68% open rate sim
            await _db.SaveChangesAsync();
            return MapCampaign(campaign);
        }

        // ── Get all campaigns ──────────────────────────────────────
        public async Task<List<CampaignDto>> GetAllAsync()
        {
            var list = await _db.NotificationCampaigns
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .ToListAsync();
            return list.Select(MapCampaign).ToList();
        }

        // ── Get single ────────────────────────────────────────────
        public async Task<CampaignDto?> GetByIdAsync(int id)
        {
            var c = await _db.NotificationCampaigns.FindAsync(id);
            return c == null ? null : MapCampaign(c);
        }

        // ── Delete/cancel campaign ─────────────────────────────────
        public async Task<bool> CancelAsync(int id)
        {
            var c = await _db.NotificationCampaigns.FindAsync(id);
            if (c == null || c.Status == "Sent") return false;
            c.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Overall stats ──────────────────────────────────────────
        public async Task<CampaignStatsDto> GetStatsAsync()
        {
            var all       = await _db.NotificationCampaigns.ToListAsync();
            var thisMonth = DateTime.UtcNow.AddDays(-30);
            var sent      = all.Where(c => c.Status == "Sent").ToList();

            return new CampaignStatsDto
            {
                TotalCampaigns = all.Count,
                SentThisMonth  = sent.Count(c => c.SentAt >= thisMonth),
                TotalReach     = sent.Sum(c => (long)c.TotalReach),
                AvgOpenRate    = sent.Any()
                    ? Math.Round(sent.Average(c => c.DeliveredCount > 0
                        ? (double)c.OpenedCount / c.DeliveredCount * 100 : 0), 1)
                    : 0,
                Recent = all.OrderByDescending(c => c.CreatedAt).Take(10).Select(MapCampaign).ToList(),
            };
        }

        // ── Mapper ─────────────────────────────────────────────────
        private static CampaignDto MapCampaign(NotificationCampaign c) => new()
        {
            Id            = c.Id,
            Title         = c.Title,
            Body          = c.Body,
            Target        = c.Target,
            TargetCity    = c.TargetCity,
            Type          = c.Type,
            Status        = c.Status,
            ScheduledAt   = c.ScheduledAt?.ToString("MMM dd, yyyy hh:mm tt"),
            SentAt        = c.SentAt?.ToString("MMM dd, yyyy hh:mm tt"),
            TotalReach    = c.TotalReach,
            DeliveredCount= c.DeliveredCount,
            OpenedCount   = c.OpenedCount,
            OpenRate      = c.DeliveredCount > 0 ? Math.Round((double)c.OpenedCount / c.DeliveredCount * 100, 1) : 0,
            DeepLink      = c.DeepLink,
            CreatedAt     = c.CreatedAt.ToString("MMM dd, yyyy"),
        };
    }
}
