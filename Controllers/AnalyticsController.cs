using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsService _svc;
        public AnalyticsController(AnalyticsService svc) => _svc = svc;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            var data = await _svc.GetSummaryAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("trend")]
        public async Task<IActionResult> Trend([FromQuery] int months = 6)
        {
            var data = await _svc.GetMonthlyTrendAsync(months);
            return Ok(new { success = true, data });
        }
    }
}
