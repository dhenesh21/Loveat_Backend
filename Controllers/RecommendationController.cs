using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    [Authorize(Roles = "Customer")]
    public class RecommendationController : ControllerBase
    {
        private readonly AIRecommendationService _svc;
        public RecommendationController(AIRecommendationService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("chefs")]
        public async Task<IActionResult> Chefs([FromQuery] int take = 10)
        {
            var data = await _svc.GetRecommendationsAsync(UserId, take);
            return Ok(new { success = true, data });
        }
    }
}
