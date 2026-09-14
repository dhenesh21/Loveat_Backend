using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    // ── Loyalty Controller ─────────────────────────────────────────
    [ApiController]
    [Route("api/loyalty")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly LoyaltyService _svc;
        public LoyaltyController(LoyaltyService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Get current user's loyalty points and tier status</summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var data = await _svc.GetStatusAsync(UserId);
            return Ok(new { success = true, data });
        }

        /// <summary>Redeem points for a reward</summary>
        [HttpPost("redeem")]
        public async Task<IActionResult> Redeem([FromBody] RedeemPointsRequestDto req)
        {
            var result = await _svc.RedeemPointsAsync(UserId, req);
            return Ok(new { success = result.Success, data = result, message = result.Message });
        }

        /// <summary>Admin: Get loyalty program stats</summary>
        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminStats()
        {
            var data = await _svc.GetAdminStatsAsync();
            return Ok(new { success = true, data });
        }
    }

    // ── Referral Controller ────────────────────────────────────────
    [ApiController]
    [Route("api/referral")]
    [Authorize]
    public class ReferralController : ControllerBase
    {
        private readonly ReferralService _svc;
        public ReferralController(ReferralService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Get current user's referral code and stats</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var data = await _svc.GetReferralStatusAsync(UserId);
            return Ok(new { success = true, data });
        }

        /// <summary>Apply a referral code (new user signup)</summary>
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyReferralRequestDto req)
        {
            var result = await _svc.ApplyReferralCodeAsync(UserId, req.Code);
            return Ok(new { success = result.Success, data = result, message = result.Message });
        }

        /// <summary>Admin: Get referral program stats</summary>
        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminStats()
        {
            var data = await _svc.GetAdminStatsAsync();
            return Ok(new { success = true, data });
        }
    }
}
