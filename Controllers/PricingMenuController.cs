using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    // ── Dynamic Pricing Controller ─────────────────────────────────
    [ApiController]
    [Route("api/pricing")]
    public class DynamicPricingController : ControllerBase
    {
        private readonly DynamicPricingService _svc;
        public DynamicPricingController(DynamicPricingService svc) => _svc = svc;

        /// <summary>Calculate price for a booking (public — used on booking screen)</summary>
        [HttpPost("calculate")]
        [AllowAnonymous]
        public async Task<IActionResult> Calculate([FromBody] PriceCalculationRequestDto req)
        {
            var result = await _svc.CalculatePriceAsync(req);
            return Ok(new { success = true, data = result });
        }

        /// <summary>Admin: Get all pricing rules</summary>
        [HttpGet("rules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRules()
        {
            var rules = await _svc.GetAllRulesAsync();
            return Ok(new { success = true, data = rules });
        }

        /// <summary>Admin: Create pricing rule</summary>
        [HttpPost("rules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRule([FromBody] CreatePricingRuleDto dto)
        {
            var rule = await _svc.CreateRuleAsync(dto);
            return Ok(new { success = true, data = rule });
        }

        /// <summary>Admin: Toggle rule active/inactive</summary>
        [HttpPost("rules/{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Toggle(int id)
        {
            var ok = await _svc.ToggleRuleAsync(id);
            return Ok(new { success = ok });
        }

        /// <summary>Admin: Delete rule</summary>
        [HttpDelete("rules/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteRuleAsync(id);
            return Ok(new { success = ok });
        }

        /// <summary>Admin: Get pricing stats</summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Stats()
        {
            var data = await _svc.GetStatsAsync();
            return Ok(new { success = true, data });
        }
    }

    // ── Menu Controller ────────────────────────────────────────────
    [ApiController]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly MenuService _svc;
        public MenuController(MenuService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Get any chef's public menu by profile ID</summary>
        [HttpGet("chef/{chefProfileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChefMenu(int chefProfileId)
        {
            var menu = await _svc.GetChefMenuAsync(chefProfileId);
            if (menu == null) return NotFound();
            return Ok(new { success = true, data = menu });
        }

        /// <summary>Chef: Get my menu</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> GetMy()
        {
            var menu = await _svc.GetMenuByUserIdAsync(UserId);
            return Ok(new { success = true, data = menu });
        }

        /// <summary>Chef: Add menu item</summary>
        [HttpPost("items")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> AddItem([FromBody] CreateMenuItemDto dto)
        {
            var profile = await _svc.GetMenuByUserIdAsync(UserId);
            if (profile == null) return BadRequest(new { success = false, message = "Chef profile not found" });
            var item = await _svc.AddItemAsync(profile.ChefProfileId, dto);
            return Ok(new { success = true, data = item });
        }

        /// <summary>Chef: Toggle item availability</summary>
        [HttpPost("items/{id}/toggle")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Toggle(int id)
        {
            var profile = await _svc.GetMenuByUserIdAsync(UserId);
            if (profile == null) return BadRequest();
            var ok = await _svc.ToggleAvailabilityAsync(id, profile.ChefProfileId);
            return Ok(new { success = ok });
        }

        /// <summary>Chef: Delete menu item</summary>
        [HttpDelete("items/{id}")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Delete(int id)
        {
            var profile = await _svc.GetMenuByUserIdAsync(UserId);
            if (profile == null) return BadRequest();
            var ok = await _svc.DeleteItemAsync(id, profile.ChefProfileId);
            return Ok(new { success = ok });
        }

        /// <summary>Admin: Get all menus</summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var menus = await _svc.GetAllMenusAsync();
            return Ok(new { success = true, data = menus });
        }
    }
}
