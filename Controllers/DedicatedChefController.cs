using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    // M182: Dedicated/preferred chef. M183: Event support-staff assignment.
    // (M184 BookingTimeoutAlertService has no dedicated controller — it's used
    // internally by BookingService's Accept/timeout flow, not called directly.)
    [ApiController, Route("api/dedicated-chef"), Authorize]
    public class DedicatedChefController : ControllerBase
    {
        private readonly DedicatedChefService _svc;
        public DedicatedChefController(DedicatedChefService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPut("preferred-chef")]
        public async Task<IActionResult> SetPreferredChef([FromBody] SetPreferredChefRequestDto req)
        {
            var (ok, msg) = await _svc.SetPreferredChefAsync(UserId, req);
            return Ok(new { success = ok, message = msg });
        }
    }

    [ApiController, Route("api/event-staff"), Authorize]
    public class EventStaffController : ControllerBase
    {
        private readonly EventStaffService _svc;
        public EventStaffController(EventStaffService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddEventStaffRequestDto req)
        {
            var (ok, msg, data) = await _svc.AddRequestAsync(UserId, req);
            return Ok(new { success = ok, message = msg, data });
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Assign([FromBody] AssignSupportStaffRequestDto req)
        {
            var (ok, msg) = await _svc.AssignStaffAsync(UserId, req);
            return Ok(new { success = ok, message = msg });
        }

        [HttpGet("bookings/{bookingId}")]
        public async Task<IActionResult> GetForBooking(int bookingId)
        {
            var data = await _svc.GetForBookingAsync(bookingId);
            return Ok(new { success = true, data });
        }
    }
}
