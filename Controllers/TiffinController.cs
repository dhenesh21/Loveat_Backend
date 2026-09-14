using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/corporate-tiffin")]
    [Authorize]
    public class TiffinController : ControllerBase
    {
        private readonly CorporateTiffinService _svc;
        public TiffinController(CorporateTiffinService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCorporateTiffinRequestDto req)
        {
            var (success, message, data) = await _svc.CreateAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpGet]
        public async Task<IActionResult> Mine()
        {
            var data = await _svc.GetMineAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/pause")]
        public async Task<IActionResult> Pause(int id)
        {
            var (success, message) = await _svc.SetStatusAsync(UserId, id, "Paused");
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("{id}/resume")]
        public async Task<IActionResult> Resume(int id)
        {
            var (success, message) = await _svc.SetStatusAsync(UserId, id, "Active");
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var (success, message) = await _svc.SetStatusAsync(UserId, id, "Cancelled");
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        /// <summary>Admin: all corporate tiffin subscriptions across every company</summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAll()
        {
            var data = await _svc.GetAllAsync();
            return Ok(new { success = true, data });
        }
    }
}
