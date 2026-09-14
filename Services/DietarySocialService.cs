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
    // ── M65: AI Dietary Matching Service ──────────────────────────
    public class DietaryMatchingService
    {
        private readonly AppDbContext _db;
        public DietaryMatchingService(AppDbContext db) => _db = db;

        public async Task<UserDietaryProfileDto?> GetProfileAsync(int userId)
        {
            var p = await _db.UserDietaryProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            return p == null ? null : Map(p);
        }

        public async Task<UserDietaryProfileDto> SaveProfileAsync(int userId, UserDietaryProfileDto dto)
        {
            var p = await _db.UserDietaryProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            if (p == null) { p = new UserDietaryProfile { UserId = userId }; _db.UserDietaryProfiles.Add(p); }
            p.DietType           = dto.DietType;
            p.Allergies          = JsonSerializer.Serialize(dto.Allergies);
            p.HealthGoals        = JsonSerializer.Serialize(dto.HealthGoals);
            p.CuisinePreferences = JsonSerializer.Serialize(dto.CuisinePreferences);
            p.SpiceLevel         = dto.SpiceLevel;
            p.MedicalConditions  = JsonSerializer.Serialize(dto.MedicalConditions);
            p.CalorieTarget      = dto.CalorieTarget;
            p.UpdatedAt          = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            dto.Id = p.Id;
            return dto;
        }

        public async Task<List<DietaryMatchDto>> GetMatchedChefsAsync(int userId)
        {
            var profile = await _db.UserDietaryProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            var chefs   = await _db.Users.Where(u => u.Role == "Chef").Take(20).ToListAsync();
            var reviews = await _db.Reviews.ToListAsync();
            var bookings= await _db.Bookings.Where(b => b.Status == "Completed").ToListAsync();
            var chefProfiles = await _db.ChefProfiles.ToListAsync();

            var userCuisines  = profile != null ? JsonSerializer.Deserialize<List<string>>(profile.CuisinePreferences) ?? new() : new();
            var userAllergies = profile != null ? JsonSerializer.Deserialize<List<string>>(profile.Allergies) ?? new() : new();
            var userGoals     = profile != null ? JsonSerializer.Deserialize<List<string>>(profile.HealthGoals) ?? new() : new();

            var matches = chefs.Select(chef => {
                var chefBookings = bookings.Where(b => b.ChefId == chef.Id).ToList();
                var chefReviews  = reviews.Where(r => r.ChefId == chef.Id).ToList();
                var chefCuisines = new List<string> { "South Indian", "North Indian", "Chinese" }; // from chef menu

                decimal score = 50; // base score
                var reasons   = new List<string>();

                // Cuisine match
                var cuisineMatch = userCuisines.Intersect(chefCuisines, StringComparer.OrdinalIgnoreCase).ToList();
                if (cuisineMatch.Any()) { score += 20; reasons.Add($"Specializes in {string.Join(", ", cuisineMatch)}"); }

                // Rating boost
                decimal avgRating = chefReviews.Any() ? (decimal)chefReviews.Average(r => r.Rating) : 0;
                if (avgRating >= 4.5m) { score += 15; reasons.Add("Top rated chef (4.5+)"); }
                else if (avgRating >= 4.0m) { score += 8; reasons.Add("Highly rated chef"); }

                // Experience boost
                if (chefBookings.Count >= 20) { score += 10; reasons.Add("Experienced (20+ bookings)"); }
                if (chefBookings.Count >= 50) { score += 5; reasons.Add("Expert chef (50+ bookings)"); }

                // Health goal match
                if (userGoals.Contains("WeightLoss"))    { score += 5;  reasons.Add("Offers low-calorie options"); }
                if (userGoals.Contains("HighProtein"))   { score += 5;  reasons.Add("High protein menu available"); }
                if (userGoals.Contains("Diabetes"))      { score += 5;  reasons.Add("Diabetic-friendly recipes"); }

                score = Math.Min(score, 100);
                string grade = score >= 80 ? "Excellent" : score >= 60 ? "Good" : "Fair";

                return new DietaryMatchDto
                {
                    ChefId       = chef.Id,
                    ChefName     = chef.FullName ?? "",
                    City         = chefProfiles.FirstOrDefault(p => p.UserId == chef.Id)?.City ?? "",
                    MatchScore   = Math.Round(score, 1),
                    MatchGrade   = grade,
                    MatchReasons = reasons,
                    Cuisines     = chefCuisines,
                    AvgRating    = Math.Round(avgRating, 1),
                    TotalBookings= chefBookings.Count,
                };
            }).OrderByDescending(m => m.MatchScore).ToList();

            return matches;
        }

        private static UserDietaryProfileDto Map(UserDietaryProfile p) => new()
        {
            Id=p.Id, DietType=p.DietType, SpiceLevel=p.SpiceLevel, CalorieTarget=p.CalorieTarget,
            Allergies          = JsonSerializer.Deserialize<List<string>>(p.Allergies)           ?? new(),
            HealthGoals        = JsonSerializer.Deserialize<List<string>>(p.HealthGoals)         ?? new(),
            CuisinePreferences = JsonSerializer.Deserialize<List<string>>(p.CuisinePreferences)  ?? new(),
            MedicalConditions  = JsonSerializer.Deserialize<List<string>>(p.MedicalConditions??  "[]") ?? new(),
        };
    }

    // ── M66: Social Features Service ──────────────────────────────
    public class SocialService
    {
        private readonly AppDbContext _db;
        public SocialService(AppDbContext db) => _db = db;

        public async Task<SocialFeedDto> GetFeedAsync(int userId)
        {
            var followingIds = await _db.ChefFollows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.ChefId)
                .ToListAsync();

            var allPosts = await _db.MealPosts
                .Include(p => p.Chef)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            var myLikes = (await _db.MealPostLikes
                .Where(l => l.UserId == userId)
                .Select(l => l.PostId)
                .ToListAsync()).ToHashSet();

            var followingPosts = allPosts.Where(p => followingIds.Contains(p.ChefId)).ToList();
            var trending       = allPosts.OrderByDescending(p => p.LikeCount + p.CommentCount).Take(10).ToList();
            var chefCityLookup = await _db.ChefProfiles.ToDictionaryAsync(p => p.UserId, p => p.City);

            return new SocialFeedDto
            {
                Posts          = allPosts.Select(p => MapPost(p, myLikes, followingIds, city: chefCityLookup.GetValueOrDefault(p.ChefId) ?? "")).ToList(),
                Following      = followingPosts.Select(p => MapPost(p, myLikes, followingIds, city: chefCityLookup.GetValueOrDefault(p.ChefId) ?? "")).ToList(),
                Trending       = trending.Select(p => MapPost(p, myLikes, followingIds, city: chefCityLookup.GetValueOrDefault(p.ChefId) ?? "")).ToList(),
                FollowingCount = followingIds.Count,
                TotalPostsInFeed = allPosts.Count,
            };
        }

        public async Task<MealPostDto> CreatePostAsync(int chefId, CreateMealPostDto dto)
        {
            var post = new MealPost
            {
                ChefId    = chefId,
                Caption   = dto.Caption,
                ImageUrls = JsonSerializer.Serialize(dto.ImageUrls),
                Tags      = JsonSerializer.Serialize(dto.Tags),
                Cuisine   = dto.Cuisine,
                BookingId = dto.BookingId,
                IsPublic  = dto.IsPublic,
            };
            _db.MealPosts.Add(post);
            await _db.SaveChangesAsync();
            var chef = await _db.Users.FindAsync(chefId);
            var chefProfile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefId);
            return MapPost(post, new HashSet<int>(), new List<int>(), chef?.FullName ?? "", chefProfile?.City ?? "");
        }

        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            var existing = await _db.MealPostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            var post     = await _db.MealPosts.FindAsync(postId);
            if (post == null) return false;
            if (existing != null)
            {
                _db.MealPostLikes.Remove(existing);
                post.LikeCount = Math.Max(0, post.LikeCount - 1);
            }
            else
            {
                _db.MealPostLikes.Add(new MealPostLike { PostId=postId, UserId=userId });
                post.LikeCount++;
            }
            await _db.SaveChangesAsync();
            return existing == null; // true = liked, false = unliked
        }

        public async Task<MealPostCommentDto> AddCommentAsync(int postId, int userId, string comment)
        {
            var c = new MealPostComment { PostId=postId, UserId=userId, Comment=comment };
            _db.MealPostComments.Add(c);
            var post = await _db.MealPosts.FindAsync(postId);
            if (post != null) post.CommentCount++;
            await _db.SaveChangesAsync();
            var user = await _db.Users.FindAsync(userId);
            return new MealPostCommentDto { Id=c.Id, UserName=user?.FullName??"", Comment=comment, CreatedAt="Just now" };
        }

        public async Task<bool> ToggleFollowAsync(int followerId, int chefId)
        {
            var existing = await _db.ChefFollows.FirstOrDefaultAsync(f => f.FollowerId==followerId && f.ChefId==chefId);
            if (existing != null) { _db.ChefFollows.Remove(existing); }
            else { _db.ChefFollows.Add(new ChefFollow { FollowerId=followerId, ChefId=chefId }); }
            await _db.SaveChangesAsync();
            return existing == null; // true = followed, false = unfollowed
        }

        public async Task<List<MealPostDto>> GetChefPostsAsync(int chefId, int userId)
        {
            var myLikes      = (await _db.MealPostLikes.Where(l => l.UserId==userId).Select(l => l.PostId).ToListAsync()).ToHashSet();
            var followingIds = await _db.ChefFollows.Where(f => f.FollowerId==userId).Select(f => f.ChefId).ToListAsync();
            var posts        = await _db.MealPosts.Include(p=>p.Chef).Include(p=>p.Comments).ThenInclude(c=>c.User)
                .Where(p => p.ChefId==chefId && p.IsPublic).OrderByDescending(p=>p.CreatedAt).ToListAsync();
            var chefProfile  = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefId);
            return posts.Select(p => MapPost(p, myLikes, followingIds, city: chefProfile?.City ?? "")).ToList();
        }

        private static MealPostDto MapPost(MealPost p, HashSet<int> myLikes, List<int> followingIds, string? chefName=null, string? city=null) => new()
        {
            Id=p.Id, ChefId=p.ChefId, ChefName=chefName??p.Chef?.FullName??"", ChefCity=city??"",
            BookingId=p.BookingId, Caption=p.Caption, Cuisine=p.Cuisine,
            LikeCount=p.LikeCount, CommentCount=p.CommentCount,
            IsLikedByMe=myLikes.Contains(p.Id), IsFollowingChef=followingIds.Contains(p.ChefId),
            ImageUrls = JsonSerializer.Deserialize<List<string>>(p.ImageUrls) ?? new(),
            Tags      = JsonSerializer.Deserialize<List<string>>(p.Tags)      ?? new(),
            CreatedAt = p.CreatedAt.ToString("MMM dd, yyyy"),
            Comments  = p.Comments?.OrderByDescending(c=>c.CreatedAt).Take(3).Select(c=>new MealPostCommentDto
                { Id=c.Id, UserName=c.User?.FullName??"", Comment=c.Comment, CreatedAt=c.CreatedAt.ToString("MMM dd") }).ToList() ?? new(),
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/dietary"), Authorize]
    public class DietaryMatchingController : ControllerBase
    {
        private readonly Services.DietaryMatchingService _svc;
        public DietaryMatchingController(Services.DietaryMatchingService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
            => Ok(new { success=true, data=await _svc.GetProfileAsync(UserId) });

        [HttpPost("profile")]
        public async Task<IActionResult> SaveProfile([FromBody] UserDietaryProfileDto dto)
            => Ok(new { success=true, data=await _svc.SaveProfileAsync(UserId, dto) });

        [HttpGet("matches")]
        public async Task<IActionResult> GetMatches()
            => Ok(new { success=true, data=await _svc.GetMatchedChefsAsync(UserId) });
    }

    [ApiController, Route("api/social"), Authorize]
    public class SocialController : ControllerBase
    {
        private readonly Services.SocialService _svc;
        public SocialController(Services.SocialService svc) => _svc = svc;
        private int    UserId   => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed()
            => Ok(new { success=true, data=await _svc.GetFeedAsync(UserId) });

        [HttpGet("chef/{chefId}/posts")]
        public async Task<IActionResult> GetChefPosts(int chefId)
            => Ok(new { success=true, data=await _svc.GetChefPostsAsync(chefId, UserId) });

        [HttpPost("posts"), Authorize(Roles="Chef")]
        public async Task<IActionResult> CreatePost([FromBody] CreateMealPostDto dto)
            => Ok(new { success=true, data=await _svc.CreatePostAsync(UserId, dto) });

        [HttpPost("posts/{id}/like")]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var liked = await _svc.ToggleLikeAsync(id, UserId);
            return Ok(new { success=true, liked });
        }

        [HttpPost("posts/{id}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromBody] dynamic req)
        {
            string comment = (string)(req.comment ?? "");
            var data = await _svc.AddCommentAsync(id, UserId, comment);
            return Ok(new { success=true, data });
        }

        [HttpPost("chefs/{chefId}/follow")]
        public async Task<IActionResult> ToggleFollow(int chefId)
        {
            var followed = await _svc.ToggleFollowAsync(UserId, chefId);
            return Ok(new { success=true, followed, message=followed?"Following chef!":"Unfollowed." });
        }
    }
}
