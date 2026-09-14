using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/search")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _svc;
        public SearchController(SearchService svc) => _svc = svc;

        [HttpGet("chefs")]
        public async Task<IActionResult> SearchChefs([FromQuery] SearchChefsRequestDto req)
        {
            var data = await _svc.SearchChefsAsync(req);
            return Ok(new { success = true, count = data.Count, data });
        }

        [HttpGet("emergency")]
        public async Task<IActionResult> SearchEmergency([FromQuery] decimal lat, [FromQuery] decimal lng, [FromQuery] decimal radiusKm = 15)
        {
            var data = await _svc.SearchEmergencyChefsAsync(lat, lng, radiusKm);
            return Ok(new { success = true, count = data.Count, data });
        }
    }
}
