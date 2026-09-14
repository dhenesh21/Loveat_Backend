using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    // M179: Gender-preference/safety filters. M180: Trusted-contact alerts.
    // M181: Arrival OTP verification.
    [ApiController, Route("api/safety-preferences"), Authorize]
    public class SafetyPreferenceController : ControllerBase
    {
        private readonly SafetyPreferenceService _svc;
        public SafetyPreferenceController(SafetyPreferenceService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _svc.GetOrCreateAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSafetyPreferenceRequestDto req)
        {
            var data = await _svc.UpdateAsync(UserId, req);
            return Ok(new { success = true, data });
        }
    }

    [ApiController, Route("api/trusted-contact"), Authorize]
    public class TrustedContactController : ControllerBase
    {
        private readonly TrustedContactAlertService _svc;
        public TrustedContactController(TrustedContactAlertService svc) => _svc = svc;

        [HttpGet("bookings/{bookingId}")]
        public async Task<IActionResult> GetForBooking(int bookingId)
        {
            var data = await _svc.GetForBookingAsync(bookingId);
            return Ok(new { success = true, data });
        }
    }

    [ApiController, Route("api/arrival-verification"), Authorize]
    public class ArrivalVerificationController : ControllerBase
    {
        private readonly ArrivalVerificationService _svc;
        public ArrivalVerificationController(ArrivalVerificationService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("bookings/{bookingId}/otp")]
        public async Task<IActionResult> GetOtp(int bookingId)
        {
            var otp = await _svc.GetOtpForCustomerAsync(UserId, bookingId);
            if (otp == null) return NotFound(new { success = false, message = "Not found or not your booking" });
            return Ok(new { success = true, data = new { otp } });
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitArrivalOtpRequestDto req)
        {
            var (ok, msg) = await _svc.SubmitOtpAsync(UserId, req);
            return Ok(new { success = ok, message = msg });
        }

        [HttpPost("skip")]
        public async Task<IActionResult> Skip([FromBody] SkipArrivalVerificationRequestDto req)
        {
            var ok = await _svc.SkipAsync(UserId, req);
            return Ok(new { success = ok });
        }

        [HttpGet("bookings/{bookingId}")]
        public async Task<IActionResult> Get(int bookingId)
        {
            var data = await _svc.GetAsync(bookingId);
            if (data == null) return NotFound(new { success = false });
            return Ok(new { success = true, data });
        }
    }
}
