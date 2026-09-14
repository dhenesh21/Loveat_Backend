using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly WalletService _svc;
        public WalletController(WalletService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _svc.GetWalletAsync(UserId);
            if (data == null) return NotFound(new { success = false, message = "User not found." });
            return Ok(new { success = true, data });
        }

        /// <summary>Dev/test top-up only — see WalletService.TopUpAsync TODO for the real-money path once G1 is wired.</summary>
        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpWalletRequestDto req)
        {
            var (success, message) = await _svc.TopUpAsync(UserId, req.Amount);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
