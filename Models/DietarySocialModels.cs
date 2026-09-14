using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M65: AI Dietary Matching ───────────────────────────────────
    public class UserDietaryProfile
    {
        [Key] public int Id { get; set; }
        public int    UserId           { get; set; }
        public User?  User             { get; set; }
        public string DietType         { get; set; } = ""; // Vegetarian/Vegan/NonVeg/Eggetarian/Jain
        public string Allergies        { get; set; } = "[]"; // JSON: ["Nuts","Gluten","Dairy"]
        public string HealthGoals      { get; set; } = "[]"; // JSON: ["WeightLoss","Diabetes","HighProtein"]
        public string CuisinePreferences{ get; set; } = "[]"; // JSON: ["SouthIndian","Chinese"]
        public string SpiceLevel       { get; set; } = "Medium"; // Mild/Medium/Spicy/ExtraSpicy
        public string? MedicalConditions{ get; set; } = "[]"; // JSON
        public decimal? CalorieTarget  { get; set; }
        public bool   IsActive         { get; set; } = true;
        public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt      { get; set; } = DateTime.UtcNow;
    }

    public class DietaryMatchScore
    {
        [Key] public int Id { get; set; }
        public int    UserId       { get; set; }
        public int    ChefId       { get; set; }
        public User?  Chef         { get; set; }
        public decimal MatchScore  { get; set; } // 0-100
        public string  MatchReasons{ get; set; } = "[]"; // JSON
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M66: Social Features ──────────────────────────────────────
    public class ChefFollow
    {
        [Key] public int Id { get; set; }
        public int    FollowerId   { get; set; }
        public User?  Follower     { get; set; }
        public int    ChefId       { get; set; }
        public User?  Chef         { get; set; }
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }

    public class MealPost
    {
        [Key] public int Id { get; set; }
        public int    ChefId       { get; set; }
        public User?  Chef         { get; set; }
        public int?   BookingId    { get; set; }
        public string Caption      { get; set; } = "";
        public string ImageUrls    { get; set; } = "[]"; // JSON array
        public string Tags         { get; set; } = "[]"; // JSON: ["BirthDay","Wedding","SouthIndian"]
        public string Cuisine      { get; set; } = "";
        public int    LikeCount    { get; set; } = 0;
        public int    CommentCount { get; set; } = 0;
        public bool   IsPublic     { get; set; } = true;
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

        // ── Navigation ──
        public ICollection<MealPostComment> Comments { get; set; } = new List<MealPostComment>();
    }

    public class MealPostLike
    {
        [Key] public int Id { get; set; }
        public int    PostId    { get; set; }
        public MealPost? Post   { get; set; }
        public int    UserId    { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }

    public class MealPostComment
    {
        [Key] public int Id { get; set; }
        public int    PostId     { get; set; }
        public MealPost? Post    { get; set; }
        public int    UserId     { get; set; }
        public User?  User       { get; set; }
        public string Comment    { get; set; } = "";
        public DateTime CreatedAt{ get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // M65 DTOs
    public class UserDietaryProfileDto
    {
        public int     Id                  { get; set; }
        public string  DietType            { get; set; } = "";
        public List<string> Allergies      { get; set; } = new();
        public List<string> HealthGoals    { get; set; } = new();
        public List<string> CuisinePreferences { get; set; } = new();
        public string  SpiceLevel          { get; set; } = "";
        public List<string> MedicalConditions { get; set; } = new();
        public decimal? CalorieTarget      { get; set; }
    }

    public class DietaryMatchDto
    {
        public int     ChefId      { get; set; }
        public string  ChefName    { get; set; } = "";
        public string  City        { get; set; } = "";
        public decimal MatchScore  { get; set; }
        public string  MatchGrade  { get; set; } = ""; // Excellent/Good/Fair
        public List<string> MatchReasons { get; set; } = new();
        public List<string> Cuisines     { get; set; } = new();
        public decimal AvgRating  { get; set; }
        public int     TotalBookings{ get; set; }
    }

    // M66 DTOs
    public class MealPostDto
    {
        public int     Id           { get; set; }
        public int     ChefId       { get; set; }
        public string  ChefName     { get; set; } = "";
        public string  ChefCity     { get; set; } = "";
        public int?    BookingId    { get; set; }
        public string  Caption      { get; set; } = "";
        public List<string> ImageUrls { get; set; } = new();
        public List<string> Tags    { get; set; } = new();
        public string  Cuisine      { get; set; } = "";
        public int     LikeCount    { get; set; }
        public int     CommentCount { get; set; }
        public bool    IsLikedByMe  { get; set; }
        public bool    IsFollowingChef { get; set; }
        public string  CreatedAt    { get; set; } = "";
        public List<MealPostCommentDto> Comments { get; set; } = new();
    }

    public class MealPostCommentDto
    {
        public int    Id        { get; set; }
        public string UserName  { get; set; } = "";
        public string Comment   { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }

    public class CreateMealPostDto
    {
        public string  Caption   { get; set; } = "";
        public List<string> ImageUrls { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string  Cuisine   { get; set; } = "";
        public int?    BookingId { get; set; }
        public bool    IsPublic  { get; set; } = true;
    }

    public class SocialFeedDto
    {
        public List<MealPostDto> Posts          { get; set; } = new();
        public List<MealPostDto> Following      { get; set; } = new();
        public List<MealPostDto> Trending       { get; set; } = new();
        public int     FollowingCount           { get; set; }
        public int     TotalPostsInFeed         { get; set; }
    }
}
