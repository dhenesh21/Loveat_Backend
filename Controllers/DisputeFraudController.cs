using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/disputes"), Authorize]
    public class DisputeController : ControllerBase
    {
        private readonly DisputeService _svc;
        public DisputeController(DisputeService svc) => _svc = svc;
        private int    UserId   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDisputeDto dto)
        {
            var d = await _svc.CreateAsync(UserId, dto);
            return Ok(new { success = true, data = d, message = $"Dispute {d.DisputeNumber} created" });
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var data = await _svc.GetUserDisputesAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> Reply(int id, [FromBody] dynamic req)
        {
            string msg  = (string)(req.message ?? "");
            string role = UserRole == "Admin" ? "Admin" : UserRole == "Chef" ? "Chef" : "Customer";
            var ok = await _svc.ReplyAsync(id, UserId, role, msg);
            return Ok(new { success = ok });
        }

        [HttpGet("admin/all"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAll()
        {
            var data = await _svc.GetAllAdminAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("admin/{id}/resolve"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(int id, [FromBody] ResolveDisputeDto dto)
        {
            var ok = await _svc.ResolveAsync(id, UserId, dto);
            return Ok(new { success = ok });
        }
    }

    [ApiController, Route("api/fraud-detection"), Authorize(Roles = "Admin")]
    public class FraudDetectionController : ControllerBase
    {
        private readonly FraudDetectionService _svc;
        public FraudDetectionController(FraudDetectionService svc) => _svc = svc;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            var data = await _svc.GetSummaryAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("{id}/action")]
        public async Task<IActionResult> Action(int id, [FromBody] dynamic req)
        {
            string action = (string)(req.action ?? "Flagged");
            string? note  = (string?)(req.note);
            var ok = await _svc.UpdateActionAsync(id, action, note);
            return Ok(new { success = ok });
        }
    }
}
