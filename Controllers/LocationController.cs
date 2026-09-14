using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/location")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _svc;
        public LocationController(LocationService svc) => _svc = svc;

        [HttpPost("geocode")]
        public async Task<IActionResult> Geocode([FromBody] GeocodeAddressRequestDto req)
        {
            var data = await _svc.GeocodeAddressAsync(req.Address);
            return Ok(new { success = true, data });
        }

        [HttpGet("service-area")]
        public async Task<IActionResult> ServiceArea([FromQuery] decimal lat, [FromQuery] decimal lng)
        {
            var data = await _svc.CheckServiceAreaAsync(lat, lng);
            return Ok(new { success = true, data });
        }

        /// <summary>Admin: chef density and booking volume per service city</summary>
        [HttpGet("admin/cities")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCities()
        {
            var data = await _svc.GetAdminCityAnalyticsAsync();
            return Ok(new { success = true, data });
        }
    }
}
