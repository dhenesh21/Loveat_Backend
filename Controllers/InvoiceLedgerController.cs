using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/invoices"), Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _svc;
        public InvoiceController(InvoiceService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var data = await _svc.GetUserInvoicesAsync(UserId);
            return Ok(new { success = true, data });
        }

        [HttpPost("generate/{bookingId}")]
        public async Task<IActionResult> Generate(int bookingId)
        {
            var invoice = await _svc.GenerateInvoiceAsync(bookingId);
            return Ok(new { success = true, data = invoice });
        }

        [HttpGet("admin/tax-report")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TaxReport([FromQuery] string period = "thisMonth")
        {
            var data = await _svc.GetTaxReportAsync(period);
            return Ok(new { success = true, data });
        }
    }

    [ApiController, Route("api/ledger"), Authorize(Roles = "Admin")]
    public class LedgerController : ControllerBase
    {
        private readonly LedgerService _svc;
        public LedgerController(LedgerService svc) => _svc = svc;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] string? type, [FromQuery] int days = 30)
        {
            var data = await _svc.GetSummaryAsync(null, type, days);
            return Ok(new { success = true, data });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> UserLedger(int userId)
        {
            var data = await _svc.GetSummaryAsync(userId, null, 365);
            return Ok(new { success = true, data });
        }
    }
}
