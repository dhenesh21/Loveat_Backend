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
    // M79: White-label Licensing Service
    // ══════════════════════════════════════════════════════════════
    public class WhiteLabelService
    {
        private readonly AppDbContext _db;
        public WhiteLabelService(AppDbContext db) => _db = db;

        public async Task<WhiteLabelClientDto> CreateClientAsync(CreateWhiteLabelClientRequestDto req)
        {
            var client = new WhiteLabelClient
            {
                BrandName = req.BrandName,
                ContactEmail = req.ContactEmail,
                Domain = req.Domain,
                PrimaryColor = req.PrimaryColor,
                LogoUrl = req.LogoUrl,
                PlanTier = req.PlanTier,
            };
            _db.WhiteLabelClients.Add(client);
            await _db.SaveChangesAsync();
            return ToDto(client, 0);
        }

        public async Task<List<WhiteLabelClientDto>> GetClientsAsync()
        {
            var clients = await _db.WhiteLabelClients.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var result = new List<WhiteLabelClientDto>();
            foreach (var c in clients)
            {
                var callCount = await _db.WhiteLabelUsageLogs.CountAsync(l => l.ClientId == c.Id);
                result.Add(ToDto(c, callCount));
            }
            return result;
        }

        /// <summary>Looks up an active client by API key — the building block a partner-facing API gateway/middleware would call before letting a request through. No such middleware exists yet; this is the piece it would call.</summary>
        public async Task<WhiteLabelClient?> ValidateApiKeyAsync(string apiKey)
            => await _db.WhiteLabelClients.FirstOrDefaultAsync(c => c.ApiKey == apiKey && c.IsActive);

        public async Task LogUsageAsync(int clientId, string endpoint, int responseStatus)
        {
            _db.WhiteLabelUsageLogs.Add(new WhiteLabelUsageLog { ClientId = clientId, Endpoint = endpoint, ResponseStatus = responseStatus });
            await _db.SaveChangesAsync();
        }

        public async Task<(bool Success, string Message)> RevokeAsync(int clientId)
        {
            var client = await _db.WhiteLabelClients.FindAsync(clientId);
            if (client == null) return (false, "Client not found.");
            client.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "API key revoked.");
        }

        private static WhiteLabelClientDto ToDto(WhiteLabelClient c, int callCount) => new()
        {
            Id = c.Id, BrandName = c.BrandName, ContactEmail = c.ContactEmail, Domain = c.Domain,
            ApiKey = c.ApiKey, PlanTier = c.PlanTier, IsActive = c.IsActive, CreatedAt = c.CreatedAt,
            TotalApiCalls = callCount,
        };
    }

    // ══════════════════════════════════════════════════════════════
    // M80: Carbon Footprint Tracker Service
    // ══════════════════════════════════════════════════════════════
    public class CarbonTrackerService
    {
        private readonly AppDbContext _db;
        private const decimal DefaultKgPerGuest = 0.6m; // typical portion weight assumption when not supplied
        private const decimal AverageCarKgPerKm = 0.12m; // representative petrol car emission factor, for the comparison note

        public CarbonTrackerService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CarbonEstimateDto? Data)> EstimateForBookingAsync(EstimateBookingCarbonRequestDto req)
        {
            var booking = await _db.Bookings.FindAsync(req.BookingId);
            if (booking == null) return (false, "Booking not found.", null);
            if (req.Ingredients.Count == 0) return (false, "At least one ingredient is required to estimate.", null);

            var factors = await _db.IngredientCarbonFactors.ToListAsync();
            var kgPerGuest = req.EstimatedKgPerGuest ?? DefaultKgPerGuest;
            var breakdown = new Dictionary<string, decimal>();
            decimal total = 0;

            foreach (var ingredient in req.Ingredients)
            {
                var factor = factors.FirstOrDefault(f => f.IngredientName.Equals(ingredient, StringComparison.OrdinalIgnoreCase))
                          ?? factors.FirstOrDefault(f => ingredient.Contains(f.IngredientName, StringComparison.OrdinalIgnoreCase));

                var carbonPerKg = factor?.CarbonKgPerKg ?? 2.5m; // representative "unknown ingredient" fallback (roughly a mixed-vegetable average)
                var contribution = Math.Round(carbonPerKg * kgPerGuest * booking.GuestCount / req.Ingredients.Count, 2);
                breakdown[ingredient] = contribution;
                total += contribution;
            }

            var estimate = new BookingCarbonEstimate
            {
                BookingId = booking.Id,
                EstimatedCarbonKg = total,
                BreakdownJson = System.Text.Json.JsonSerializer.Serialize(breakdown),
            };
            _db.BookingCarbonEstimates.Add(estimate);
            await _db.SaveChangesAsync();

            var kmEquivalent = Math.Round(total / AverageCarKgPerKm, 1);

            return (true, "Estimated.", new CarbonEstimateDto
            {
                BookingId = booking.Id,
                EstimatedCarbonKg = total,
                ComparisonNote = $"About the same as driving {kmEquivalent}km in an average petrol car.",
                BreakdownByIngredient = breakdown,
            });
        }

        public async Task<CustomerCarbonSummaryDto> GetCustomerSummaryAsync(int customerId)
        {
            var estimates = await _db.BookingCarbonEstimates
                .Where(e => _db.Bookings.Any(b => b.Id == e.BookingId && b.CustomerId == customerId))
                .ToListAsync();

            var total = estimates.Sum(e => e.EstimatedCarbonKg);
            return new CustomerCarbonSummaryDto
            {
                TotalBookingsTracked = estimates.Count,
                TotalCarbonKg = total,
                AverageCarbonPerBookingKg = estimates.Count == 0 ? 0 : Math.Round(total / estimates.Count, 2),
            };
        }

        /// <summary>Representative emission factors (broadly consistent with published food-carbon-footprint averages) — see class-level doc comment on IngredientCarbonFactor for the honesty caveat.</summary>
        public async Task SeedFactorsAsync()
        {
            if (await _db.IngredientCarbonFactors.AnyAsync()) return;
            _db.IngredientCarbonFactors.AddRange(
                new IngredientCarbonFactor { IngredientName = "Mutton", CarbonKgPerKg = 27m, Category = "Meat" },
                new IngredientCarbonFactor { IngredientName = "Beef", CarbonKgPerKg = 27m, Category = "Meat" },
                new IngredientCarbonFactor { IngredientName = "Chicken", CarbonKgPerKg = 6.9m, Category = "Meat" },
                new IngredientCarbonFactor { IngredientName = "Fish", CarbonKgPerKg = 5m, Category = "Meat" },
                new IngredientCarbonFactor { IngredientName = "Egg", CarbonKgPerKg = 4.5m, Category = "Dairy" },
                new IngredientCarbonFactor { IngredientName = "Paneer", CarbonKgPerKg = 3.2m, Category = "Dairy" },
                new IngredientCarbonFactor { IngredientName = "Milk", CarbonKgPerKg = 3.2m, Category = "Dairy" },
                new IngredientCarbonFactor { IngredientName = "Rice", CarbonKgPerKg = 4m, Category = "Grain" },
                new IngredientCarbonFactor { IngredientName = "Wheat", CarbonKgPerKg = 1.4m, Category = "Grain" },
                new IngredientCarbonFactor { IngredientName = "Lentils", CarbonKgPerKg = 0.9m, Category = "Grain" },
                new IngredientCarbonFactor { IngredientName = "Dal", CarbonKgPerKg = 0.9m, Category = "Grain" },
                new IngredientCarbonFactor { IngredientName = "Vegetables", CarbonKgPerKg = 2m, Category = "Vegetable" },
                new IngredientCarbonFactor { IngredientName = "Potato", CarbonKgPerKg = 0.5m, Category = "Vegetable" },
                new IngredientCarbonFactor { IngredientName = "Oil", CarbonKgPerKg = 3m, Category = "Other" }
            );
            await _db.SaveChangesAsync();
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/white-label")]
    [Authorize(Roles = "Admin")]
    public class WhiteLabelController : ControllerBase
    {
        private readonly Services.WhiteLabelService _svc;
        public WhiteLabelController(Services.WhiteLabelService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWhiteLabelClientRequestDto req)
        {
            var data = await _svc.CreateClientAsync(req);
            return Ok(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _svc.GetClientsAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/revoke")]
        public async Task<IActionResult> Revoke(int id)
        {
            var (success, message) = await _svc.RevokeAsync(id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController]
    [Route("api/carbon")]
    [Authorize]
    public class CarbonController : ControllerBase
    {
        private readonly Services.CarbonTrackerService _svc;
        public CarbonController(Services.CarbonTrackerService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("estimate")]
        public async Task<IActionResult> Estimate([FromBody] EstimateBookingCarbonRequestDto req)
        {
            var (success, message, data) = await _svc.EstimateForBookingAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, data });
        }

        [HttpGet("me")]
        public async Task<IActionResult> MySummary()
        {
            var data = await _svc.GetCustomerSummaryAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("admin/seed")]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            await _svc.SeedFactorsAsync();
            return Ok(new { success = true });
        }
    }
}
