namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M77: Live streaming
    // ══════════════════════════════════════════════════════════════
    public class ScheduleStreamRequestDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime ScheduledAt { get; set; }
    }

    public class LiveStreamDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public string? ChefImageUrl { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = "";
        public string? StreamUrl { get; set; }
        public string? IngestUrl { get; set; } // only populated for the streaming chef's own view — never sent to viewers
        public int ViewerCount { get; set; }
        public bool IsStub { get; set; } = true; // true when the streaming provider isn't configured — StreamUrl won't play real video
    }

    public class PostStreamCommentRequestDto
    {
        public string Comment { get; set; } = "";
    }

    public class StreamCommentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M78: Leaderboard
    // ══════════════════════════════════════════════════════════════
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public string? ChefImageUrl { get; set; }
        public int Points { get; set; }
        public int BookingsCompleted { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class LeaderboardRewardDto
    {
        public int RankFrom { get; set; }
        public int RankTo { get; set; }
        public string RewardDescription { get; set; } = "";
        public string RewardType { get; set; } = "";
    }

    public class SeasonLeaderboardDto
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<LeaderboardEntryDto> Entries { get; set; } = new();
        public List<LeaderboardRewardDto> Rewards { get; set; } = new();
    }
}
