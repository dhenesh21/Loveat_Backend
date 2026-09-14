using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    /// <summary>
    /// M16: customer -> chef reviews. Shares the "api/reviews" prefix with
    /// the pre-existing CustomerRatingController (rate-customer, M37) —
    /// action paths don't overlap so both controllers coexist fine.
    /// </summary>
    [ApiController]
    [Route("api/reviews")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _svc;
        public ReviewController(ReviewService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequestDto req)
        {
            var (success, message, data) = await _svc.CreateAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpPost("respond")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Respond([FromBody] ChefRespondToReviewRequestDto req)
        {
            var (success, message) = await _svc.ChefRespondAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("chef/{chefUserId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ChefSummary(int chefUserId)
        {
            var data = await _svc.GetChefReviewSummaryAsync(chefUserId);
            return Ok(new { success = true, data });
        }

        /// <summary>Admin: moderate reviews across the platform</summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAll([FromQuery] bool onlyFlagged = false)
        {
            var data = await _svc.GetAllForAdminAsync(onlyFlagged);
            return Ok(new { success = true, data });
        }

        [HttpPost("admin/{id}/flag")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminFlag(int id, [FromQuery] bool flagged = true)
        {
            var (success, message) = await _svc.SetFlaggedAsync(id, flagged);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDelete(int id)
        {
            var (success, message) = await _svc.AdminDeleteAsync(id);
            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }
    }
}
