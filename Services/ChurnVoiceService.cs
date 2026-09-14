using System.Text.Json;
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
    // M71: Churn Prediction Service
    // ══════════════════════════════════════════════════════════════
    public class ChurnPredictionService
    {
        private readonly AppDbContext _db;
        private readonly NotificationService _notifications;

        public ChurnPredictionService(AppDbContext db, NotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public async Task<List<ChurnRiskDto>> GetAtRiskCustomersAsync(string? riskLevel = null, int take = 50)
        {
            var query = _db.ChurnRiskScores.AsQueryable();
            if (!string.IsNullOrWhiteSpace(riskLevel)) query = query.Where(s => s.RiskLevel == riskLevel);

            var scores = await query.OrderByDescending(s => s.RiskScore).Take(take).ToListAsync();
            return await MapAllAsync(scores);
        }

        public async Task<ChurnRiskDto?> GetCustomerRiskAsync(int customerId)
        {
            var score = await _db.ChurnRiskScores.FirstOrDefaultAsync(s => s.CustomerId == customerId);
            if (score == null)
            {
                score = await ComputeForCustomerAsync(customerId);
                if (score == null) return null;
            }
            return (await MapAllAsync(new List<ChurnRiskScore> { score })).FirstOrDefault();
        }

        /// <summary>Recomputes risk for every customer who has at least one booking. Intended to run on a schedule (G7/Hangfire) once that's wired — exposed as a manual admin trigger for now, same pattern as HeatMapService.RecomputeAsync.</summary>
        public async Task<int> RecomputeAllAsync()
        {
            var customerIds = await _db.Bookings.Select(b => b.CustomerId).Distinct().ToListAsync();
            int count = 0;
            foreach (var customerId in customerIds)
            {
                if (await ComputeForCustomerAsync(customerId) != null) count++;
            }
            return count;
        }

        private async Task<ChurnRiskScore?> ComputeForCustomerAsync(int customerId)
        {
            var bookings = await _db.Bookings.Where(b => b.CustomerId == customerId).ToListAsync();
            if (bookings.Count == 0) return null;

            var now = DateTime.UtcNow;
            var last90 = now.AddDays(-90);

            var lastBooking = bookings.OrderByDescending(b => b.CreatedAt).First();
            var daysSinceLastBooking = (int)(now - lastBooking.CreatedAt).TotalDays;
            var bookingsLast90 = bookings.Count(b => b.CreatedAt >= last90);
            var cancelledLast90 = bookings.Count(b => b.Status == "Cancelled" && b.CreatedAt >= last90);

            int score = daysSinceLastBooking switch
            {
                < 14 => 5,
                < 30 => 20,
                < 60 => 40,
                < 90 => 70,
                _ => 90,
            };

            // High recent cancellation rate suggests dissatisfaction, not just inactivity.
            if (bookingsLast90 > 0 && cancelledLast90 >= (double)bookingsLast90 / 2)
                score += 10;

            // Long-time customers with a solid history are stickier — pull the score down a bit.
            if (bookings.Count > 10) score -= 10;

            score = Math.Clamp(score, 0, 100);
            var level = score >= 70 ? "High" : score >= 40 ? "Medium" : "Low";

            var existing = await _db.ChurnRiskScores.FirstOrDefaultAsync(s => s.CustomerId == customerId);
            if (existing == null)
            {
                existing = new ChurnRiskScore { CustomerId = customerId };
                _db.ChurnRiskScores.Add(existing);
            }

            existing.RiskScore = score;
            existing.RiskLevel = level;
            existing.DaysSinceLastBooking = daysSinceLastBooking;
            existing.TotalBookings = bookings.Count;
            existing.BookingsLast90Days = bookingsLast90;
            existing.CancelledLast90Days = cancelledLast90;
            existing.FactorsJson = JsonSerializer.Serialize(new { daysSinceLastBooking, bookingsLast90, cancelledLast90, totalBookings = bookings.Count });
            existing.ComputedAt = now;

            await _db.SaveChangesAsync();
            return existing;
        }

        /// <summary>Sends a personal, single-use retention coupon + notification to a specific at-risk customer. Uses batch 19's real Coupon table rather than inventing a new discount mechanism.</summary>
        public async Task<ChurnInterventionResultDto> InterventionAsync(int customerId)
        {
            var customer = await _db.Users.FindAsync(customerId);
            if (customer == null) return new ChurnInterventionResultDto { Success = false, Message = "Customer not found." };

            var code = $"COMEBACK{customerId}{Random.Shared.Next(1000, 9999)}";
            _db.Coupons.Add(new Coupon
            {
                Code = code,
                Description = "We miss you! Welcome-back discount.",
                DiscountType = "Percent",
                DiscountValue = 20,
                MaxDiscount = 150,
                UsageLimit = 1,
                UsageLimitPerUser = 1,
                AppliesTo = "All",
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(14),
                IsActive = true,
            });

            var score = await _db.ChurnRiskScores.FirstOrDefaultAsync(s => s.CustomerId == customerId);
            if (score != null)
            {
                score.InterventionSent = true;
                score.InterventionSentAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            await _notifications.CreateAsync(customerId, "We miss you! 🍲",
                $"It's been a while — here's 20% off your next booking. Use code {code} before it expires in 14 days.", "Promo");

            return new ChurnInterventionResultDto { Success = true, Message = "Retention coupon sent.", CouponCode = code };
        }

        private async Task<List<ChurnRiskDto>> MapAllAsync(List<ChurnRiskScore> scores)
        {
            var result = new List<ChurnRiskDto>();
            foreach (var s in scores)
            {
                var customer = await _db.Users.FindAsync(s.CustomerId);
                result.Add(new ChurnRiskDto
                {
                    CustomerId = s.CustomerId,
                    CustomerName = customer?.FullName ?? "",
                    CustomerPhone = customer?.PhoneNumber,
                    RiskScore = s.RiskScore,
                    RiskLevel = s.RiskLevel,
                    DaysSinceLastBooking = s.DaysSinceLastBooking,
                    TotalBookings = s.TotalBookings,
                    BookingsLast90Days = s.BookingsLast90Days,
                    InterventionSent = s.InterventionSent,
                    ComputedAt = s.ComputedAt,
                });
            }
            return result;
        }
    }

    // ══════════════════════════════════════════════════════════════
    // M72: Voice Ordering Service
    // ══════════════════════════════════════════════════════════════
    public class VoiceOrderingService
    {
        private readonly AppDbContext _db;
        private readonly BookingService _booking;
        private readonly EmergencyService _emergency;

        public VoiceOrderingService(AppDbContext db, BookingService booking, EmergencyService emergency)
        {
            _db = db;
            _booking = booking;
            _emergency = emergency;
        }

        public async Task<VoiceCommandResponseDto> ProcessCommandAsync(int userId, VoiceCommandRequestDto req)
        {
            var text = (req.Transcript ?? "").Trim().ToLowerInvariant();
            VoiceCommandResponseDto response;

            if (text.Contains("cancel"))
                response = await HandleCancelAsync(userId);
            else if (text.Contains("status") || text.Contains("where is my") || text.Contains("my booking"))
                response = await HandleStatusAsync(userId);
            else if (text.Contains("book"))
                response = await HandleBookAsync(userId, req);
            else
                response = new VoiceCommandResponseDto
                {
                    DetectedIntent = "Unknown",
                    ResponseText = "Sorry, I didn't understand that. You can say things like 'book a chef', 'what's my booking status', or 'cancel my booking'.",
                    ActionTaken = false,
                };

            _db.VoiceCommandLogs.Add(new VoiceCommandLog
            {
                UserId = userId,
                RawTranscript = req.Transcript ?? "",
                DetectedIntent = response.DetectedIntent,
                ResponseText = response.ResponseText,
                ActionTaken = response.ActionTaken,
                RelatedBookingId = response.RelatedBookingId,
            });
            await _db.SaveChangesAsync();

            return response;
        }

        private async Task<VoiceCommandResponseDto> HandleStatusAsync(int userId)
        {
            var latest = await _db.Bookings.Where(b => b.CustomerId == userId).OrderByDescending(b => b.CreatedAt).FirstOrDefaultAsync();
            if (latest == null)
                return new VoiceCommandResponseDto { DetectedIntent = "CheckStatus", ResponseText = "You don't have any bookings yet.", ActionTaken = false };

            var chef = await _db.Users.FindAsync(latest.ChefId);
            return new VoiceCommandResponseDto
            {
                DetectedIntent = "CheckStatus",
                ResponseText = $"Your booking with {chef?.FullName ?? "your chef"} is currently {latest.Status}.",
                ActionTaken = false,
                RelatedBookingId = latest.Id,
            };
        }

        private async Task<VoiceCommandResponseDto> HandleCancelAsync(int userId)
        {
            var active = await _db.Bookings
                .Where(b => b.CustomerId == userId && (b.Status == "Pending" || b.Status == "Accepted"))
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (active == null)
                return new VoiceCommandResponseDto { DetectedIntent = "CancelBooking", ResponseText = "You don't have an active booking to cancel.", ActionTaken = false };

            var (success, message) = await _booking.CancelBookingAsync(userId, active.Id, "Cancelled via voice command.");
            return new VoiceCommandResponseDto
            {
                DetectedIntent = "CancelBooking",
                ResponseText = success ? "Your booking has been cancelled." : $"Couldn't cancel your booking: {message}",
                ActionTaken = success,
                RelatedBookingId = active.Id,
            };
        }

        private async Task<VoiceCommandResponseDto> HandleBookAsync(int userId, VoiceCommandRequestDto req)
        {
            if (!req.Latitude.HasValue || !req.Longitude.HasValue)
                return new VoiceCommandResponseDto { DetectedIntent = "BookChef", ResponseText = "I need your location to find a nearby chef — please enable location and try again.", ActionTaken = false };

            // Reuses Phase 6's EmergencyService — "book me a chef right now" is exactly
            // the one-tap flow that already exists there, whether triggered by a button or a voice command.
            var (success, message, data) = await _emergency.BookNearestAvailableAsync(userId, new BookEmergencyChefRequestDto
            {
                Latitude = req.Latitude.Value,
                Longitude = req.Longitude.Value,
                Address = req.Address ?? "",
            });

            return new VoiceCommandResponseDto
            {
                DetectedIntent = "BookChef",
                ResponseText = success ? $"Done — {message}" : $"I couldn't book a chef: {message}",
                ActionTaken = success,
                RelatedBookingId = data?.Id,
            };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/churn")]
    [Authorize(Roles = "Admin")]
    public class ChurnController : ControllerBase
    {
        private readonly Services.ChurnPredictionService _svc;
        public ChurnController(Services.ChurnPredictionService svc) => _svc = svc;

        [HttpGet("at-risk")]
        public async Task<IActionResult> AtRisk([FromQuery] string? level, [FromQuery] int take = 50)
        {
            var data = await _svc.GetAtRiskCustomersAsync(level, take);
            return Ok(new { success = true, count = data.Count, data });
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> Get(int customerId)
        {
            var data = await _svc.GetCustomerRiskAsync(customerId);
            if (data == null) return NotFound(new { success = false, message = "No booking history for this customer." });
            return Ok(new { success = true, data });
        }

        [HttpPost("recompute")]
        public async Task<IActionResult> Recompute()
        {
            var count = await _svc.RecomputeAllAsync();
            return Ok(new { success = true, message = $"Recomputed churn risk for {count} customer(s)." });
        }

        [HttpPost("{customerId}/intervene")]
        public async Task<IActionResult> Intervene(int customerId)
        {
            var data = await _svc.InterventionAsync(customerId);
            if (!data.Success) return BadRequest(data);
            return Ok(new { success = true, data });
        }
    }

    [ApiController]
    [Route("api/voice")]
    [Authorize]
    public class VoiceController : ControllerBase
    {
        private readonly Services.VoiceOrderingService _svc;
        public VoiceController(Services.VoiceOrderingService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("command")]
        public async Task<IActionResult> Command([FromBody] VoiceCommandRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Transcript))
                return BadRequest(new { success = false, message = "Transcript is required." });

            var data = await _svc.ProcessCommandAsync(UserId, req);
            return Ok(new { success = true, data });
        }
    }
}
