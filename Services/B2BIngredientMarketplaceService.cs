using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // M81: B2B API Service
    // ══════════════════════════════════════════════════════════════
    public class B2BService
    {
        private readonly AppDbContext _db;
        private readonly BookingService _booking;

        public B2BService(AppDbContext db, BookingService booking)
        {
            _db = db;
            _booking = booking;
        }

        public async Task<B2BPartnerDto> CreatePartnerAsync(CreateB2BPartnerRequestDto req)
        {
            var partner = new B2BPartner
            {
                CompanyName = req.CompanyName,
                ContactEmail = req.ContactEmail,
                CommissionRate = req.CommissionRate,
            };
            _db.B2BPartners.Add(partner);
            await _db.SaveChangesAsync();
            return ToDto(partner, 0);
        }

        public async Task<List<B2BPartnerDto>> GetPartnersAsync()
        {
            var partners = await _db.B2BPartners.OrderByDescending(p => p.CreatedAt).ToListAsync();
            var result = new List<B2BPartnerDto>();
            foreach (var p in partners)
            {
                var count = await _db.B2BBookingRequests.CountAsync(r => r.PartnerId == p.Id && r.Status == "Created");
                result.Add(ToDto(p, count));
            }
            return result;
        }

        /// <summary>
        /// Creates a booking on behalf of a hotel/co-living guest who has no
        /// LovEat account. A lightweight Customer account is created for the
        /// guest by phone number (matching an existing one if the same guest
        /// has booked before), then reuses BookingService.CreateBookingAsync
        /// directly rather than duplicating booking/pricing logic.
        /// </summary>
        public async Task<(bool Success, string Message, B2BBookingResultDto? Data)> CreateBookingOnBehalfAsync(CreateB2BBookingRequestDto req)
        {
            var partner = await _db.B2BPartners.FirstOrDefaultAsync(p => p.ApiKey == req.ApiKey && p.IsActive);
            if (partner == null) return (false, "Invalid or inactive API key.", null);

            var guest = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.GuestPhone);
            if (guest == null)
            {
                guest = new User
                {
                    PhoneNumber = req.GuestPhone,
                    FullName = string.IsNullOrWhiteSpace(req.GuestName) ? "Guest" : req.GuestName,
                    Role = "Customer",
                    IsPhoneVerified = true, // trusted via the partner's own guest verification, not LovEat OTP
                    IsActive = true,
                };
                _db.Users.Add(guest);
                await _db.SaveChangesAsync();
                _db.UserProfiles.Add(new UserProfile { UserId = guest.Id });
                await _db.SaveChangesAsync();
            }

            var b2bRequest = new B2BBookingRequest
            {
                PartnerId = partner.Id,
                GuestName = req.GuestName,
                GuestPhone = req.GuestPhone,
            };
            _db.B2BBookingRequests.Add(b2bRequest);
            await _db.SaveChangesAsync();

            var (success, message, bookingData) = await _booking.CreateBookingAsync(guest.Id, new CreateBookingRequestDto
            {
                ChefId = req.ChefId,
                BookingType = "Instant",
                Cuisine = req.Cuisine,
                ScheduledAt = req.ScheduledAt,
                DurationMinutes = req.DurationMinutes,
                GuestCount = req.GuestCount,
                Address = req.Address,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Notes = $"Booked via B2B partner: {partner.CompanyName}",
            });

            if (!success || bookingData == null)
            {
                b2bRequest.Status = "Failed";
                await _db.SaveChangesAsync();
                return (false, message, new B2BBookingResultDto { RequestId = b2bRequest.Id, Status = "Failed" });
            }

            b2bRequest.BookingId = bookingData.Id;
            b2bRequest.Status = "Created";
            await _db.SaveChangesAsync();

            return (true, "Booking created for guest.", new B2BBookingResultDto
            {
                RequestId = b2bRequest.Id, BookingId = bookingData.Id, Status = "Created", TotalAmount = bookingData.TotalAmount,
            });
        }

        public async Task<List<B2BBookingResultDto>> GetPartnerBookingsAsync(string apiKey)
        {
            var partner = await _db.B2BPartners.FirstOrDefaultAsync(p => p.ApiKey == apiKey && p.IsActive);
            if (partner == null) return new List<B2BBookingResultDto>();

            var requests = await _db.B2BBookingRequests.Where(r => r.PartnerId == partner.Id).OrderByDescending(r => r.CreatedAt).ToListAsync();
            var result = new List<B2BBookingResultDto>();
            foreach (var r in requests)
            {
                var booking = r.BookingId.HasValue ? await _db.Bookings.FindAsync(r.BookingId.Value) : null;
                result.Add(new B2BBookingResultDto { RequestId = r.Id, BookingId = r.BookingId, Status = r.Status, TotalAmount = booking?.TotalAmount });
            }
            return result;
        }

        private static B2BPartnerDto ToDto(B2BPartner p, int totalBookings) => new()
        {
            Id = p.Id, CompanyName = p.CompanyName, ContactEmail = p.ContactEmail,
            ApiKey = p.ApiKey, CommissionRate = p.CommissionRate, IsActive = p.IsActive, TotalBookings = totalBookings,
        };
    }

    // ══════════════════════════════════════════════════════════════
    // M82: Chef-to-Chef Ingredient Marketplace Service
    // ══════════════════════════════════════════════════════════════
    public class IngredientMarketplaceService
    {
        private readonly AppDbContext _db;
        public IngredientMarketplaceService(AppDbContext db) => _db = db;

        public async Task<List<IngredientListingDto>> GetListingsAsync(string? city = null, string? ingredientName = null, string? type = null)
        {
            var query = _db.IngredientListings.Include(l => l.Chef).Where(l => l.IsAvailable);
            if (!string.IsNullOrWhiteSpace(city)) query = query.Where(l => l.City == city);
            if (!string.IsNullOrWhiteSpace(ingredientName)) query = query.Where(l => l.IngredientName.Contains(ingredientName));
            if (!string.IsNullOrWhiteSpace(type)) query = query.Where(l => l.ListingType == type);

            var list = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
            return await MapAllAsync(list);
        }

        public async Task<List<IngredientListingDto>> GetMyListingsAsync(int chefId)
        {
            var list = await _db.IngredientListings.Include(l => l.Chef)
                .Where(l => l.ChefId == chefId).OrderByDescending(l => l.CreatedAt).ToListAsync();
            return await MapAllAsync(list);
        }

        public async Task<IngredientListingDto> CreateListingAsync(int chefId, CreateIngredientListingRequestDto req)
        {
            // Same fix as batch 34's EquipmentService: City lives on ChefProfile, not User —
            // needs its own lookup rather than a `.Chef.City` navigation that wouldn't compile.
            var chefProfile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefId);

            var listing = new IngredientListing
            {
                ChefId = chefId,
                IngredientName = req.IngredientName,
                Quantity = req.Quantity,
                Unit = req.Unit,
                Price = req.Price,
                ListingType = req.ListingType,
                Description = req.Description,
                City = chefProfile?.City,
            };
            _db.IngredientListings.Add(listing);
            await _db.SaveChangesAsync();

            listing.Chef = await _db.Users.FindAsync(chefId);
            return Map(listing, chefProfile?.City);
        }

        public async Task<(bool Success, string Message, IngredientSwapRequestDto? Data)> RequestSwapAsync(int chefId, int listingId, RequestIngredientSwapRequestDto req)
        {
            var listing = await _db.IngredientListings.FirstOrDefaultAsync(l => l.Id == listingId && l.IsAvailable);
            if (listing == null) return (false, "Listing not found or no longer available.", null);
            if (listing.ChefId == chefId) return (false, "You can't request your own listing.", null);

            var swapReq = new IngredientSwapRequest
            {
                ListingId = listingId,
                RequestedByChefId = chefId,
                OfferedItem = req.OfferedItem,
                Message = req.Message,
            };
            _db.IngredientSwapRequests.Add(swapReq);
            await _db.SaveChangesAsync();

            var requester = await _db.Users.FindAsync(chefId);
            return (true, "Request sent.", new IngredientSwapRequestDto
            {
                Id = swapReq.Id, ListingId = listingId, IngredientName = listing.IngredientName,
                RequestedByName = requester?.FullName ?? "", OfferedItem = req.OfferedItem, Status = swapReq.Status, Message = req.Message,
            });
        }

        public async Task<(bool Success, string Message)> UpdateSwapStatusAsync(int chefId, int requestId, string status)
        {
            var swapReq = await _db.IngredientSwapRequests.Include(r => r.Listing).FirstOrDefaultAsync(r => r.Id == requestId);
            if (swapReq == null) return (false, "Request not found.");
            if (swapReq.Listing == null || swapReq.Listing.ChefId != chefId) return (false, "Only the listing owner can update this request.");

            swapReq.Status = status;
            if (status == "Completed" && swapReq.Listing != null) swapReq.Listing.IsAvailable = false;
            await _db.SaveChangesAsync();

            return (true, $"Request marked {status}.");
        }

        public async Task<List<IngredientSwapRequestDto>> GetRequestsForMyListingsAsync(int chefId)
        {
            var requests = await _db.IngredientSwapRequests.Include(r => r.Listing).Include(r => r.RequestedBy)
                .Where(r => r.Listing != null && r.Listing.ChefId == chefId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(r => new IngredientSwapRequestDto
            {
                Id = r.Id, ListingId = r.ListingId, IngredientName = r.Listing?.IngredientName ?? "",
                RequestedByName = r.RequestedBy?.FullName ?? "", OfferedItem = r.OfferedItem, Status = r.Status, Message = r.Message,
            }).ToList();
        }

        private async Task<List<IngredientListingDto>> MapAllAsync(List<IngredientListing> list)
        {
            var chefIds = list.Select(l => l.ChefId).Distinct().ToList();
            var cityMap = await _db.ChefProfiles.Where(p => chefIds.Contains(p.UserId)).ToDictionaryAsync(p => p.UserId, p => p.City);
            return list.Select(l => Map(l, cityMap.GetValueOrDefault(l.ChefId))).ToList();
        }

        private static IngredientListingDto Map(IngredientListing l, string? chefCity) => new()
        {
            Id = l.Id, ChefId = l.ChefId, ChefName = l.Chef?.FullName ?? "", ChefCity = chefCity,
            IngredientName = l.IngredientName, Quantity = l.Quantity, Unit = l.Unit, Price = l.Price,
            ListingType = l.ListingType, Description = l.Description, IsAvailable = l.IsAvailable,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/b2b")]
    public class B2BController : ControllerBase
    {
        private readonly Services.B2BService _svc;
        public B2BController(Services.B2BService svc) => _svc = svc;

        [HttpPost("admin/partners")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePartner([FromBody] CreateB2BPartnerRequestDto req)
        {
            var data = await _svc.CreatePartnerAsync(req);
            return Ok(new { success = true, data });
        }

        [HttpGet("admin/partners")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPartners()
        {
            var data = await _svc.GetPartnersAsync();
            return Ok(new { success = true, data });
        }

        /// <summary>Partner-facing endpoint — authenticated via API key in the body, not a LovEat user JWT, since the calling system is a hotel/co-living partner's own backend, not a logged-in LovEat user.</summary>
        [HttpPost("bookings")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateBooking([FromBody] CreateB2BBookingRequestDto req)
        {
            var (success, message, data) = await _svc.CreateBookingOnBehalfAsync(req);
            if (!success) return BadRequest(new { success, message, data });
            return Ok(new { success, message, data });
        }

        [HttpGet("bookings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookings([FromQuery] string apiKey)
        {
            var data = await _svc.GetPartnerBookingsAsync(apiKey);
            return Ok(new { success = true, data });
        }
    }

    [ApiController]
    [Route("api/ingredient-marketplace")]
    [Authorize(Roles = "Chef")]
    public class IngredientMarketplaceController : ControllerBase
    {
        private readonly Services.IngredientMarketplaceService _svc;
        public IngredientMarketplaceController(Services.IngredientMarketplaceService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? city, [FromQuery] string? ingredientName, [FromQuery] string? type)
            => Ok(new { success = true, data = await _svc.GetListingsAsync(city, ingredientName, type) });

        [HttpGet("my")]
        public async Task<IActionResult> GetMy() => Ok(new { success = true, data = await _svc.GetMyListingsAsync(UserId) });

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIngredientListingRequestDto req)
            => Ok(new { success = true, data = await _svc.CreateListingAsync(UserId, req) });

        [HttpPost("{id}/request")]
        public async Task<IActionResult> Request(int id, [FromBody] RequestIngredientSwapRequestDto req)
        {
            var (success, message, data) = await _svc.RequestSwapAsync(UserId, id, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpPost("requests/{id}/update")]
        public async Task<IActionResult> UpdateRequest(int id, [FromQuery] string status)
        {
            var (success, message) = await _svc.UpdateSwapStatusAsync(UserId, id, status);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("requests/mine")]
        public async Task<IActionResult> GetRequestsForMyListings()
            => Ok(new { success = true, data = await _svc.GetRequestsForMyListingsAsync(UserId) });
    }
}
