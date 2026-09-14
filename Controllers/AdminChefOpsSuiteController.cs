using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/chef-business-suite"), Authorize(Roles = "Admin")]
    public class ChefBusinessSuiteController : ControllerBase
    {
        private readonly ChefBusinessSuiteService _svc;
        public ChefBusinessSuiteController(ChefBusinessSuiteService svc) => _svc = svc;

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview() => Ok(new { success = true, data = await _svc.GetOverviewAsync() });
    }

    [ApiController, Route("api/chef-performance-overview"), Authorize(Roles = "Admin")]
    public class ChefPerformanceOverviewController : ControllerBase
    {
        private readonly ChefPerformanceOverviewService _svc;
        public ChefPerformanceOverviewController(ChefPerformanceOverviewService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }

    [ApiController, Route("api/equipment-listings"), Authorize(Roles = "Admin")]
    public class EquipmentListingController : ControllerBase
    {
        private readonly EquipmentCertificationService _svc;
        public EquipmentListingController(EquipmentCertificationService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetListingsAsync() });

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEquipmentListingRequestDto req) => Ok(new { success = true, data = await _svc.CreateListingAsync(req) });
    }

    [ApiController, Route("api/certification-courses"), Authorize(Roles = "Admin")]
    public class CertificationCourseController : ControllerBase
    {
        private readonly EquipmentCertificationService _svc;
        public CertificationCourseController(EquipmentCertificationService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetCoursesAsync() });
    }

    [ApiController, Route("api/corporate-manager"), Authorize(Roles = "Admin")]
    public class CorporateManagerController : ControllerBase
    {
        private readonly CorporateManagerService _svc;
        public CorporateManagerController(CorporateManagerService svc) => _svc = svc;

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients() => Ok(new { success = true, data = await _svc.GetClientsAsync() });

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans() => Ok(new { success = true, data = await _svc.GetPlansAsync() });

        [HttpPatch("clients/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequestDto req)
            => Ok(new { success = await _svc.UpdateStatusAsync(id, req.Status) });
    }

    [ApiController, Route("api/admin-dietary-social"), Authorize(Roles = "Admin")]
    public class AdminDietarySocialController : ControllerBase
    {
        private readonly AdminDietarySocialService _svc;
        public AdminDietarySocialController(AdminDietarySocialService svc) => _svc = svc;

        [HttpGet("profiles")]
        public async Task<IActionResult> GetProfiles() => Ok(new { success = true, data = await _svc.GetUserProfilesAsync() });

        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts() => Ok(new { success = true, data = await _svc.GetPostsAsync() });

        [HttpGet("follows")]
        public async Task<IActionResult> GetFollows() => Ok(new { success = true, data = await _svc.GetFollowsAsync() });
    }

    [ApiController, Route("api/admin-group-ml"), Authorize(Roles = "Admin")]
    public class AdminGroupMLController : ControllerBase
    {
        private readonly AdminGroupMLService _svc;
        public AdminGroupMLController(AdminGroupMLService svc) => _svc = svc;

        [HttpGet("group-bookings")]
        public async Task<IActionResult> GetGroupBookings() => Ok(new { success = true, data = await _svc.GetGroupBookingsAsync() });

        [HttpGet("ml-stats")]
        public async Task<IActionResult> GetMLStats() => Ok(new { success = true, data = await _svc.GetMLStatsAsync() });
    }

    [ApiController, Route("api/admin-tracking-weather-contract-menu"), Authorize(Roles = "Admin")]
    public class AdminTrackingWeatherContractMenuBuildController : ControllerBase
    {
        private readonly AdminTrackingWeatherContractMenuBuildService _svc;
        private readonly WeatherRecommendationService _weatherSvc;
        public AdminTrackingWeatherContractMenuBuildController(AdminTrackingWeatherContractMenuBuildService svc, WeatherRecommendationService weatherSvc)
        {
            _svc = svc;
            _weatherSvc = weatherSvc;
        }

        [HttpGet("tracking")]
        public async Task<IActionResult> GetTracking() => Ok(new { success = true, data = await _svc.GetTrackingAsync() });

        [HttpGet("weather-rules")]
        public async Task<IActionResult> GetWeatherRules() => Ok(new { success = true, data = await _weatherSvc.GetAllAsync() });

        [HttpGet("contracts")]
        public async Task<IActionResult> GetContracts() => Ok(new { success = true, data = await _svc.GetContractsAsync() });

        [HttpGet("menu-builds")]
        public async Task<IActionResult> GetMenuBuilds() => Ok(new { success = true, data = await _svc.GetMenuBuildsAsync() });
    }

    [ApiController, Route("api/admin-wishlist-badge-prefs-reorder"), Authorize(Roles = "Admin")]
    public class AdminWishlistBadgePreferencesReorderController : ControllerBase
    {
        private readonly AdminWishlistBadgePreferencesReorderService _svc;
        public AdminWishlistBadgePreferencesReorderController(AdminWishlistBadgePreferencesReorderService svc) => _svc = svc;

        [HttpGet("wishlist-stats")]
        public async Task<IActionResult> GetWishlistStats() => Ok(new { success = true, data = await _svc.GetWishlistStatsAsync() });

        [HttpGet("badges")]
        public async Task<IActionResult> GetBadges() => Ok(new { success = true, data = await _svc.GetBadgesAsync() });

        [HttpGet("preferences-stats")]
        public async Task<IActionResult> GetPreferencesStats() => Ok(new { success = true, data = await _svc.GetPreferencesStatsAsync() });

        [HttpGet("top-combos")]
        public async Task<IActionResult> GetTopCombos() => Ok(new { success = true, data = await _svc.GetTopCombosAsync() });
    }
}
