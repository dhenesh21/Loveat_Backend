using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/chef-profile")]
    public class ChefProfileController : ControllerBase
    {
        private readonly ChefProfileService _svc;
        public ChefProfileController(ChefProfileService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("me")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> GetMine()
        {
            var data = await _svc.GetMyProfileAsync(UserId);
            return Ok(new { success = true, data });
        }

        /// <summary>Public — used by CustomerApp when viewing a chef's profile from search results.</summary>
        [HttpGet("{chefProfileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublic(int chefProfileId)
        {
            var data = await _svc.GetPublicProfileAsync(chefProfileId);
            if (data == null) return NotFound(new { success = false, message = "Chef not found." });
            return Ok(new { success = true, data });
        }

        [HttpPut("me")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Update([FromBody] UpdateChefProfileRequestDto req)
        {
            var (success, message) = await _svc.UpdateAsync(UserId, req);
            return Ok(new { success, message });
        }

        /// <summary>Admin: every chef's profile, for the chef directory dashboard</summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAll()
        {
            var data = await _svc.GetAllForAdminAsync();
            return Ok(new { success = true, data });
        }
    }
}
