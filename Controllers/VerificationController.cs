using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/verification")]
    [Authorize]
    public class VerificationController : ControllerBase
    {
        private readonly VerificationService _svc;
        public VerificationController(VerificationService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("submit")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Submit([FromBody] SubmitVerificationRequestDto req)
        {
            var (success, message) = await _svc.SubmitDocumentAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("status")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Status()
        {
            var data = await _svc.GetStatusAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var data = await _svc.GetPendingForAdminAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("admin/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review([FromBody] ReviewVerificationRequestDto req)
        {
            var (success, message) = await _svc.ReviewAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
