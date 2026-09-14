using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/heatmap")]
    [Authorize(Roles = "Admin")]
    public class HeatMapController : ControllerBase
    {
        private readonly HeatMapService _svc;
        public HeatMapController(HeatMapService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? city)
        {
            var data = await _svc.GetHeatMapAsync(city);
            return Ok(new { success = true, data });
        }

        /// <summary>Manual trigger until G7 (Hangfire scheduled jobs) exists to run this automatically.</summary>
        [HttpPost("admin/recompute")]
        public async Task<IActionResult> Recompute()
        {
            var count = await _svc.RecomputeAsync();
            return Ok(new { success = true, message = $"Recomputed {count} city entries." });
        }
    }
}
