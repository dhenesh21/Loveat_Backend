using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    // ── M67: Equipment Marketplace Service ────────────────────────
    public class EquipmentService
    {
        private readonly AppDbContext _db;
        public EquipmentService(AppDbContext db) => _db = db;

        public async Task<List<EquipmentListingDto>> GetListingsAsync(string? city = null, string? category = null, string? type = null)
        {
            var q = _db.EquipmentListings.Include(l => l.Chef).Where(l => l.IsAvailable);
            if (city != null)     q = q.Where(l => l.City == city);
            if (category != null) q = q.Where(l => l.Category == category);
            if (type != null)     q = q.Where(l => l.ListingType == type);
            var list = await q.OrderByDescending(l => l.CreatedAt).ToListAsync();
            return await MapAllAsync(list);
        }

        public async Task<List<EquipmentListingDto>> GetMyListingsAsync(int chefId)
        {
            var list = await _db.EquipmentListings.Include(l => l.Chef)
                .Where(l => l.ChefId == chefId).OrderByDescending(l => l.CreatedAt).ToListAsync();
            return await MapAllAsync(list);
        }

        public async Task<EquipmentListingDto> CreateListingAsync(int chefId, CreateListingDto dto)
        {
            var chefProfile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefId);

            var listing = new EquipmentListing
            {
                ChefId      = chefId,
                Title       = dto.Title,
                Description = dto.Description,
                Category    = dto.Category,
                Condition   = dto.Condition,
                ListingType = dto.ListingType,
                Price       = dto.Price,
                PriceUnit   = dto.PriceUnit,
                Brand       = dto.Brand,
                ImageUrls   = JsonSerializer.Serialize(dto.ImageUrls),
                // Default the listing's location to the chef's registered city — City lives on
                // ChefProfile, not User, so this needs its own lookup rather than a `.Chef.City` navigation.
                City        = chefProfile?.City,
            };
            _db.EquipmentListings.Add(listing);
            await _db.SaveChangesAsync();

            listing.Chef = await _db.Users.FindAsync(chefId);
            return Map(listing, chefProfile?.City);
        }

        public async Task<EquipmentRequestDto> RequestEquipmentAsync(int chefId, int listingId, string? message, DateTime? from, DateTime? to)
        {
            var listing = await _db.EquipmentListings.FindAsync(listingId);
            decimal? cost = null;
            if (from.HasValue && to.HasValue && listing != null)
                cost = listing.Price * (decimal)(to.Value - from.Value).TotalDays;

            var req = new EquipmentRequest
            {
                ListingId        = listingId,
                RequestedByChefId= chefId,
                FromDate         = from,
                ToDate           = to,
                TotalCost        = cost,
                Message          = message,
            };
            _db.EquipmentRequests.Add(req);
            await _db.SaveChangesAsync();
            var chef = await _db.Users.FindAsync(chefId);
            return new EquipmentRequestDto
            {
                Id=req.Id, ListingId=listingId, ListingTitle=listing?.Title??"",
                RequestedByName=chef?.FullName??"", Status=req.Status,
                FromDate=from?.ToString("MMM dd, yyyy"), ToDate=to?.ToString("MMM dd, yyyy"),
                TotalCost=cost, Message=message, CreatedAt=req.CreatedAt.ToString("MMM dd, yyyy"),
            };
        }

        public async Task<bool> UpdateRequestStatusAsync(int requestId, string status)
        {
            var req = await _db.EquipmentRequests.FindAsync(requestId);
            if (req == null) return false;
            req.Status = status;
            if (status == "Returned")
            {
                var listing = await _db.EquipmentListings.FindAsync(req.ListingId);
                if (listing != null) listing.IsAvailable = true;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Batch-fetches each listed chef's ChefProfile.City in one query rather
        /// than N+1 lookups, then maps every listing. User doesn't carry a City
        /// field (that's on ChefProfile), so this join is required — a plain
        /// `l.Chef.City` navigation would not compile.
        /// </summary>
        private async Task<List<EquipmentListingDto>> MapAllAsync(List<EquipmentListing> list)
        {
            var chefIds = list.Select(l => l.ChefId).Distinct().ToList();
            var cityMap = await _db.ChefProfiles
                .Where(p => chefIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p.City);

            return list.Select(l => Map(l, cityMap.GetValueOrDefault(l.ChefId))).ToList();
        }

        private static EquipmentListingDto Map(EquipmentListing l, string? chefCity) => new()
        {
            Id=l.Id, ChefId=l.ChefId, ChefName=l.Chef?.FullName??"", ChefCity=chefCity??"",
            Title=l.Title, Description=l.Description, Category=l.Category, Condition=l.Condition,
            ListingType=l.ListingType, Price=l.Price, PriceUnit=l.PriceUnit, Brand=l.Brand,
            IsAvailable=l.IsAvailable, City=l.City,
            ImageUrls=JsonSerializer.Deserialize<List<string>>(l.ImageUrls??"[]")?? new(),
            CreatedAt=l.CreatedAt.ToString("MMM dd, yyyy"),
        };
    }

    // ── M68: Certification Service ─────────────────────────────────
    public class CertificationService
    {
        private readonly AppDbContext _db;
        public CertificationService(AppDbContext db) => _db = db;

        public async Task<List<CertificationCourseDto>> GetCoursesAsync(int chefId)
        {
            var courses  = await _db.CertificationCourses.Where(c => c.IsActive).ToListAsync();
            var enrolled = await _db.ChefCertifications.Where(c => c.ChefId == chefId).ToListAsync();
            var enrollMap= enrolled.ToDictionary(e => e.CourseId);

            return courses.Select(c => {
                enrollMap.TryGetValue(c.Id, out var cert);
                return new CertificationCourseDto
                {
                    Id=c.Id, Title=c.Title, Description=c.Description, Category=c.Category,
                    Level=c.Level, Provider=c.Provider, DurationHours=c.DurationHours, Fee=c.Fee,
                    BadgeIconUrl=c.BadgeIconUrl, IsEnrolled=cert!=null,
                    IsCompleted=cert?.Status=="Completed", ProgressPercent=cert?.ProgressPercent??0,
                };
            }).ToList();
        }

        public async Task<List<ChefCertificationDto>> GetMyBadgesAsync(int chefId)
        {
            var certs = await _db.ChefCertifications
                .Include(c => c.Course)
                .Where(c => c.ChefId == chefId)
                .OrderByDescending(c => c.EnrolledAt)
                .ToListAsync();
            return certs.Select(MapCert).ToList();
        }

        public async Task<ChefCertificationDto> EnrollAsync(int chefId, int courseId)
        {
            var existing = await _db.ChefCertifications.FirstOrDefaultAsync(c => c.ChefId==chefId && c.CourseId==courseId);
            if (existing != null) return MapCert(existing);

            var cert = new ChefCertification { ChefId=chefId, CourseId=courseId };
            _db.ChefCertifications.Add(cert);
            await _db.SaveChangesAsync();
            cert.Course = await _db.CertificationCourses.FindAsync(courseId);
            return MapCert(cert);
        }

        public async Task<ChefCertificationDto> UpdateProgressAsync(int chefId, int courseId, decimal progress)
        {
            var cert = await _db.ChefCertifications
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ChefId==chefId && c.CourseId==courseId);
            if (cert == null) throw new InvalidOperationException("Not enrolled in this course.");

            cert.ProgressPercent = Math.Min(progress, 100);
            cert.Status          = progress >= 100 ? "Completed" : "InProgress";
            if (progress >= 100)
            {
                cert.CompletedAt    = DateTime.UtcNow;
                cert.ExpiresAt      = DateTime.UtcNow.AddYears(2);
                cert.CertificateNo  = $"LOVEAT-CERT-{chefId:D4}-{courseId:D3}-{DateTime.UtcNow:yyyyMMdd}";
            }
            await _db.SaveChangesAsync();
            return MapCert(cert);
        }

        public async Task SeedCoursesAsync()
        {
            if (await _db.CertificationCourses.AnyAsync()) return;
            _db.CertificationCourses.AddRange(
                new CertificationCourse { Title="Food Hygiene Basics",       Category="Hygiene",         Level="Beginner",     DurationHours=4,  Fee=0,   Provider="LovEat Academy", BadgeIconUrl="🧼", Description="Essential food safety and hygiene practices for home chefs." },
                new CertificationCourse { Title="Nutrition Fundamentals",     Category="NutritionBasics", Level="Beginner",     DurationHours=6,  Fee=0,   Provider="LovEat Academy", BadgeIconUrl="🥗", Description="Understanding macros, micros and meal balance." },
                new CertificationCourse { Title="South Indian Mastery",       Category="Regional",        Level="Intermediate", DurationHours=12, Fee=199, Provider="LovEat Academy", BadgeIconUrl="🍛", Description="Advanced South Indian techniques from sambhar to biryani." },
                new CertificationCourse { Title="Baking and Confectionery",   Category="Baking",          Level="Intermediate", DurationHours=10, Fee=299, Provider="LovEat Academy", BadgeIconUrl="🎂", Description="Cakes, bread, pastries and Indian sweets." },
                new CertificationCourse { Title="Advanced Event Cooking",     Category="AdvancedCooking", Level="Advanced",     DurationHours=20, Fee=499, Provider="LovEat Academy", BadgeIconUrl="⭐", Description="Large-scale cooking for weddings and corporate events." }
            );
            await _db.SaveChangesAsync();
        }

        private static ChefCertificationDto MapCert(ChefCertification c) => new()
        {
            Id=c.Id, CourseTitle=c.Course?.Title??"", Category=c.Course?.Category??"",
            Level=c.Course?.Level??"", Provider=c.Course?.Provider??"",
            Status=c.Status, ProgressPercent=c.ProgressPercent,
            CertificateNo=c.CertificateNo, CertificateUrl=c.CertificateUrl,
            BadgeIconUrl=c.Course?.BadgeIconUrl??"🏅",
            EnrolledAt=c.EnrolledAt.ToString("MMM dd, yyyy"),
            CompletedAt=c.CompletedAt?.ToString("MMM dd, yyyy"),
            ExpiresAt=c.ExpiresAt?.ToString("MMM dd, yyyy"),
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/equipment"), Authorize(Roles="Chef")]
    public class EquipmentController : ControllerBase
    {
        private readonly Services.EquipmentService _svc;
        public EquipmentController(Services.EquipmentService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? city, [FromQuery] string? category, [FromQuery] string? type)
            => Ok(new { success=true, data=await _svc.GetListingsAsync(city, category, type) });

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
            => Ok(new { success=true, data=await _svc.GetMyListingsAsync(UserId) });

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateListingDto dto)
            => Ok(new { success=true, data=await _svc.CreateListingAsync(UserId, dto) });

        [HttpPost("{id}/request")]
        public async Task<IActionResult> Request(int id, [FromBody] RequestEquipmentActionDto req)
        {
            DateTime? from = DateTime.TryParse(req.FromDate, out var f) ? f : null;
            DateTime? to   = DateTime.TryParse(req.ToDate, out var t) ? t : null;
            var data = await _svc.RequestEquipmentAsync(UserId, id, req.Message, from, to);
            return Ok(new { success=true, data, message="Equipment request sent to the owner." });
        }

        [HttpPost("requests/{id}/update")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] UpdateEquipmentRequestStatusDto req)
        {
            var ok = await _svc.UpdateRequestStatusAsync(id, req.Status);
            return Ok(new { success=ok });
        }
    }

    [ApiController, Route("api/certifications"), Authorize(Roles="Chef")]
    public class CertificationController : ControllerBase
    {
        private readonly Services.CertificationService _svc;
        public CertificationController(Services.CertificationService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
            => Ok(new { success=true, data=await _svc.GetCoursesAsync(UserId) });

        [HttpGet("my-badges")]
        public async Task<IActionResult> GetMyBadges()
            => Ok(new { success=true, data=await _svc.GetMyBadgesAsync(UserId) });

        [HttpPost("enroll/{courseId}")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var data = await _svc.EnrollAsync(UserId, courseId);
            return Ok(new { success=true, data, message="Enrolled! Start learning to earn your badge." });
        }

        [HttpPost("progress/{courseId}")]
        public async Task<IActionResult> UpdateProgress(int courseId, [FromBody] UpdateCertificationProgressDto req)
        {
            var data = await _svc.UpdateProgressAsync(UserId, courseId, req.ProgressPercent);
            return Ok(new { success=true, data });
        }

        [HttpPost("seed"), AllowAnonymous]
        public async Task<IActionResult> Seed() { await _svc.SeedCoursesAsync(); return Ok(new { success=true }); }
    }
}
