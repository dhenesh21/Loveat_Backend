using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/emergency")]
    // NOTE: role checks moved to per-action [Authorize] below. A class-level
    // [Authorize(Roles="Customer")] combined with an action-level
    // [Authorize(Roles="Admin")] would AND together (both must match a
    // single role claim), which is never satisfiable — it would 403 the
    // admin endpoint for every admin, permanently.
    [Authorize]
    public class EmergencyController : ControllerBase
    {
        private readonly EmergencyService _svc;
        public EmergencyController(EmergencyService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("book")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Book([FromBody] BookEmergencyChefRequestDto req)
        {
            var (success, message, data) = await _svc.BookNearestAvailableAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        /// <summary>Admin: live emergency requests + chefs currently available for emergency bookings</summary>
        [HttpGet("admin/active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminActive()
        {
            var data = await _svc.GetAdminOverviewAsync();
            return Ok(new { success = true, data });
        }
    }
}
