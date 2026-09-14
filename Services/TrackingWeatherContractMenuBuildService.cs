using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class OrderTrackingTimelineService
    {
        private readonly AppDbContext _db;
        public OrderTrackingTimelineService(AppDbContext db) => _db = db;
        private static readonly string[] StageOrder = { "Confirmed", "Preparing", "OutForDelivery", "Delivered" };

        public async Task<(bool Success, string Message, OrderTrackingEventDto? Event)> AddEventAsync(AddTrackingEventRequestDto req)
        {
            if (!StageOrder.Contains(req.Stage) && req.Stage != "Cancelled") return (false, $"Invalid stage. Must be one of: {string.Join(", ", StageOrder)}, Cancelled.", null);
            if (!await _db.Bookings.AnyAsync(b => b.Id == req.BookingId)) return (false, "Booking not found.", null);

            var evt = new OrderTrackingEvent { BookingId = req.BookingId, Stage = req.Stage, EstimatedAt = req.EstimatedAt, Notes = req.Notes };
            _db.OrderTrackingEvents.Add(evt);
            await _db.SaveChangesAsync();
            return (true, "Tracking event added.", new OrderTrackingEventDto { Stage = evt.Stage, EstimatedAt = evt.EstimatedAt, OccurredAt = evt.OccurredAt, Notes = evt.Notes });
        }

        public async Task<OrderTrackingTimelineDto> GetTimelineAsync(int bookingId)
        {
            var events = await _db.OrderTrackingEvents.Where(e => e.BookingId == bookingId).OrderBy(e => e.OccurredAt).ToListAsync();
            return new OrderTrackingTimelineDto
            {
                BookingId = bookingId, CurrentStage = events.LastOrDefault()?.Stage ?? "Confirmed",
                Events = events.Select(e => new OrderTrackingEventDto { Stage = e.Stage, EstimatedAt = e.EstimatedAt, OccurredAt = e.OccurredAt, Notes = e.Notes }).ToList(),
            };
        }
    }

    public class WeatherRecommendationService
    {
        private readonly AppDbContext _db;
        public WeatherRecommendationService(AppDbContext db) => _db = db;

        public async Task<WeatherRuleDto> CreateRuleAsync(CreateWeatherRuleRequestDto req)
        {
            var rule = new WeatherRecommendationRule { WeatherCondition = req.WeatherCondition, SuggestedCuisines = req.SuggestedCuisines, Message = req.Message };
            _db.WeatherRecommendationRules.Add(rule);
            await _db.SaveChangesAsync();
            return ToDto(rule);
        }

        public async Task<List<WeatherRuleDto>> GetAllAsync() => (await _db.WeatherRecommendationRules.ToListAsync()).Select(ToDto).ToList();

        public async Task<WeatherRuleDto?> GetRecommendationAsync(GetWeatherRecommendationRequestDto req)
        {
            var rule = await _db.WeatherRecommendationRules.FirstOrDefaultAsync(r => r.WeatherCondition == req.WeatherCondition && r.IsActive);
            return rule == null ? null : ToDto(rule);
        }

        private static WeatherRuleDto ToDto(WeatherRecommendationRule r) => new()
        {
            Id = r.Id, WeatherCondition = r.WeatherCondition,
            SuggestedCuisines = r.SuggestedCuisines.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Message = r.Message,
        };
    }

    public class DigitalContractService
    {
        private readonly AppDbContext _db;
        public DigitalContractService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, DigitalContractDto? Contract)> CreateAsync(CreateDigitalContractRequestDto req)
        {
            if (!await _db.Bookings.AnyAsync(b => b.Id == req.BookingId)) return (false, "Booking not found.", null);
            var contract = new DigitalContract { BookingId = req.BookingId, TermsText = req.TermsText };
            _db.DigitalContracts.Add(contract);
            await _db.SaveChangesAsync();
            return (true, "Contract created.", ToDto(contract));
        }

        public async Task<(bool Success, string Message, DigitalContractDto? Contract)> AcceptAsync(AcceptContractRequestDto req)
        {
            var contract = await _db.DigitalContracts.FindAsync(req.ContractId);
            if (contract == null) return (false, "Contract not found.", null);
            if (contract.Status == "Voided") return (false, "This contract has been voided.", null);

            if (req.Role == "Customer") { contract.CustomerAccepted = true; contract.CustomerAcceptedAt = DateTime.UtcNow; contract.CustomerAcceptedIp = req.IpAddress; }
            else if (req.Role == "Chef") { contract.ChefAccepted = true; contract.ChefAcceptedAt = DateTime.UtcNow; contract.ChefAcceptedIp = req.IpAddress; }
            else return (false, "Role must be Customer or Chef.", null);

            if (contract.CustomerAccepted && contract.ChefAccepted) contract.Status = "FullyAccepted";
            await _db.SaveChangesAsync();
            return (true, "Accepted.", ToDto(contract));
        }

        public async Task<DigitalContractDto?> GetAsync(int contractId)
        {
            var contract = await _db.DigitalContracts.FindAsync(contractId);
            return contract == null ? null : ToDto(contract);
        }

        private static DigitalContractDto ToDto(DigitalContract c) => new()
        {
            Id = c.Id, BookingId = c.BookingId, TermsText = c.TermsText, CustomerAccepted = c.CustomerAccepted,
            ChefAccepted = c.ChefAccepted, Status = c.Status, CreatedAt = c.CreatedAt,
        };
    }

    public class CustomMenuBuildService
    {
        private readonly AppDbContext _db;
        public CustomMenuBuildService(AppDbContext db) => _db = db;

        public async Task<CustomMenuBuildDto> SaveAsync(SaveCustomMenuBuildRequestDto req)
        {
            var build = new CustomMenuBuild
            {
                CustomerId = req.CustomerId, ChefId = req.ChefId, BuildName = string.IsNullOrWhiteSpace(req.BuildName) ? "My custom order" : req.BuildName,
                SelectedItemsJson = System.Text.Json.JsonSerializer.Serialize(req.Items), EstimatedTotal = req.Items.Sum(i => i.Quantity * i.UnitPrice),
            };
            _db.CustomMenuBuilds.Add(build);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(build);
        }

        public async Task<List<CustomMenuBuildDto>> GetForCustomerAsync(int customerId)
        {
            var builds = await _db.CustomMenuBuilds.Where(b => b.CustomerId == customerId).OrderByDescending(b => b.CreatedAt).ToListAsync();
            var result = new List<CustomMenuBuildDto>();
            foreach (var b in builds) result.Add(await ToDtoAsync(b));
            return result;
        }

        public async Task<(bool Success, string Message)> SendToChefAsync(int buildId)
        {
            var build = await _db.CustomMenuBuilds.FindAsync(buildId);
            if (build == null) return (false, "Build not found.");
            build.Status = "SentToChef";
            await _db.SaveChangesAsync();
            return (true, "Sent to chef for confirmation.");
        }

        private async Task<CustomMenuBuildDto> ToDtoAsync(CustomMenuBuild b)
        {
            var chef = await _db.Users.FindAsync(b.ChefId);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<BuildItemRequestDto>>(b.SelectedItemsJson) ?? new();
            return new CustomMenuBuildDto { Id = b.Id, BuildName = b.BuildName, ChefName = chef?.FullName, Items = items, EstimatedTotal = b.EstimatedTotal, Status = b.Status };
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/order-tracking")]
    public class OrderTrackingTimelineController : ControllerBase
    {
        private readonly Services.OrderTrackingTimelineService _svc;
        public OrderTrackingTimelineController(Services.OrderTrackingTimelineService svc) => _svc = svc;

        [HttpPost("events"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> AddEvent([FromBody] AddTrackingEventRequestDto req)
        {
            var (success, message, evt) = await _svc.AddEventAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = evt });
        }

        [HttpGet("booking/{bookingId}"), Authorize]
        public async Task<IActionResult> GetTimeline(int bookingId) => Ok(new { success = true, data = await _svc.GetTimelineAsync(bookingId) });
    }

    [ApiController, Route("api/weather-recommendations")]
    public class WeatherRecommendationController : ControllerBase
    {
        private readonly Services.WeatherRecommendationService _svc;
        public WeatherRecommendationController(Services.WeatherRecommendationService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRule([FromBody] CreateWeatherRuleRequestDto req) => Ok(new { success = true, data = await _svc.CreateRuleAsync(req) });

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("get")]
        public async Task<IActionResult> GetRecommendation([FromBody] GetWeatherRecommendationRequestDto req)
        {
            var rec = await _svc.GetRecommendationAsync(req);
            if (rec == null) return Ok(new { success = true, data = (object?)null, message = "No recommendation for this condition." });
            return Ok(new { success = true, data = rec });
        }
    }

    [ApiController, Route("api/digital-contracts"), Authorize]
    public class DigitalContractController : ControllerBase
    {
        private readonly Services.DigitalContractService _svc;
        public DigitalContractController(Services.DigitalContractService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDigitalContractRequestDto req)
        {
            var (success, message, contract) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = contract });
        }

        [HttpPost("accept")]
        public async Task<IActionResult> Accept([FromBody] AcceptContractRequestDto req)
        {
            req.IpAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
            var (success, message, contract) = await _svc.AcceptAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = contract });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var contract = await _svc.GetAsync(id);
            if (contract == null) return NotFound(new { success = false, message = "Contract not found." });
            return Ok(new { success = true, data = contract });
        }
    }

    [ApiController, Route("api/custom-menu-builds"), Authorize]
    public class CustomMenuBuildController : ControllerBase
    {
        private readonly Services.CustomMenuBuildService _svc;
        public CustomMenuBuildController(Services.CustomMenuBuildService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveCustomMenuBuildRequestDto req) => Ok(new { success = true, data = await _svc.SaveAsync(req) });

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(int customerId) => Ok(new { success = true, data = await _svc.GetForCustomerAsync(customerId) });

        [HttpPost("send")]
        public async Task<IActionResult> SendToChef([FromBody] SendBuildToChefRequestDto req)
        {
            var (success, message) = await _svc.SendToChefAsync(req.BuildId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
