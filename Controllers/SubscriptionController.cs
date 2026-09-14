using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService _svc;
        public SubscriptionController(SubscriptionService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("plans")]
        [AllowAnonymous]
        public IActionResult Plans() => Ok(new { success = true, data = _svc.GetPlans() });

        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionRequestDto req)
        {
            var (success, message, data) = await _svc.SubscribeAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Mine()
        {
            var data = await _svc.GetMineAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("pause")]
        public async Task<IActionResult> Pause()
        {
            var (success, message) = await _svc.PauseAsync(UserId);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("resume")]
        public async Task<IActionResult> Resume()
        {
            var (success, message) = await _svc.ResumeAsync(UserId);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            var (success, message) = await _svc.CancelAsync(UserId);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }
    }
}
