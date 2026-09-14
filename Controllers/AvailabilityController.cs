using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/availability")]
    [Authorize(Roles = "Chef")]
    public class AvailabilityController : ControllerBase
    {
        private readonly AvailabilityService _svc;
        public AvailabilityController(AvailabilityService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var data = await _svc.GetStatusAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPut("general")]
        public async Task<IActionResult> SetGeneral([FromBody] SetGeneralAvailabilityRequestDto req)
        {
            var (success, message) = await _svc.SetGeneralAvailabilityAsync(UserId, req.IsAvailable);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPut("emergency")]
        public async Task<IActionResult> SetEmergency([FromBody] SetEmergencyAvailabilityRequestDto req)
        {
            var (success, message) = await _svc.SetEmergencyAvailabilityAsync(UserId, req);
            return Ok(new { success, message });
        }
    }
}
