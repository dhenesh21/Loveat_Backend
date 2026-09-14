using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _svc;
        public NotificationController(NotificationService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool unreadOnly = false)
        {
            var data = await _svc.GetMyNotificationsAsync(UserId, unreadOnly);
            return Ok(new { success = true, data });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _svc.GetUnreadCountAsync(UserId);
            return Ok(new { success = true, count });
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _svc.MarkReadAsync(UserId, id);
            return Ok(new { success = true });
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _svc.MarkAllReadAsync(UserId);
            return Ok(new { success = true });
        }

        [HttpPost("register-device")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceTokenRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Token))
                return BadRequest(new { success = false, message = "Device token is required." });

            await _svc.RegisterDeviceTokenAsync(UserId, req.Token, req.Platform);
            return Ok(new { success = true, message = "Device registered for push notifications." });
        }
    }
}
