using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _svc;
        private readonly OtpService _otp;

        public AuthController(AuthService svc, OtpService otp)
        {
            _svc = svc;
            _otp = otp;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Step 1 of login/register: request an OTP for a phone number.</summary>
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Phone))
                return BadRequest(new { success = false, message = "Phone number is required." });

            var (success, message) = await _otp.SendOtpAsync(req.Phone, req.Purpose);
            return Ok(new { success, message });
        }

        /// <summary>
        /// Step 2: verify the OTP. Creates the account automatically if this
        /// phone number hasn't been seen before, then returns JWT tokens
        /// either way — this single endpoint covers both login and register.
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Phone) || string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new { success = false, message = "Phone and code are required." });

            var result = await _svc.VerifyOtpAndAuthenticateAsync(req);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>Returns the currently authenticated user, for app startup/session restore.</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await _svc.GetCurrentUserAsync(UserId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            return Ok(new { success = true, data = user });
        }

        /// <summary>Exchanges a valid refresh token for a new access token, without requiring the user to OTP-verify again.</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
                return BadRequest(new { success = false, message = "Refresh token is required." });

            var result = await _svc.RefreshAsync(req.RefreshToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _svc.LogoutAsync(UserId);
            return Ok(new { success = true, message = "Logged out." });
        }
    }
}
