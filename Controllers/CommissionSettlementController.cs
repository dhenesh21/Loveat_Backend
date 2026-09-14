using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    // ── M41: Commission Controller ─────────────────────────────────
    [ApiController, Route("api/commission"), Authorize(Roles = "Admin")]
    public class CommissionController : ControllerBase
    {
        private readonly CommissionService _svc;
        public CommissionController(CommissionService svc) => _svc = svc;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            var data = await _svc.GetSummaryAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] CreateCommissionRuleDto dto)
        {
            var rule = await _svc.CreateRuleAsync(dto);
            return Ok(new { success = true, data = rule });
        }

        [HttpPost("rules/{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var ok = await _svc.ToggleRuleAsync(id);
            return Ok(new { success = ok });
        }

        // Called internally from BookingService on completion
        [HttpPost("record")]
        [AllowAnonymous] // internal call from service
        public async Task<IActionResult> Record([FromBody] dynamic req)
        {
            int bookingId     = (int)(req.bookingId ?? 0);
            int chefId        = (int)(req.chefId ?? 0);
            decimal amount    = (decimal)(req.amount ?? 0);
            var entry         = await _svc.RecordCommissionAsync(bookingId, chefId, amount);
            return Ok(new { success = true, data = entry });
        }
    }

    // ── M42: Settlement Controller ─────────────────────────────────
    [ApiController, Route("api/settlement"), Authorize]
    public class SettlementController : ControllerBase
    {
        private readonly SettlementService _svc;
        public SettlementController(SettlementService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Chef: Get wallet overview + bank accounts + history</summary>
        [HttpGet("wallet")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> GetWallet()
        {
            var data = await _svc.GetWalletOverviewAsync(UserId);
            return Ok(new { success = true, data });
        }

        /// <summary>Chef: Add bank account</summary>
        [HttpPost("bank-accounts")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> AddBank([FromBody] AddBankAccountDto dto)
        {
            var account = await _svc.AddBankAccountAsync(UserId, dto);
            return Ok(new { success = true, data = account });
        }

        /// <summary>Chef: Request withdrawal</summary>
        [HttpPost("withdraw")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto dto)
        {
            var (success, message, data) = await _svc.RequestWithdrawalAsync(UserId, dto);
            return Ok(new { success, message, data });
        }

        /// <summary>Admin: Get settlement requests, optionally filtered by status ("All" or omitted = Pending only, for backwards-compat with the old default)</summary>
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminPending([FromQuery] string? status)
        {
            var data = await _svc.GetForAdminAsync(status ?? "Pending");
            return Ok(new { success = true, data });
        }

        /// <summary>Admin: Process (approve/reject) settlement</summary>
        [HttpPost("admin/{id}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Process(int id, [FromBody] dynamic req)
        {
            string status  = (string)(req.status ?? "Completed");
            string? txnRef = (string?)(req.transactionRef);
            string? reason = (string?)(req.rejectionReason);
            var ok         = await _svc.ProcessSettlementAsync(id, status, txnRef, reason);
            return Ok(new { success = ok });
        }
    }
}
