using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    /// <summary>
    /// Standalone OTP send/resend endpoints. Verification itself happens as
    /// part of AuthController.VerifyOtp, since a successful verify needs to
    /// immediately issue tokens — kept together there rather than split
    /// across two round trips.
    /// </summary>
    [ApiController]
    [Route("api/otp")]
    public class OtpController : ControllerBase
    {
        private readonly OtpService _svc;
        public OtpController(OtpService svc) => _svc = svc;

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendOtpRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Phone))
                return BadRequest(new { success = false, message = "Phone number is required." });

            var (success, message) = await _svc.SendOtpAsync(req.Phone, req.Purpose);
            return Ok(new { success, message });
        }

        [HttpPost("resend")]
        public async Task<IActionResult> Resend([FromBody] SendOtpRequestDto req)
        {
            var (success, message) = await _svc.SendOtpAsync(req.Phone, req.Purpose);
            return Ok(new { success, message });
        }
    }
}
