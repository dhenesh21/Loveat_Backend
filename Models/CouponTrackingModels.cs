using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    public class Coupon
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code         { get; set; } = "";
        public string Description  { get; set; } = "";
        public string DiscountType { get; set; } = "Percent"; // Percent / Flat
        public decimal DiscountValue { get; set; }            // 20 = 20% or ₹20
        public decimal MinOrderAmount{ get; set; } = 0;
        public decimal MaxDiscount   { get; set; } = 0;       // cap for percent coupons
        public int  UsageLimit     { get; set; } = 0;         // 0 = unlimited
        public int  UsedCount      { get; set; } = 0;
        public int? UsageLimitPerUser { get; set; } = 1;
        public string AppliesTo    { get; set; } = "All";     // All / NewUser / BookingType
        public string? TargetValue { get; set; }
        public bool IsActive       { get; set; } = true;
        public DateTime ValidFrom  { get; set; } = DateTime.UtcNow;
        public DateTime ValidTo    { get; set; } = DateTime.UtcNow.AddDays(30);
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

        public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
    }

    public class CouponUsage
    {
        [Key] public int Id { get; set; }
        public int CouponId  { get; set; }
        public Coupon? Coupon{ get; set; }
        public int UserId    { get; set; }
        public User? User    { get; set; }
        public int? BookingId{ get; set; }
        public decimal DiscountApplied { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }

    // Module 24 - Live Order Tracking
    public class BookingTracking
    {
        [Key] public int Id { get; set; }
        public int BookingId  { get; set; }
        public int ChefId     { get; set; }
        public decimal ChefLat{ get; set; }
        public decimal ChefLng{ get; set; }
        public string Status  { get; set; } = "Accepted"; // Accepted/OnTheWay/Arrived/Cooking/Done
        public string? ETA    { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // Coupon DTOs
    public class CouponDto
    {
        public int     Id            { get; set; }
        public string  Code          { get; set; } = "";
        public string  Description   { get; set; } = "";
        public string  DiscountType  { get; set; } = "";
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount{ get; set; }
        public decimal MaxDiscount   { get; set; }
        public int     UsageLimit    { get; set; }
        public int     UsedCount     { get; set; }
        public string  AppliesTo     { get; set; } = "";
        public bool    IsActive      { get; set; }
        public string  ValidFrom     { get; set; } = "";
        public string  ValidTo       { get; set; } = "";
        public bool    IsExpired     { get; set; }
        public bool    IsExhausted   { get; set; }
    }

    public class CreateCouponDto
    {
        public string  Code          { get; set; } = "";
        public string  Description   { get; set; } = "";
        public string  DiscountType  { get; set; } = "Percent";
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount{ get; set; } = 0;
        public decimal MaxDiscount   { get; set; } = 0;
        public int     UsageLimit    { get; set; } = 0;
        public int?    UsageLimitPerUser { get; set; } = 1;
        public string  AppliesTo     { get; set; } = "All";
        public string? TargetValue   { get; set; }
        public DateTime ValidFrom    { get; set; }
        public DateTime ValidTo      { get; set; }
    }

    public class ApplyCouponRequestDto
    {
        public string  Code        { get; set; } = "";
        public decimal OrderAmount { get; set; }
        public string? BookingType { get; set; }
    }

    public class ApplyCouponResponseDto
    {
        public bool    Success         { get; set; }
        public string  Message         { get; set; } = "";
        public decimal DiscountAmount  { get; set; }
        public decimal FinalAmount     { get; set; }
        public string  Code            { get; set; } = "";
    }

    // Tracking DTOs
    public class BookingTrackingDto
    {
        public int     BookingId   { get; set; }
        public string  Status      { get; set; } = "";
        public decimal ChefLat     { get; set; }
        public decimal ChefLng     { get; set; }
        public string? ETA         { get; set; }
        public string  ChefName    { get; set; } = "";
        public string  ChefPhone   { get; set; } = "";
        public string  UpdatedAt   { get; set; } = "";
    }

    public class UpdateTrackingDto
    {
        public int     BookingId { get; set; }
        public string  Status    { get; set; } = "";
        public decimal Lat       { get; set; }
        public decimal Lng       { get; set; }
        public string? ETA       { get; set; }
    }

    // M16 - Rate Customer DTO
    public class RateCustomerDto
    {
        public int    BookingId  { get; set; }
        public int    CustomerId { get; set; }
        public int    Rating     { get; set; }  // 1–5
        public string Comment    { get; set; } = "";
    }
}
