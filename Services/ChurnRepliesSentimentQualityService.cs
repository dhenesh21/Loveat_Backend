using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // Note: M113 (Churn Prediction) already exists in ChurnVoiceService.cs
    // (the earlier M71 module) — nothing new needed here.

    // ── M114: AI Suite — Suggested Replies ─────────────────────────
    public class SuggestedRepliesService
    {
        private readonly AppDbContext _db;
        public SuggestedRepliesService(AppDbContext db) => _db = db;

        public async Task<ReplyTemplateDto> CreateAsync(CreateReplyTemplateRequestDto req)
        {
            var t = new ReplyTemplate { Category = req.Category, Title = req.Title, Body = req.Body, Keywords = req.Keywords };
            _db.ReplyTemplates.Add(t);
            await _db.SaveChangesAsync();
            return ToDto(t);
        }

        public async Task<List<ReplyTemplateDto>> GetAllAsync() => (await _db.ReplyTemplates.OrderByDescending(t => t.UsageCount).ToListAsync()).Select(ToDto).ToList();

        // Matches templates for the ticket's category (or "All"), scoring
        // keyword overlap against the ticket subject + description text,
        // then ranks by (keyword matches, usage count) descending.
        public async Task<List<ReplyTemplateDto>> SuggestAsync(SuggestRepliesRequestDto req)
        {
            var ticket = await _db.SupportTickets.FindAsync(req.TicketId);
            if (ticket == null) return new List<ReplyTemplateDto>();

            var haystack = $"{ticket.Subject} {ticket.Description}".ToLowerInvariant();
            var candidates = await _db.ReplyTemplates.Where(t => t.IsActive && (t.Category == ticket.Category || t.Category == "All")).ToListAsync();

            var ranked = candidates.Select(t =>
            {
                var keywordHits = (t.Keywords ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Count(k => haystack.Contains(k.ToLowerInvariant()));
                return (Template: t, KeywordHits: keywordHits);
            })
            .OrderByDescending(x => x.KeywordHits).ThenByDescending(x => x.Template.UsageCount)
            .Take(req.Take <= 0 ? 3 : req.Take)
            .Select(x => x.Template).ToList();

            return ranked.Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message)> RecordUsageAsync(int templateId)
        {
            var t = await _db.ReplyTemplates.FindAsync(templateId);
            if (t == null) return (false, "Template not found.");
            t.UsageCount += 1;
            await _db.SaveChangesAsync();
            return (true, "Usage recorded.");
        }

        private static ReplyTemplateDto ToDto(ReplyTemplate t) => new() { Id = t.Id, Category = t.Category, Title = t.Title, Body = t.Body, Keywords = t.Keywords, UsageCount = t.UsageCount, IsActive = t.IsActive };
    }

    // ── M115: AI Suite — Sentiment Analysis ────────────────────────
    public class SentimentAnalysisService
    {
        private readonly AppDbContext _db;
        // Small fixed lexicon — intentionally simple and transparent rather
        // than a trained model, consistent with the rest of the AI Suite.
        private static readonly string[] PositiveWords = { "great", "excellent", "amazing", "love", "delicious", "perfect", "wonderful", "best", "friendly", "fresh", "tasty", "recommend", "fantastic", "awesome" };
        private static readonly string[] NegativeWords = { "bad", "terrible", "awful", "worst", "late", "cold", "rude", "disappointed", "poor", "horrible", "never", "cancel", "refund", "disgusting", "unhygienic" };

        public SentimentAnalysisService(AppDbContext db) => _db = db;

        public async Task<SentimentScoreDto> AnalyzeAsync(AnalyzeSentimentRequestDto req)
        {
            var lower = (req.Text ?? "").ToLowerInvariant();
            var posHits = PositiveWords.Where(w => lower.Contains(w)).ToList();
            var negHits = NegativeWords.Where(w => lower.Contains(w)).ToList();

            var raw = posHits.Count - negHits.Count;
            var total = posHits.Count + negHits.Count;
            var score = total == 0 ? 0m : Math.Clamp((decimal)raw / total, -1m, 1m);
            var label = score > 0.2m ? "Positive" : score < -0.2m ? "Negative" : "Neutral";
            var matched = string.Join(", ", posHits.Concat(negHits));

            var existing = await _db.SentimentScores.FirstOrDefaultAsync(s => s.SourceType == req.SourceType && s.SourceId == req.SourceId);
            if (existing == null) { existing = new SentimentScore { SourceType = req.SourceType, SourceId = req.SourceId }; _db.SentimentScores.Add(existing); }
            existing.Score = score;
            existing.Label = label;
            existing.MatchedKeywords = matched;
            existing.AnalyzedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new SentimentScoreDto { SourceType = req.SourceType, SourceId = req.SourceId, Score = score, Label = label, MatchedKeywords = matched, AnalyzedAt = existing.AnalyzedAt };
        }

        // Sweeps recent Review comments that haven't been scored yet.
        public async Task<int> BackfillReviewsAsync(int maxReviews = 200)
        {
            var scoredIds = await _db.SentimentScores.Where(s => s.SourceType == "Review").Select(s => s.SourceId).ToListAsync();
            var reviews = await _db.Reviews.Where(r => r.Comment != null && r.Comment != "" && !scoredIds.Contains(r.Id)).Take(maxReviews).ToListAsync();
            foreach (var r in reviews) await AnalyzeAsync(new AnalyzeSentimentRequestDto { SourceType = "Review", SourceId = r.Id, Text = r.Comment! });
            return reviews.Count;
        }

        public async Task<SentimentSummaryDto> GetSummaryAsync(string sourceType)
        {
            var scores = await _db.SentimentScores.Where(s => s.SourceType == sourceType).ToListAsync();
            return new SentimentSummaryDto
            {
                TotalAnalyzed = scores.Count, Positive = scores.Count(s => s.Label == "Positive"), Neutral = scores.Count(s => s.Label == "Neutral"),
                Negative = scores.Count(s => s.Label == "Negative"), AverageScore = scores.Count == 0 ? 0 : Math.Round((double)scores.Average(s => s.Score), 2),
            };
        }
    }

    // ── M116: AI Suite — Chef Quality Scoring ──────────────────────
    public class ChefQualityScoringService
    {
        private readonly AppDbContext _db;
        public ChefQualityScoringService(AppDbContext db) => _db = db;

        // Transparent weighted formula: 40% rating, 30% completion rate,
        // 20% inverse cancellation rate, 10% responsiveness proxy (share of
        // bookings accepted rather than left pending/expired).
        public async Task<(bool Success, string Message, ChefQualityScoreDto? Score)> ComputeAsync(int chefId)
        {
            var chef = await _db.ChefProfiles.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == chefId);
            if (chef == null) return (false, "Chef not found.", null);

            var bookings = await _db.Bookings.Where(b => b.ChefId == chefId).ToListAsync();
            var total = bookings.Count;
            var completed = bookings.Count(b => b.Status == "Completed");
            var cancelled = bookings.Count(b => b.Status == "Cancelled");
            var accepted = bookings.Count(b => b.Status is "Accepted" or "InProgress" or "Completed");

            var ratingComponent = Math.Round(chef.AverageRating / 5m * 100, 1);
            var completionComponent = total == 0 ? 50 : Math.Round((decimal)completed / total * 100, 1);
            var cancellationPenalty = total == 0 ? 0 : Math.Round((decimal)cancelled / total * 100, 1);
            var responsivenessComponent = total == 0 ? 50 : Math.Round((decimal)accepted / total * 100, 1);

            var overall = Math.Clamp(ratingComponent * 0.4m + completionComponent * 0.3m + responsivenessComponent * 0.1m - cancellationPenalty * 0.2m + 20m, 0, 100);
            var tier = overall >= 85 ? "Elite" : overall >= 65 ? "Good" : overall >= 40 ? "Standard" : "AtRisk";

            var record = new ChefQualityScore
            {
                ChefId = chefId, RatingComponent = ratingComponent, CompletionComponent = completionComponent,
                CancellationPenalty = cancellationPenalty, ResponsivenessComponent = responsivenessComponent, OverallScore = overall, Tier = tier,
            };
            _db.ChefQualityScores.Add(record);
            await _db.SaveChangesAsync();

            return (true, "Computed.", new ChefQualityScoreDto
            {
                ChefId = chefId, ChefName = chef.User?.FullName, RatingComponent = ratingComponent, CompletionComponent = completionComponent,
                CancellationPenalty = cancellationPenalty, ResponsivenessComponent = responsivenessComponent, OverallScore = overall, Tier = tier, ComputedAt = record.ComputedAt,
            });
        }

        public async Task<List<ChefQualityLeaderboardEntryDto>> GetLeaderboardAsync(int take = 20)
        {
            var latestPerChef = await _db.ChefQualityScores.Include(s => s.Chef)
                .GroupBy(s => s.ChefId).Select(g => g.OrderByDescending(s => s.ComputedAt).First())
                .OrderByDescending(s => s.OverallScore).Take(take).ToListAsync();
            return latestPerChef.Select(s => new ChefQualityLeaderboardEntryDto { ChefId = s.ChefId, ChefName = s.Chef?.FullName, OverallScore = s.OverallScore, Tier = s.Tier }).ToList();
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/suggested-replies"), Authorize(Roles = "Admin")]
    public class SuggestedRepliesController : ControllerBase
    {
        private readonly Services.SuggestedRepliesService _svc;
        public SuggestedRepliesController(Services.SuggestedRepliesService svc) => _svc = svc;

        [HttpPost("templates")]
        public async Task<IActionResult> Create([FromBody] CreateReplyTemplateRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet("templates")]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("suggest")]
        public async Task<IActionResult> Suggest([FromBody] SuggestRepliesRequestDto req) => Ok(new { success = true, data = await _svc.SuggestAsync(req) });

        [HttpPost("use")]
        public async Task<IActionResult> Use([FromBody] UseTemplateRequestDto req)
        {
            var (success, message) = await _svc.RecordUsageAsync(req.TemplateId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/sentiment-analysis"), Authorize(Roles = "Admin")]
    public class SentimentAnalysisController : ControllerBase
    {
        private readonly Services.SentimentAnalysisService _svc;
        public SentimentAnalysisController(Services.SentimentAnalysisService svc) => _svc = svc;

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] AnalyzeSentimentRequestDto req) => Ok(new { success = true, data = await _svc.AnalyzeAsync(req) });

        [HttpPost("backfill-reviews")]
        public async Task<IActionResult> Backfill([FromQuery] int maxReviews = 200) => Ok(new { success = true, data = new { analyzed = await _svc.BackfillReviewsAsync(maxReviews) } });

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string sourceType = "Review") => Ok(new { success = true, data = await _svc.GetSummaryAsync(sourceType) });
    }

    [ApiController, Route("api/chef-quality-scoring"), Authorize(Roles = "Admin")]
    public class ChefQualityScoringController : ControllerBase
    {
        private readonly Services.ChefQualityScoringService _svc;
        public ChefQualityScoringController(Services.ChefQualityScoringService svc) => _svc = svc;

        [HttpPost("compute")]
        public async Task<IActionResult> Compute([FromBody] ComputeChefQualityRequestDto req)
        {
            var (success, message, score) = await _svc.ComputeAsync(req.ChefId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = score });
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int take = 20) => Ok(new { success = true, data = await _svc.GetLeaderboardAsync(take) });
    }
}
