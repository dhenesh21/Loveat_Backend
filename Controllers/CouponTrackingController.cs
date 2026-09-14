using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.DTOs;
using LovEat.API.Services;
using LovEat.API.Data;
using LovEat.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LovEat.API.Controllers
{
    // ── M20: Coupon Controller ─────────────────────────────────────
    [ApiController]
    [Route("api/coupons")]
    public class CouponController : ControllerBase
    {
        private readonly CouponService _svc;
        public CouponController(CouponService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var data = await _svc.GetActiveCouponsAsync();
            return Ok(new { success=true, data });
        }

        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> Apply([FromBody] ApplyCouponRequestDto req)
        {
            var result = await _svc.ApplyAsync(UserId, req);
            return Ok(new { success=result.Success, data=result, message=result.Message });
        }

        [HttpGet("admin/all")]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> AdminAll()
        {
            var data = await _svc.GetAllAdminAsync();
            return Ok(new { success=true, data });
        }

        [HttpPost("admin/create")]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            var coupon = await _svc.CreateAsync(dto);
            return Ok(new { success=true, data=coupon });
        }

        [HttpPost("admin/{id}/toggle")]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Toggle(int id)
        {
            var ok = await _svc.ToggleAsync(id);
            return Ok(new { success=ok });
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return Ok(new { success=ok });
        }
    }

    // ── M24: Tracking Controller ───────────────────────────────────
    [ApiController]
    [Route("api/tracking")]
    [Authorize]
    public class TrackingController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TrackingController(AppDbContext db) => _db = db;

        /// <summary>Customer: Get live tracking for booking</summary>
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetTracking(int bookingId)
        {
            var tracking = await _db.BookingTrackings
                .FirstOrDefaultAsync(t => t.BookingId == bookingId);
            if (tracking == null)
                return Ok(new { success=false, message="Tracking not started yet" });

            var chef = await _db.Users.FindAsync(tracking.ChefId);
            return Ok(new { success=true, data=new BookingTrackingDto
            {
                BookingId  = tracking.BookingId,
                Status     = tracking.Status,
                ChefLat    = tracking.ChefLat,
                ChefLng    = tracking.ChefLng,
                ETA        = tracking.ETA,
                ChefName   = chef?.FullName ?? "",
                ChefPhone  = chef?.PhoneNumber ?? "",
                UpdatedAt  = tracking.UpdatedAt.ToString("hh:mm tt"),
            }});
        }

        /// <summary>Chef: Update live location + status</summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateTracking([FromBody] UpdateTrackingDto dto)
        {
            var existing = await _db.BookingTrackings
                .FirstOrDefaultAsync(t => t.BookingId == dto.BookingId);

            if (existing == null)
            {
                var chefId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                _db.BookingTrackings.Add(new BookingTracking
                {
                    BookingId  = dto.BookingId,
                    ChefId     = chefId,
                    Status     = dto.Status,
                    ChefLat    = dto.Lat,
                    ChefLng    = dto.Lng,
                    ETA        = dto.ETA,
                    UpdatedAt  = DateTime.UtcNow,
                });
            }
            else
            {
                existing.Status    = dto.Status;
                existing.ChefLat   = dto.Lat;
                existing.ChefLng   = dto.Lng;
                existing.ETA       = dto.ETA;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
            return Ok(new { success=true });
        }
    }

    // ── M16: Rate Customer Controller ──────────────────────────────
    [ApiController]
    [Route("api/reviews")]
    [Authorize]
    public class CustomerRatingController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CustomerRatingController(AppDbContext db) => _db = db;

        /// <summary>Chef: Rate a customer after completed booking</summary>
        [HttpPost("rate-customer")]
        [Authorize(Roles="Chef")]
        public async Task<IActionResult> RateCustomer([FromBody] RateCustomerDto dto)
        {
            // Verify booking belongs to this chef
            var chefId  = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == dto.BookingId && b.ChefId == chefId && b.Status == "Completed");
            if (booking == null)
                return BadRequest(new { success=false, message="Booking not found or not completed" });

            var existing = await _db.CustomerRatings.FirstOrDefaultAsync(r => r.BookingId == dto.BookingId);
            if (existing != null)
                return BadRequest(new { success=false, message="You've already rated this customer for this booking." });

            _db.CustomerRatings.Add(new CustomerRating
            {
                BookingId = dto.BookingId,
                ChefId = chefId,
                CustomerId = booking.CustomerId,
                Rating = dto.Rating,
                Comment = dto.Comment,
            });
            await _db.SaveChangesAsync();

            return Ok(new { success=true, message="Customer rated successfully", data=dto });
        }
    }
}
