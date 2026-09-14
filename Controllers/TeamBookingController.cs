using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/team-booking")]
    [Authorize]
    public class TeamBookingController : ControllerBase
    {
        private readonly TeamBookingService _svc;
        public TeamBookingController(TeamBookingService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("assign")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Assign([FromBody] AssignTeamChefRequestDto req)
        {
            var (success, message, data) = await _svc.AssignChefAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpPost("{assignmentId}/respond")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Respond(int assignmentId, [FromQuery] bool accept)
        {
            var (success, message) = await _svc.RespondAsync(UserId, assignmentId, accept);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetTeam(int bookingId)
        {
            var data = await _svc.GetTeamAsync(bookingId);
            return Ok(new { success = true, data });
        }

        /// <summary>Admin: every booking that has a chef team assigned</summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAll()
        {
            var data = await _svc.GetAllTeamBookingsAsync();
            return Ok(new { success = true, data });
        }
    }
}
