using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    // ── Support Controller ─────────────────────────────────────────
    [ApiController]
    [Route("api/support")]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private readonly SupportService _svc;
        public SupportController(SupportService svc) => _svc = svc;

        private int    UserId   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

        /// <summary>Create a new support ticket</summary>
        [HttpPost("tickets")]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
        {
            var ticket = await _svc.CreateTicketAsync(UserId, dto);
            return Ok(new { success = true, data = ticket, message = "Ticket created successfully" });
        }

        /// <summary>Get current user's tickets</summary>
        [HttpGet("tickets/my")]
        public async Task<IActionResult> GetMy()
        {
            var tickets = await _svc.GetMyTicketsAsync(UserId);
            return Ok(new { success = true, data = tickets });
        }

        /// <summary>Get single ticket with messages</summary>
        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var isAdmin = UserRole == "Admin";
            var ticket  = await _svc.GetTicketAsync(id, UserId, isAdmin);
            if (ticket == null) return NotFound(new { success = false, message = "Ticket not found" });
            return Ok(new { success = true, data = ticket });
        }

        /// <summary>Reply to a ticket</summary>
        [HttpPost("tickets/reply")]
        public async Task<IActionResult> Reply([FromBody] ReplyTicketDto dto)
        {
            var role = UserRole == "Admin" ? "Admin" : "User";
            var msg  = await _svc.ReplyAsync(UserId, role, dto);
            if (msg == null) return NotFound();
            return Ok(new { success = true, data = msg });
        }

        /// <summary>Admin: Update ticket status</summary>
        [HttpPut("tickets/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto dto)
        {
            var ok = await _svc.UpdateStatusAsync(id, dto);
            return Ok(new { success = ok });
        }

        /// <summary>Admin: Get all tickets + stats</summary>
        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminStats()
        {
            var data = await _svc.GetAdminStatsAsync();
            return Ok(new { success = true, data });
        }

        /// <summary>Get FAQs (public)</summary>
        [HttpGet("faqs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFaqs([FromQuery] string? audience)
        {
            var faqs = await _svc.GetFaqsAsync(audience);
            return Ok(new { success = true, data = faqs });
        }
    }

    // ── Campaign Controller ────────────────────────────────────────
    [ApiController]
    [Route("api/campaigns")]
    [Authorize(Roles = "Admin")]
    public class CampaignController : ControllerBase
    {
        private readonly CampaignService _svc;
        public CampaignController(CampaignService svc) => _svc = svc;

        private int AdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Create a new campaign</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
        {
            var campaign = await _svc.CreateAsync(AdminId, dto);
            return Ok(new { success = true, data = campaign });
        }

        /// <summary>Send campaign immediately</summary>
        [HttpPost("{id}/send")]
        public async Task<IActionResult> Send(int id)
        {
            var campaign = await _svc.SendNowAsync(id);
            if (campaign == null) return NotFound();
            return Ok(new { success = true, data = campaign, message = "Campaign sent!" });
        }

        /// <summary>Get all campaigns</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _svc.GetAllAsync();
            return Ok(new { success = true, data = list });
        }

        /// <summary>Get single campaign</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var c = await _svc.GetByIdAsync(id);
            if (c == null) return NotFound();
            return Ok(new { success = true, data = c });
        }

        /// <summary>Cancel a campaign</summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _svc.CancelAsync(id);
            return Ok(new { success = ok });
        }

        /// <summary>Get campaign stats</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            var data = await _svc.GetStatsAsync();
            return Ok(new { success = true, data });
        }
    }
}
