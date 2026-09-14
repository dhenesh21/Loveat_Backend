using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/capacity-planning"), Authorize(Roles = "Admin")]
    public class CapacityPlanningController : ControllerBase
    {
        private readonly CapacityPlanningService _svc;
        public CapacityPlanningController(CapacityPlanningService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateCapacityForecastRequestDto req) => Ok(new { success = true, data = await _svc.GenerateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetHistory() => Ok(new { success = true, data = await _svc.GetHistoryAsync() });
    }

    [ApiController, Route("api/dependency-health"), Authorize(Roles = "Admin")]
    public class DependencyHealthController : ControllerBase
    {
        private readonly DependencyHealthService _svc;
        public DependencyHealthController(DependencyHealthService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Log([FromBody] LogDependencyHealthRequestDto req) => Ok(new { success = true, data = await _svc.LogAsync(req) });

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview() => Ok(new { success = true, data = await _svc.GetOverviewAsync() });
    }

    [ApiController, Route("api/infra-cost"), Authorize(Roles = "Admin")]
    public class InfraCostController : ControllerBase
    {
        private readonly InfraCostService _svc;
        public InfraCostController(InfraCostService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Log([FromBody] LogInfraCostRequestDto req) => Ok(new { success = true, data = await _svc.LogAsync(req) });

        [HttpGet("summary/{periodKey}")]
        public async Task<IActionResult> GetSummary(string periodKey) => Ok(new { success = true, data = await _svc.GetSummaryAsync(periodKey) });
    }

    [ApiController, Route("api/api-versions"), Authorize(Roles = "Admin")]
    public class ApiVersionController : ControllerBase
    {
        private readonly ApiVersionService _svc;
        public ApiVersionController(ApiVersionService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApiVersionRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("deprecate")]
        public async Task<IActionResult> Deprecate([FromBody] DeprecateApiVersionRequestDto req)
        {
            var (success, message) = await _svc.DeprecateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/dr-drills"), Authorize(Roles = "Admin")]
    public class DrDrillController : ControllerBase
    {
        private readonly DrDrillService _svc;
        public DrDrillController(DrDrillService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Log([FromBody] LogDrDrillRequestDto req) => Ok(new { success = true, data = await _svc.LogAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }

    [ApiController, Route("api/ops-dashboard"), Authorize(Roles = "Admin")]
    public class OpsDashboardController : ControllerBase
    {
        private readonly OpsDashboardService _svc;
        public OpsDashboardController(OpsDashboardService svc) => _svc = svc;

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary() => Ok(new { success = true, data = await _svc.GetSummaryAsync() });
    }
}
