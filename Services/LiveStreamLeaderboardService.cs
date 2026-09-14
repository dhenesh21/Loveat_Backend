using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // M77: Live Streaming Service
    // ══════════════════════════════════════════════════════════════
    public class LiveStreamService
    {
        private readonly AppDbContext _db;
        private readonly IStreamingProviderService _streaming;
        public LiveStreamService(AppDbContext db, IStreamingProviderService streaming) { _db = db; _streaming = streaming; }

        public async Task<LiveStreamDto> ScheduleAsync(int chefId, ScheduleStreamRequestDto req)
        {
            var stream = new LiveStream
            {
                ChefId = chefId,
                Title = req.Title,
                Description = req.Description,
                ScheduledAt = req.ScheduledAt,
            };
            _db.LiveStreams.Add(stream);
            await _db.SaveChangesAsync();

            stream.Chef = await _db.Users.FindAsync(chefId);
            return ToDto(stream);
        }

        public async Task<List<LiveStreamDto>> GetUpcomingAsync(int take = 20)
        {
            var streams = await _db.LiveStreams.Include(s => s.Chef)
                .Where(s => s.Status == "Scheduled" && s.ScheduledAt >= DateTime.UtcNow)
                .OrderBy(s => s.ScheduledAt).Take(take).ToListAsync();
            return streams.Select(ToDto).ToList();
        }

        public async Task<List<LiveStreamDto>> GetLiveNowAsync()
        {
            var streams = await _db.LiveStreams.Include(s => s.Chef).Where(s => s.Status == "Live").ToListAsync();
            return streams.Select(ToDto).ToList();
        }

        /// <summary>Creates a real Mux live stream and stores its ingest/playback URLs — P1 hardening, replacing the previous stub:// fake URL.</summary>
        public async Task<(bool Success, string Message)> StartAsync(int chefId, int streamId)
        {
            var stream = await _db.LiveStreams.FirstOrDefaultAsync(s => s.Id == streamId && s.ChefId == chefId);
            if (stream == null) return (false, "Stream not found.");
            if (stream.Status != "Scheduled") return (false, $"Stream can't be started from status '{stream.Status}'.");

            var (success, ingestUrl, streamKey, playbackUrl, error) = await _streaming.CreateLiveStreamAsync();
            if (!success)
                return (false, error ?? "Could not start the live stream.");

            stream.Status = "Live";
            stream.StartedAt = DateTime.UtcNow;
            stream.StreamUrl = playbackUrl;
            stream.IngestUrl = ingestUrl;
            stream.StreamKey = streamKey;
            await _db.SaveChangesAsync();
            return (true, "You're live! Point your broadcasting app at the ingest URL to go live.");
        }

        public async Task<(bool Success, string Message)> EndAsync(int chefId, int streamId)
        {
            var stream = await _db.LiveStreams.FirstOrDefaultAsync(s => s.Id == streamId && s.ChefId == chefId);
            if (stream == null) return (false, "Stream not found.");
            if (stream.Status != "Live") return (false, "Stream is not currently live.");

            stream.Status = "Ended";
            stream.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Stream ended.");
        }

        public async Task<(bool Success, int ViewerCount)> JoinAsync(int streamId)
        {
            var stream = await _db.LiveStreams.FirstOrDefaultAsync(s => s.Id == streamId && s.Status == "Live");
            if (stream == null) return (false, 0);

            stream.ViewerCount++;
            if (stream.ViewerCount > stream.PeakViewerCount) stream.PeakViewerCount = stream.ViewerCount;
            await _db.SaveChangesAsync();
            return (true, stream.ViewerCount);
        }

        public async Task<(bool Success, string Message, StreamCommentDto? Data)> PostCommentAsync(int userId, int streamId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return (false, "Comment can't be empty.", null);

            var stream = await _db.LiveStreams.FindAsync(streamId);
            if (stream == null) return (false, "Stream not found.", null);

            var entry = new StreamComment { StreamId = streamId, UserId = userId, Comment = comment };
            _db.StreamComments.Add(entry);
            await _db.SaveChangesAsync();

            var user = await _db.Users.FindAsync(userId);
            return (true, "Posted.", new StreamCommentDto { Id = entry.Id, UserId = userId, UserName = user?.FullName ?? "", Comment = comment, CreatedAt = entry.CreatedAt });
        }

        public async Task<List<StreamCommentDto>> GetCommentsAsync(int streamId, int take = 50)
        {
            var comments = await _db.StreamComments.Include(c => c.User)
                .Where(c => c.StreamId == streamId)
                .OrderByDescending(c => c.CreatedAt).Take(take).ToListAsync();

            return comments.Select(c => new StreamCommentDto
            {
                Id = c.Id, UserId = c.UserId, UserName = c.User?.FullName ?? "", Comment = c.Comment, CreatedAt = c.CreatedAt,
            }).OrderBy(c => c.CreatedAt).ToList();
        }

        private static LiveStreamDto ToDto(LiveStream s) => new()
        {
            Id = s.Id, ChefId = s.ChefId, ChefName = s.Chef?.FullName ?? "", ChefImageUrl = s.Chef?.ProfileImageUrl,
            Title = s.Title, Description = s.Description, ScheduledAt = s.ScheduledAt, Status = s.Status,
            StreamUrl = s.StreamUrl, IngestUrl = s.IngestUrl, ViewerCount = s.ViewerCount, IsStub = string.IsNullOrEmpty(s.IngestUrl) && s.Status != "Scheduled",
        };
    }

    // ══════════════════════════════════════════════════════════════
    // M78: Leaderboard Service
    // ══════════════════════════════════════════════════════════════
    public class LeaderboardService
    {
        private readonly AppDbContext _db;
        public LeaderboardService(AppDbContext db) => _db = db;

        public async Task<LeaderboardSeason> GetOrCreateCurrentSeasonAsync()
        {
            var now = DateTime.UtcNow;
            var season = await _db.LeaderboardSeasons.FirstOrDefaultAsync(s => s.IsActive && s.StartDate <= now && s.EndDate >= now);
            if (season != null) return season;

            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

            season = new LeaderboardSeason { Name = $"{monthStart:MMMM yyyy}", StartDate = monthStart, EndDate = monthEnd };
            _db.LeaderboardSeasons.Add(season);
            await _db.SaveChangesAsync();

            _db.LeaderboardRewards.AddRange(
                new LeaderboardReward { SeasonId = season.Id, RankFrom = 1, RankTo = 1, RewardDescription = "₹2000 cash bonus + Top Chef badge", RewardType = "Cash" },
                new LeaderboardReward { SeasonId = season.Id, RankFrom = 2, RankTo = 5, RewardDescription = "₹500 cash bonus", RewardType = "Cash" },
                new LeaderboardReward { SeasonId = season.Id, RankFrom = 6, RankTo = 20, RewardDescription = "20% off next platform commission (coupon)", RewardType = "Coupon" }
            );
            await _db.SaveChangesAsync();

            return season;
        }

        public async Task<SeasonLeaderboardDto> GetLeaderboardAsync(int? seasonId = null, int take = 50)
        {
            var season = seasonId.HasValue
                ? await _db.LeaderboardSeasons.FindAsync(seasonId.Value)
                : await GetOrCreateCurrentSeasonAsync();

            if (season == null) return new SeasonLeaderboardDto();

            await RecomputeAsync(season.Id);

            var entries = await _db.ChefLeaderboardEntries.Include(e => e.Chef)
                .Where(e => e.SeasonId == season.Id)
                .OrderBy(e => e.Rank)
                .Take(take)
                .ToListAsync();

            var rewards = await _db.LeaderboardRewards.Where(r => r.SeasonId == season.Id).OrderBy(r => r.RankFrom).ToListAsync();

            return new SeasonLeaderboardDto
            {
                SeasonId = season.Id,
                SeasonName = season.Name,
                StartDate = season.StartDate,
                EndDate = season.EndDate,
                Entries = entries.Select(e => new LeaderboardEntryDto
                {
                    Rank = e.Rank, ChefId = e.ChefId, ChefName = e.Chef?.FullName ?? "", ChefImageUrl = e.Chef?.ProfileImageUrl,
                    Points = e.Points, BookingsCompleted = e.BookingsCompleted, AverageRating = e.AverageRating,
                }).ToList(),
                Rewards = rewards.Select(r => new LeaderboardRewardDto
                {
                    RankFrom = r.RankFrom, RankTo = r.RankTo, RewardDescription = r.RewardDescription, RewardType = r.RewardType,
                }).ToList(),
            };
        }

        /// <summary>Recomputes every chef's points for a season from their completed bookings in that window: 10 points per completed booking + 5 points per average-rating star.</summary>
        private async Task RecomputeAsync(int seasonId)
        {
            var season = await _db.LeaderboardSeasons.FindAsync(seasonId);
            if (season == null) return;

            var completedBookings = await _db.Bookings
                .Where(b => b.Status == "Completed" && b.CompletedAt >= season.StartDate && b.CompletedAt <= season.EndDate)
                .ToListAsync();

            var byChef = completedBookings.GroupBy(b => b.ChefId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var (chefId, bookingsCompleted) in byChef)
            {
                var chefProfile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefId);
                var avgRating = chefProfile?.AverageRating ?? 0;
                var points = bookingsCompleted * 10 + (int)Math.Round(avgRating * 5);

                var entry = await _db.ChefLeaderboardEntries.FirstOrDefaultAsync(e => e.SeasonId == seasonId && e.ChefId == chefId);
                if (entry == null)
                {
                    entry = new ChefLeaderboardEntry { SeasonId = seasonId, ChefId = chefId };
                    _db.ChefLeaderboardEntries.Add(entry);
                }

                entry.Points = points;
                entry.BookingsCompleted = bookingsCompleted;
                entry.AverageRating = avgRating;
                entry.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();

            var ranked = await _db.ChefLeaderboardEntries.Where(e => e.SeasonId == seasonId).OrderByDescending(e => e.Points).ToListAsync();
            for (int i = 0; i < ranked.Count; i++) ranked[i].Rank = i + 1;
            await _db.SaveChangesAsync();
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/live-streams")]
    public class LiveStreamController : ControllerBase
    {
        private readonly Services.LiveStreamService _svc;
        public LiveStreamController(Services.LiveStreamService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Schedule([FromBody] ScheduleStreamRequestDto req)
        {
            var data = await _svc.ScheduleAsync(UserId, req);
            return Ok(new { success = true, data });
        }

        [HttpGet("upcoming")]
        [AllowAnonymous]
        public async Task<IActionResult> Upcoming([FromQuery] int take = 20)
        {
            var data = await _svc.GetUpcomingAsync(take);
            return Ok(new { success = true, data });
        }

        [HttpGet("live")]
        [AllowAnonymous]
        public async Task<IActionResult> Live()
        {
            var data = await _svc.GetLiveNowAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/start")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Start(int id)
        {
            var (success, message) = await _svc.StartAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("{id}/end")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> End(int id)
        {
            var (success, message) = await _svc.EndAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<IActionResult> Join(int id)
        {
            var (success, viewerCount) = await _svc.JoinAsync(id);
            if (!success) return BadRequest(new { success, message = "Stream is not live." });
            return Ok(new { success, viewerCount });
        }

        [HttpPost("{id}/comments")]
        [Authorize]
        public async Task<IActionResult> PostComment(int id, [FromBody] PostStreamCommentRequestDto req)
        {
            var (success, message, data) = await _svc.PostCommentAsync(UserId, id, req.Comment);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, data });
        }

        [HttpGet("{id}/comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int id, [FromQuery] int take = 50)
        {
            var data = await _svc.GetCommentsAsync(id, take);
            return Ok(new { success = true, data });
        }
    }

    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly Services.LeaderboardService _svc;
        public LeaderboardController(Services.LeaderboardService svc) => _svc = svc;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] int? seasonId, [FromQuery] int take = 50)
        {
            var data = await _svc.GetLeaderboardAsync(seasonId, take);
            return Ok(new { success = true, data });
        }
    }
}
