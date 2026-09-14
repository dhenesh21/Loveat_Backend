using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/system-incidents"), Authorize(Roles = "Admin")]
    public class SystemIncidentController : ControllerBase
    {
        private readonly SystemIncidentService _svc;
        public SystemIncidentController(SystemIncidentService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSystemIncidentRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false) => Ok(new { success = true, data = await _svc.GetAllAsync(activeOnly) });

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateIncidentStatusRequestDto req)
        {
            var (success, message) = await _svc.UpdateStatusAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("oncall")]
        public async Task<IActionResult> CreateOnCall([FromBody] CreateOnCallScheduleRequestDto req) => Ok(new { success = true, data = await _svc.CreateOnCallAsync(req) });

        [HttpGet("oncall")]
        public async Task<IActionResult> GetOnCall() => Ok(new { success = true, data = await _svc.GetOnCallScheduleAsync() });
    }

    [ApiController, Route("api/backup-tracking"), Authorize(Roles = "Admin")]
    public class BackupTrackingController : ControllerBase
    {
        private readonly BackupTrackingService _svc;
        public BackupTrackingController(BackupTrackingService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Log([FromBody] LogBackupRequestDto req) => Ok(new { success = true, data = await _svc.LogAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetRecent([FromQuery] int take = 30) => Ok(new { success = true, data = await _svc.GetRecentAsync(take) });

        [HttpGet("health")]
        public async Task<IActionResult> GetHealth() => Ok(new { success = true, data = await _svc.GetHealthSummaryAsync() });
    }

    [ApiController, Route("api/runbooks"), Authorize(Roles = "Admin")]
    public class RunbookController : ControllerBase
    {
        private readonly RunbookService _svc;
        public RunbookController(RunbookService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRunbookRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? category) => Ok(new { success = true, data = await _svc.GetAllAsync(category) });

        [HttpPost("used")]
        public async Task<IActionResult> MarkUsed([FromBody] MarkRunbookUsedRequestDto req)
        {
            var (success, message) = await _svc.MarkUsedAsync(req.RunbookId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/change-requests"), Authorize(Roles = "Admin")]
    public class ChangeManagementController : ControllerBase
    {
        private readonly ChangeManagementService _svc;
        public ChangeManagementController(ChangeManagementService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateChangeRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("decide")]
        public async Task<IActionResult> Decide([FromBody] DecideChangeRequestDto req)
        {
            var (success, message) = await _svc.DecideAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("deployed")]
        public async Task<IActionResult> MarkDeployed([FromBody] MarkChangeDeployedRequestDto req)
        {
            var (success, message) = await _svc.MarkDeployedAsync(req.ChangeRequestId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
