using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M77: Live cooking show / chef streaming
    //
    // Honest scope: actual video ingest/playback (RTMP/HLS/WebRTC) needs a
    // dedicated streaming infra provider (e.g. Mux, Agora, AWS IVS) — that's
    // a real infra integration, not something this API project can host
    // itself. This builds the metadata layer any such provider would sit
    // behind: scheduling, viewer/comment tracking, and a StreamUrl field
    // that's a placeholder until a real provider is wired (new gap, G10).
    // ══════════════════════════════════════════════════════════════
    public class LiveStream
    {
        [Key] public int Id { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(100)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime ScheduledAt { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled / Live / Ended / Cancelled

        /// <summary>Real Mux HLS playback URL, set by StartAsync once the provider call succeeds.</summary>
        public string? StreamUrl { get; set; }

        /// <summary>RTMP ingest URL + stream key the chef's broadcasting app pushes video to — shown only to the chef, never to viewers.</summary>
        public string? IngestUrl { get; set; }
        public string? StreamKey { get; set; }

        public int ViewerCount { get; set; } = 0;
        public int PeakViewerCount { get; set; } = 0;

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class StreamComment
    {
        [Key] public int Id { get; set; }

        public int StreamId { get; set; }
        public LiveStream? Stream { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(300)]
        public string Comment { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M78: Gamified chef leaderboard with seasonal rewards
    // ══════════════════════════════════════════════════════════════
    public class LeaderboardSeason
    {
        [Key] public int Id { get; set; }

        [MaxLength(60)]
        public string Name { get; set; } = "";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChefLeaderboardEntry
    {
        [Key] public int Id { get; set; }

        public int SeasonId { get; set; }
        public LeaderboardSeason? Season { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        public int Points { get; set; } = 0;
        public int Rank { get; set; } = 0;
        public int BookingsCompleted { get; set; } = 0;
        public decimal AverageRating { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LeaderboardReward
    {
        [Key] public int Id { get; set; }

        public int SeasonId { get; set; }
        public LeaderboardSeason? Season { get; set; }

        public int RankFrom { get; set; }
        public int RankTo { get; set; }

        [MaxLength(200)]
        public string RewardDescription { get; set; } = "";

        [MaxLength(20)]
        public string RewardType { get; set; } = "Badge"; // Cash / Badge / Coupon
    }
}
