using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M109: Internal Notes ───────────────────────────────────────
    public class InternalNoteService
    {
        private readonly AppDbContext _db;
        public InternalNoteService(AppDbContext db) => _db = db;

        public async Task<InternalNoteDto> CreateAsync(int adminUserId, CreateInternalNoteRequestDto req)
        {
            var note = new InternalNote { EntityType = req.EntityType, EntityId = req.EntityId, Note = req.Note, IsPinned = req.IsPinned, CreatedByAdminId = adminUserId };
            _db.InternalNotes.Add(note);
            await _db.SaveChangesAsync();
            var admin = await _db.Users.FindAsync(adminUserId);
            return ToDto(note, admin?.FullName);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(UpdateInternalNoteRequestDto req)
        {
            var note = await _db.InternalNotes.FindAsync(req.NoteId);
            if (note == null) return (false, "Note not found.");
            if (req.Note != null) note.Note = req.Note;
            if (req.IsPinned.HasValue) note.IsPinned = req.IsPinned.Value;
            note.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Note updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int noteId)
        {
            var note = await _db.InternalNotes.FindAsync(noteId);
            if (note == null) return (false, "Note not found.");
            _db.InternalNotes.Remove(note);
            await _db.SaveChangesAsync();
            return (true, "Note deleted.");
        }

        public async Task<List<InternalNoteDto>> GetForEntityAsync(string entityType, int entityId)
        {
            var notes = await _db.InternalNotes.Include(n => n.CreatedByAdmin)
                .Where(n => n.EntityType == entityType && n.EntityId == entityId)
                .OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.CreatedAt).ToListAsync();
            return notes.Select(n => ToDto(n, n.CreatedByAdmin?.FullName)).ToList();
        }

        private static InternalNoteDto ToDto(InternalNote n, string? createdByName) => new()
        {
            Id = n.Id, EntityType = n.EntityType, EntityId = n.EntityId, Note = n.Note, IsPinned = n.IsPinned,
            CreatedByName = createdByName, CreatedAt = n.CreatedAt, UpdatedAt = n.UpdatedAt,
        };
    }

    // ── M110: Customer Feedback Center ─────────────────────────────
    public class FeedbackCenterService
    {
        private readonly AppDbContext _db;
        public FeedbackCenterService(AppDbContext db) => _db = db;

        public async Task<FeedbackSurveyDto> CreateSurveyAsync(CreateFeedbackSurveyRequestDto req)
        {
            var survey = new FeedbackSurvey { Title = req.Title, SurveyType = req.SurveyType, TargetAudience = req.TargetAudience };
            _db.FeedbackSurveys.Add(survey);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(survey);
        }

        public async Task<List<FeedbackSurveyDto>> GetAllSurveysAsync()
        {
            var surveys = await _db.FeedbackSurveys.OrderByDescending(s => s.CreatedAt).ToListAsync();
            var result = new List<FeedbackSurveyDto>();
            foreach (var s in surveys) result.Add(await ToDtoAsync(s));
            return result;
        }

        public async Task<(bool Success, string Message)> SubmitAsync(SubmitFeedbackRequestDto req)
        {
            var survey = await _db.FeedbackSurveys.FindAsync(req.SurveyId);
            if (survey == null) return (false, "Survey not found.");
            if (!survey.IsActive) return (false, "This survey is no longer accepting responses.");
            var maxScore = survey.SurveyType == "NPS" ? 10 : 5;
            if (req.Score < 0 || req.Score > maxScore) return (false, $"Score must be between 0 and {maxScore} for a {survey.SurveyType} survey.");
            _db.FeedbackResponses.Add(new FeedbackResponse { FeedbackSurveyId = req.SurveyId, UserId = req.UserId, Score = req.Score, Comment = req.Comment });
            await _db.SaveChangesAsync();
            return (true, "Thanks for your feedback!");
        }

        public async Task<FeedbackSurveyStatsDto?> GetStatsAsync(int surveyId)
        {
            var survey = await _db.FeedbackSurveys.FindAsync(surveyId);
            if (survey == null) return null;
            var responses = await _db.FeedbackResponses.Where(r => r.FeedbackSurveyId == surveyId).ToListAsync();
            var stats = new FeedbackSurveyStatsDto
            {
                SurveyId = surveyId, SurveyType = survey.SurveyType, ResponseCount = responses.Count,
                AverageScore = responses.Count == 0 ? 0 : Math.Round(responses.Average(r => r.Score), 2),
            };
            if (survey.SurveyType == "NPS" && responses.Count > 0)
            {
                stats.Promoters = responses.Count(r => r.Score >= 9);
                stats.Detractors = responses.Count(r => r.Score <= 6);
                stats.Passives = responses.Count - stats.Promoters - stats.Detractors;
                stats.NpsScore = Math.Round((double)(stats.Promoters - stats.Detractors) / responses.Count * 100, 1);
            }
            return stats;
        }

        private async Task<FeedbackSurveyDto> ToDtoAsync(FeedbackSurvey s)
        {
            var responses = await _db.FeedbackResponses.Where(r => r.FeedbackSurveyId == s.Id).ToListAsync();
            return new FeedbackSurveyDto
            {
                Id = s.Id, Title = s.Title, SurveyType = s.SurveyType, TargetAudience = s.TargetAudience, IsActive = s.IsActive,
                ResponseCount = responses.Count, AverageScore = responses.Count == 0 ? 0 : Math.Round(responses.Average(r => r.Score), 2), CreatedAt = s.CreatedAt,
            };
        }
    }

    // ── M111: AI Suite — Smart Pricing Engine ──────────────────────
    public class SmartPricingService
    {
        private readonly AppDbContext _db;
        public SmartPricingService(AppDbContext db) => _db = db;

        // Heuristic: compares average predicted demand (last 7 days on file)
        // for a city against its active chef supply. High demand + low
        // supply suggests a higher multiplier; the inverse suggests a
        // discount to stimulate bookings. This is explicitly a rules-based
        // heuristic, not a trained model — documented as advisory-only.
        public async Task<List<PricingSuggestionDto>> GenerateAsync(GeneratePricingSuggestionsRequestDto req)
        {
            var since = DateTime.UtcNow.AddDays(-7);
            var demandQuery = _db.DemandPredictions.Where(d => d.CreatedAt >= since);
            if (!string.IsNullOrEmpty(req.City)) demandQuery = demandQuery.Where(d => d.City == req.City);
            var demandByCity = await demandQuery.GroupBy(d => d.City).Select(g => new { City = g.Key, Avg = g.Average(x => x.PredictedDemand) }).ToListAsync();

            var results = new List<PricingSuggestionDto>();
            foreach (var d in demandByCity)
            {
                var supply = await _db.ChefProfiles.CountAsync(c => c.User != null); // active chef supply proxy (all chef profiles on file)
                decimal multiplier = 1.0m;
                string reason;
                if (d.Avg >= 75 && supply < 20) { multiplier = 1.20m; reason = $"High demand ({d.Avg:F0}/100) with limited chef supply ({supply}) — suggest +20% surge."; }
                else if (d.Avg >= 60) { multiplier = 1.10m; reason = $"Elevated demand ({d.Avg:F0}/100) — suggest +10%."; }
                else if (d.Avg <= 25) { multiplier = 0.90m; reason = $"Low demand ({d.Avg:F0}/100) — suggest -10% to stimulate bookings."; }
                else { reason = $"Demand within normal range ({d.Avg:F0}/100) — no change suggested."; }

                var suggestion = new PricingSuggestion { City = d.City, SuggestedMultiplier = multiplier, DemandScore = d.Avg, ActiveChefSupply = supply, Reason = reason };
                _db.PricingSuggestions.Add(suggestion);
                results.Add(ToDto(suggestion));
            }
            await _db.SaveChangesAsync();
            return results;
        }

        public async Task<List<PricingSuggestionDto>> GetAllAsync(string? status = null)
        {
            var q = _db.PricingSuggestions.AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(p => p.Status == status);
            return (await q.OrderByDescending(p => p.GeneratedAt).ToListAsync()).Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message)> DecideAsync(DecidePricingSuggestionRequestDto req)
        {
            var suggestion = await _db.PricingSuggestions.FindAsync(req.SuggestionId);
            if (suggestion == null) return (false, "Suggestion not found.");
            suggestion.Status = req.Apply ? "Applied" : "Dismissed";
            await _db.SaveChangesAsync();
            return (true, req.Apply ? "Marked as applied (apply the multiplier in your pricing config)." : "Dismissed.");
        }

        private static PricingSuggestionDto ToDto(PricingSuggestion p) => new()
        {
            Id = p.Id, City = p.City, Category = p.Category, SuggestedMultiplier = p.SuggestedMultiplier, DemandScore = p.DemandScore,
            ActiveChefSupply = p.ActiveChefSupply, Reason = p.Reason, Status = p.Status, GeneratedAt = p.GeneratedAt,
        };
    }

    // ── M112: AI Suite — Fraud Risk Scoring ─────────────────────────
    public class AIFraudScoringService
    {
        private readonly AppDbContext _db;
        public AIFraudScoringService(AppDbContext db) => _db = db;

        public async Task<RiskSignalDto> UpsertSignalAsync(UpsertRiskSignalRequestDto req)
        {
            var signal = await _db.RiskSignalDefinitions.FirstOrDefaultAsync(s => s.SignalKey == req.SignalKey);
            if (signal == null) { signal = new RiskSignalDefinition { SignalKey = req.SignalKey }; _db.RiskSignalDefinitions.Add(signal); }
            signal.Description = req.Description;
            signal.WeightPoints = req.WeightPoints;
            await _db.SaveChangesAsync();
            return new RiskSignalDto { Id = signal.Id, SignalKey = signal.SignalKey, Description = signal.Description, WeightPoints = signal.WeightPoints, IsActive = signal.IsActive };
        }

        public async Task<List<RiskSignalDto>> GetSignalsAsync()
            => (await _db.RiskSignalDefinitions.ToListAsync()).Select(s => new RiskSignalDto { Id = s.Id, SignalKey = s.SignalKey, Description = s.Description, WeightPoints = s.WeightPoints, IsActive = s.IsActive }).ToList();

        // Heuristic behavioral scoring: checks a fixed set of built-in
        // signals against the user's recent activity, sums the configured
        // weight for each one that fires, then writes the result into the
        // existing M45 FraudDetectionLog so it surfaces in the same review
        // queue as other fraud flags.
        public async Task<RiskScoreResultDto> ComputeAsync(int userId)
        {
            var signals = await _db.RiskSignalDefinitions.Where(s => s.IsActive).ToDictionaryAsync(s => s.SignalKey);
            decimal score = 0;
            var triggered = new List<string>();

            var since30d = DateTime.UtcNow.AddDays(-30);
            var recentBookings = await _db.Bookings.Where(b => b.CustomerId == userId && b.CreatedAt >= since30d).ToListAsync();
            var recentRefunds = await _db.RefundRequests.Where(r => r.CustomerId == userId && r.RequestedAt >= since30d).ToListAsync();

            if (recentBookings.Count >= 10 && signals.TryGetValue("rapid_repeat_bookings", out var s1))
            { score += s1.WeightPoints; triggered.Add(s1.SignalKey); }

            if (recentBookings.Count > 0 && (decimal)recentRefunds.Count / recentBookings.Count >= 0.5m && signals.TryGetValue("high_refund_rate", out var s2))
            { score += s2.WeightPoints; triggered.Add(s2.SignalKey); }

            var cancelledCount = recentBookings.Count(b => b.Status == "Cancelled");
            if (recentBookings.Count > 0 && (decimal)cancelledCount / recentBookings.Count >= 0.5m && signals.TryGetValue("high_cancellation_rate", out var s3))
            { score += s3.WeightPoints; triggered.Add(s3.SignalKey); }

            score = Math.Clamp(score, 0, 100);
            var severity = score >= 75 ? "Critical" : score >= 50 ? "High" : score >= 25 ? "Medium" : "Low";

            _db.FraudDetectionLogs.Add(new FraudDetectionLog
            {
                UserId = userId, DetectionType = "AIRiskScore", Severity = severity, RiskScore = score,
                Evidence = System.Text.Json.JsonSerializer.Serialize(new { triggered, recentBookingCount = recentBookings.Count, recentRefundCount = recentRefunds.Count }),
                Action = score >= 75 ? "Flagged" : "Cleared",
            });
            await _db.SaveChangesAsync();

            return new RiskScoreResultDto { UserId = userId, RiskScore = score, Severity = severity, TriggeredSignals = triggered };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/internal-notes"), Authorize(Roles = "Admin")]
    public class InternalNoteController : ControllerBase
    {
        private readonly Services.InternalNoteService _svc;
        public InternalNoteController(Services.InternalNoteService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInternalNoteRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(UserId, req) });

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateInternalNoteRequestDto req)
        {
            var (success, message) = await _svc.UpdateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _svc.DeleteAsync(id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> GetForEntity([FromQuery] string entityType, [FromQuery] int entityId) => Ok(new { success = true, data = await _svc.GetForEntityAsync(entityType, entityId) });
    }

    [ApiController, Route("api/feedback-center")]
    public class FeedbackCenterController : ControllerBase
    {
        private readonly Services.FeedbackCenterService _svc;
        public FeedbackCenterController(Services.FeedbackCenterService svc) => _svc = svc;

        [HttpPost("surveys"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateFeedbackSurveyRequestDto req) => Ok(new { success = true, data = await _svc.CreateSurveyAsync(req) });

        [HttpGet("surveys"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSurveys() => Ok(new { success = true, data = await _svc.GetAllSurveysAsync() });

        [HttpPost("submit"), Authorize]
        public async Task<IActionResult> Submit([FromBody] SubmitFeedbackRequestDto req)
        {
            var (success, message) = await _svc.SubmitAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("surveys/{id}/stats"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStats(int id)
        {
            var stats = await _svc.GetStatsAsync(id);
            if (stats == null) return NotFound(new { success = false, message = "Survey not found." });
            return Ok(new { success = true, data = stats });
        }
    }

    [ApiController, Route("api/smart-pricing"), Authorize(Roles = "Admin")]
    public class SmartPricingController : ControllerBase
    {
        private readonly Services.SmartPricingService _svc;
        public SmartPricingController(Services.SmartPricingService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GeneratePricingSuggestionsRequestDto req) => Ok(new { success = true, data = await _svc.GenerateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status) => Ok(new { success = true, data = await _svc.GetAllAsync(status) });

        [HttpPost("decide")]
        public async Task<IActionResult> Decide([FromBody] DecidePricingSuggestionRequestDto req)
        {
            var (success, message) = await _svc.DecideAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/ai-fraud-scoring"), Authorize(Roles = "Admin")]
    public class AIFraudScoringController : ControllerBase
    {
        private readonly Services.AIFraudScoringService _svc;
        public AIFraudScoringController(Services.AIFraudScoringService svc) => _svc = svc;

        [HttpPost("signals")]
        public async Task<IActionResult> UpsertSignal([FromBody] UpsertRiskSignalRequestDto req) => Ok(new { success = true, data = await _svc.UpsertSignalAsync(req) });

        [HttpGet("signals")]
        public async Task<IActionResult> GetSignals() => Ok(new { success = true, data = await _svc.GetSignalsAsync() });

        [HttpPost("compute")]
        public async Task<IActionResult> Compute([FromBody] ComputeRiskScoreRequestDto req) => Ok(new { success = true, data = await _svc.ComputeAsync(req.UserId) });
    }
}
