using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    /// <summary>User-facing fraud reporting. Admin review/management of all flags (manual + automatic) lives in batch 23's FraudDetectionService/controller.</summary>
    [ApiController]
    [Route("api/fraud-reports")]
    [Authorize]
    public class FraudController : ControllerBase
    {
        private readonly FraudService _svc;
        public FraudController(FraudService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitFraudReportRequestDto req)
        {
            var (success, message) = await _svc.SubmitReportAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("mine")]
        public async Task<IActionResult> Mine()
        {
            var data = await _svc.GetMyReportsAsync(UserId);
            return Ok(new { success = true, data });
        }
    }
}
