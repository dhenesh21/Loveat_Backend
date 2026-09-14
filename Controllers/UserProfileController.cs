using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly UserProfileService _svc;
        public UserProfileController(UserProfileService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _svc.GetProfileAsync(UserId);
            if (data == null) return NotFound(new { success = false, message = "User not found." });
            return Ok(new { success = true, data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProfileRequestDto req)
        {
            var (success, message) = await _svc.UpdateProfileAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var data = await _svc.GetLocationsAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("locations")]
        public async Task<IActionResult> SaveLocation([FromBody] SaveLocationRequestDto req)
        {
            var (success, message, data) = await _svc.SaveLocationAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpDelete("locations/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var (success, message) = await _svc.DeleteLocationAsync(UserId, id);
            if (!success) return NotFound(new { success, message });
            return Ok(new { success, message });
        }
    }
}
