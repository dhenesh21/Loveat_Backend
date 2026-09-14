using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    /// <summary>REST fallback/history alongside the live ChatHub — a client not currently connected to the hub (e.g. opening ChatListScreen) still needs a way to load past messages.</summary>
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _svc;
        public ChatController(ChatService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("threads")]
        public async Task<IActionResult> Threads()
        {
            var data = await _svc.GetThreadsAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> History(int otherUserId, [FromQuery] int take = 50)
        {
            var data = await _svc.GetHistoryAsync(UserId, otherUserId, take);
            return Ok(new { success = true, data });
        }

        /// <summary>REST send for clients not connected to the SignalR hub. Prefer ChatHub.SendMessage when connected, for live delivery.</summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendMessageRequestDto req)
        {
            var data = await _svc.SendMessageAsync(UserId, req);
            if (data == null) return BadRequest(new { success = false, message = "Message text or attachment is required." });
            return Ok(new { success = true, data });
        }

        [HttpPost("read/{otherUserId}")]
        public async Task<IActionResult> MarkRead(int otherUserId)
        {
            await _svc.MarkThreadReadAsync(UserId, otherUserId);
            return Ok(new { success = true });
        }
    }
}
