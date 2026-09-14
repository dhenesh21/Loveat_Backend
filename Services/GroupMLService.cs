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
    // M69: Group Booking Service
    // ══════════════════════════════════════════════════════════════
    public class GroupBookingService
    {
        private readonly AppDbContext _db;
        private readonly SearchService _search;
        private readonly BookingService _booking;

        public GroupBookingService(AppDbContext db, SearchService search, BookingService booking)
        {
            _db = db;
            _search = search;
            _booking = booking;
        }

        public async Task<(bool Success, string Message, GroupBookingDto? Data)> CreateGroupBookingAsync(int customerId, CreateGroupBookingRequestDto req)
        {
            if (req.TotalGuestCount <= 0) return (false, "Total guest count must be positive.", null);
            if (req.MaxGuestsPerChef <= 0) return (false, "Max guests per chef must be positive.", null);

            var neededChefs = (int)Math.Ceiling((double)req.TotalGuestCount / req.MaxGuestsPerChef);

            var candidates = await _search.SearchChefsAsync(new SearchChefsRequestDto
            {
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                RadiusKm = 20,
                Cuisine = req.Cuisine,
                AvailableNow = true,
            });

            if (candidates.Count < neededChefs)
                return (false, $"Only {candidates.Count} available chef(s) found nearby, but this group needs {neededChefs} to cover {req.TotalGuestCount} guests. Try a smaller group or a wider search area.", null);

            var group = new GroupBooking
            {
                CustomerId = customerId,
                TotalGuestCount = req.TotalGuestCount,
                Cuisine = req.Cuisine,
                ScheduledAt = req.ScheduledAt,
                DurationMinutes = req.DurationMinutes,
                Address = req.Address,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
            };
            _db.GroupBookings.Add(group);
            await _db.SaveChangesAsync();

            var createdBookingIds = new List<int>();
            var splits = new List<GroupBookingSplit>();
            decimal totalAmount = 0;
            int remaining = req.TotalGuestCount;

            for (int i = 0; i < neededChefs; i++)
            {
                var chef = candidates[i];
                var guestsForThisChef = Math.Min(req.MaxGuestsPerChef, remaining);
                remaining -= guestsForThisChef;

                var (success, message, bookingData) = await _booking.CreateBookingAsync(customerId, new CreateBookingRequestDto
                {
                    ChefId = chef.UserId,
                    BookingType = "Group",
                    Cuisine = req.Cuisine,
                    ScheduledAt = req.ScheduledAt,
                    DurationMinutes = req.DurationMinutes,
                    GuestCount = guestsForThisChef,
                    Address = req.Address,
                    Latitude = req.Latitude,
                    Longitude = req.Longitude,
                    Notes = $"Part of group booking #{group.Id} ({guestsForThisChef} of {req.TotalGuestCount} guests).",
                });

                if (!success || bookingData == null)
                {
                    // Roll back: cancel every sub-booking already created for this group rather than
                    // leaving a half-formed group booking the customer never agreed to.
                    foreach (var bid in createdBookingIds)
                        await _booking.CancelBookingAsync(customerId, bid, "Group booking creation failed — rolling back.");

                    _db.GroupBookings.Remove(group);
                    await _db.SaveChangesAsync();

                    return (false, $"Could not book {chef.FullName}: {message}. The group booking was rolled back — no partial bookings were kept.", null);
                }

                createdBookingIds.Add(bookingData.Id);
                totalAmount += bookingData.TotalAmount;

                splits.Add(new GroupBookingSplit
                {
                    GroupBookingId = group.Id,
                    BookingId = bookingData.Id,
                    ChefId = chef.UserId,
                    GuestCount = guestsForThisChef,
                });
            }

            _db.GroupBookingSplits.AddRange(splits);
            group.TotalAmount = totalAmount;
            await _db.SaveChangesAsync();

            return (true, $"Group booking created across {neededChefs} chef(s).", await ToDtoAsync(group.Id));
        }

        public async Task<GroupBookingDto?> GetAsync(int customerId, int groupId)
        {
            var group = await _db.GroupBookings.FirstOrDefaultAsync(g => g.Id == groupId && g.CustomerId == customerId);
            return group == null ? null : await ToDtoAsync(group.Id);
        }

        public async Task<List<GroupBookingDto>> GetMineAsync(int customerId)
        {
            var groups = await _db.GroupBookings.Where(g => g.CustomerId == customerId).OrderByDescending(g => g.CreatedAt).ToListAsync();
            var result = new List<GroupBookingDto>();
            foreach (var g in groups)
                result.Add(await ToDtoAsync(g.Id) ?? new GroupBookingDto { Id = g.Id });
            return result;
        }

        public async Task<(bool Success, string Message)> CancelAsync(int customerId, int groupId)
        {
            var group = await _db.GroupBookings.FirstOrDefaultAsync(g => g.Id == groupId && g.CustomerId == customerId);
            if (group == null) return (false, "Group booking not found.");
            if (group.Status == "Cancelled") return (false, "This group booking is already cancelled.");

            var splits = await _db.GroupBookingSplits.Where(s => s.GroupBookingId == groupId).ToListAsync();
            foreach (var split in splits)
                await _booking.CancelBookingAsync(customerId, split.BookingId, "Group booking cancelled by customer.");

            group.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return (true, "Group booking cancelled — all chef bookings in the group were cancelled too.");
        }

        private async Task<GroupBookingDto?> ToDtoAsync(int groupId)
        {
            var group = await _db.GroupBookings.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return null;

            var splits = await _db.GroupBookingSplits.Where(s => s.GroupBookingId == groupId).ToListAsync();
            var splitDtos = new List<GroupBookingSplitDto>();

            foreach (var split in splits)
            {
                var chef = await _db.Users.FindAsync(split.ChefId);
                var booking = await _db.Bookings.FindAsync(split.BookingId);
                splitDtos.Add(new GroupBookingSplitDto
                {
                    BookingId = split.BookingId,
                    ChefId = split.ChefId,
                    ChefName = chef?.FullName ?? "",
                    GuestCount = split.GuestCount,
                    Amount = booking?.TotalAmount ?? 0,
                    Status = booking?.Status ?? "Unknown",
                });
            }

            return new GroupBookingDto
            {
                Id = group.Id,
                TotalGuestCount = group.TotalGuestCount,
                Cuisine = group.Cuisine,
                ScheduledAt = group.ScheduledAt,
                Address = group.Address,
                Status = group.Status,
                TotalAmount = group.TotalAmount,
                Splits = splitDtos,
                CreatedAt = group.CreatedAt,
            };
        }
    }

    // ══════════════════════════════════════════════════════════════
    // M70: ML (heuristic) Chef-Customer Matching Service
    // ══════════════════════════════════════════════════════════════
    public class MLMatchingService
    {
        private readonly AppDbContext _db;
        private const int RecomputeWindowHours = 24;

        public MLMatchingService(AppDbContext db) => _db = db;

        public async Task<List<ChefMatchDto>> GetTopMatchesAsync(int customerId, int take = 10)
        {
            var staleBefore = DateTime.UtcNow.AddHours(-RecomputeWindowHours);
            var cached = await _db.ChefMatchScores
                .Where(s => s.CustomerId == customerId && s.ComputedAt >= staleBefore)
                .OrderByDescending(s => s.Score)
                .Take(take)
                .ToListAsync();

            if (cached.Count >= Math.Min(take, 1))
                return await MapScoresAsync(cached);

            return await RecomputeAsync(customerId, take);
        }

        private async Task<List<ChefMatchDto>> RecomputeAsync(int customerId, int take)
        {
            var pastBookings = await _db.Bookings
                .Where(b => b.CustomerId == customerId && b.Status == "Completed")
                .ToListAsync();

            var repeatCounts = pastBookings.GroupBy(b => b.ChefId).ToDictionary(g => g.Key, g => g.Count());
            var topCuisines = pastBookings.Where(b => b.Cuisine != null)
                .GroupBy(b => b.Cuisine!)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => g.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var reviewsGiven = await _db.Reviews.Where(r => r.CustomerId == customerId).ToListAsync();
            var ratingGivenMap = reviewsGiven.GroupBy(r => r.ChefId).ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

            var candidates = await _db.ChefProfiles.Include(p => p.User)
                .Where(p => p.User != null && p.User.IsActive)
                .ToListAsync();

            var scored = new List<(ChefProfile Profile, decimal Score, List<string> Reasons, object Factors)>();

            foreach (var profile in candidates)
            {
                decimal score = 0;
                var reasons = new List<string>();

                var repeatCount = repeatCounts.GetValueOrDefault(profile.UserId, 0);
                if (repeatCount > 0)
                {
                    score += Math.Min(repeatCount * 8, 24);
                    reasons.Add($"You've booked this chef {repeatCount} time(s) before");
                }

                if (ratingGivenMap.TryGetValue(profile.UserId, out var ratingGiven))
                {
                    score += (decimal)ratingGiven * 6; // up to 30
                    reasons.Add($"You rated this chef {ratingGiven:0.0}★ previously");
                }

                var cuisines = SafeDeserialize(profile.Cuisines);
                if (topCuisines.Count > 0 && cuisines.Any(c => topCuisines.Contains(c)))
                {
                    score += 20;
                    reasons.Add("Matches cuisines you book most often");
                }

                score += profile.AverageRating * 6; // up to 30
                if (reasons.Count == 0) reasons.Add("Highly rated on LovEat");

                score = Math.Min(score, 100);

                scored.Add((profile, score, reasons, new { repeatCount, ratingGiven = ratingGivenMap.GetValueOrDefault(profile.UserId), cuisineMatch = cuisines.Any(c => topCuisines.Contains(c)), chefRating = profile.AverageRating }));
            }

            var top = scored.OrderByDescending(s => s.Score).Take(take).ToList();

            // Replace this customer's cached scores with the freshly computed ones.
            var oldScores = await _db.ChefMatchScores.Where(s => s.CustomerId == customerId).ToListAsync();
            _db.ChefMatchScores.RemoveRange(oldScores);

            foreach (var (profile, score, _, factors) in top)
            {
                _db.ChefMatchScores.Add(new ChefMatchScore
                {
                    CustomerId = customerId,
                    ChefId = profile.UserId,
                    Score = score,
                    FactorsJson = JsonSerializer.Serialize(factors),
                });
            }

            _db.AIAnalyticsEntries.Add(new AIAnalytics
            {
                UserId = customerId,
                AnalyticsType = "MLChefMatch",
                InputDataJson = JsonSerializer.Serialize(new { pastBookingCount = pastBookings.Count, topCuisines }),
                OutputDataJson = JsonSerializer.Serialize(top.Select(t => new { chefId = t.Profile.UserId, score = t.Score })),
                ModelVersion = "heuristic-v1", // honest label — see class-level doc comment on ChefMatchScore
            });

            await _db.SaveChangesAsync();

            return top.Select(t => new ChefMatchDto
            {
                ChefProfileId = t.Profile.Id,
                UserId = t.Profile.UserId,
                FullName = t.Profile.User!.FullName,
                ProfileImageUrl = t.Profile.User.ProfileImageUrl,
                Cuisines = SafeDeserialize(t.Profile.Cuisines),
                HourlyRate = t.Profile.HourlyRate,
                AverageRating = t.Profile.AverageRating,
                MatchScore = Math.Round(t.Score, 1),
                Reasons = t.Reasons,
            }).ToList();
        }

        private async Task<List<ChefMatchDto>> MapScoresAsync(List<ChefMatchScore> scores)
        {
            var result = new List<ChefMatchDto>();
            foreach (var s in scores)
            {
                var profile = await _db.ChefProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == s.ChefId);
                if (profile?.User == null) continue;

                result.Add(new ChefMatchDto
                {
                    ChefProfileId = profile.Id,
                    UserId = profile.UserId,
                    FullName = profile.User.FullName,
                    ProfileImageUrl = profile.User.ProfileImageUrl,
                    Cuisines = SafeDeserialize(profile.Cuisines),
                    HourlyRate = profile.HourlyRate,
                    AverageRating = profile.AverageRating,
                    MatchScore = Math.Round(s.Score, 1),
                    Reasons = new List<string> { "Based on your booking history" },
                });
            }
            return result;
        }

        private static List<string> SafeDeserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
            catch { return new List<string>(); }
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/group-bookings")]
    [Authorize(Roles = "Customer")]
    public class GroupBookingController : ControllerBase
    {
        private readonly Services.GroupBookingService _svc;
        public GroupBookingController(Services.GroupBookingService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupBookingRequestDto req)
        {
            var (success, message, data) = await _svc.CreateGroupBookingAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _svc.GetAsync(UserId, id);
            if (data == null) return NotFound(new { success = false, message = "Group booking not found." });
            return Ok(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> Mine()
        {
            var data = await _svc.GetMineAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var (success, message) = await _svc.CancelAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController]
    [Route("api/ml-matching")]
    [Authorize(Roles = "Customer")]
    public class MLMatchingController : ControllerBase
    {
        private readonly Services.MLMatchingService _svc;
        public MLMatchingController(Services.MLMatchingService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("chefs")]
        public async Task<IActionResult> GetMatches([FromQuery] int take = 10)
        {
            var data = await _svc.GetTopMatchesAsync(UserId, take);
            return Ok(new { success = true, data });
        }
    }
}
