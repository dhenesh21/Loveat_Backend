using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _svc;
        public BookingController(BookingService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string Role => User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequestDto req)
        {
            var (success, message, data) = await _svc.CreateBookingAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetMine([FromQuery] string? status)
        {
            var data = await _svc.GetMyBookingsAsync(UserId, Role, status);
            return Ok(new { success = true, count = data.Count, data });
        }

        /// <summary>Admin: all bookings across every customer/chef, optionally filtered by status</summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAll([FromQuery] string? status)
        {
            var data = await _svc.GetAllBookingsAsync(status);
            return Ok(new { success = true, count = data.Count, data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var data = await _svc.GetBookingAsync(UserId, id);
            if (data == null) return NotFound(new { success = false, message = "Booking not found." });
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/accept")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Accept(int id)
        {
            var (success, message) = await _svc.AcceptBookingAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("{id}/start")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Start(int id)
        {
            var (success, message) = await _svc.StartBookingAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Complete(int id)
        {
            var (success, message) = await _svc.CompleteBookingAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] UpdateBookingStatusRequestDto req)
        {
            var (success, message) = await _svc.CancelBookingAsync(UserId, id, req.CancellationReason);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
