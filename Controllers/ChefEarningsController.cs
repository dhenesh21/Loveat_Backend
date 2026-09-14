using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    // ── Chef Earnings Controller ────────────────────────────────────
    [ApiController]
    [Route("api/earnings")]
    [Authorize]
    public class ChefEarningsController : ControllerBase
    {
        private readonly ChefEarningsService _svc;
        public ChefEarningsController(ChefEarningsService svc) => _svc = svc;

        private int    UserId   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "";

        /// <summary>Chef: Get my earnings overview with charts</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var data = await _svc.GetOverviewAsync(UserId);
            return Ok(new { success = true, data });
        }

        /// <summary>Chef: Get specific chef earnings (admin or self)</summary>
        [HttpGet("chef/{chefId}")]
        public async Task<IActionResult> GetChef(int chefId)
        {
            if (UserRole != "Admin" && UserId != chefId)
                return Forbid();
            var data = await _svc.GetOverviewAsync(chefId);
            return Ok(new { success = true, data });
        }

        /// <summary>Admin: Get all chefs earnings summary</summary>
        [HttpGet("admin/summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminSummary()
        {
            var data = await _svc.GetAdminSummaryAsync();
            return Ok(new { success = true, data });
        }
    }
}
