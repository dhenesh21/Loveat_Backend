using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _svc;
        public SettingsController(SettingsService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Public — both mobile apps read these at startup (feature flags, general config).</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? category)
        {
            var data = await _svc.GetAllAsync(category);
            return Ok(new { success = true, data });
        }

        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Upsert([FromBody] UpsertAppSettingRequestDto req)
        {
            var (success, message) = await _svc.UpsertAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpDelete("admin/{key}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string key)
        {
            var (success, message) = await _svc.DeleteAsync(key);
            if (!success) return NotFound(new { success, message });
            return Ok(new { success, message });
        }
    }
}
