using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class CouponService
    {
        private readonly AppDbContext _db;
        public CouponService(AppDbContext db) => _db = db;

        public async Task<List<CouponDto>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            var coupons = await _db.Coupons
                .Where(c => c.IsActive && c.ValidTo >= now)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return coupons.Select(Map).ToList();
        }

        public async Task<List<CouponDto>> GetAllAdminAsync()
        {
            var coupons = await _db.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return coupons.Select(Map).ToList();
        }

        public async Task<ApplyCouponResponseDto> ApplyAsync(int userId, ApplyCouponRequestDto req)
        {
            var now    = DateTime.UtcNow;
            var coupon = await _db.Coupons
                .Include(c => c.Usages)
                .FirstOrDefaultAsync(c => c.Code == req.Code.ToUpper() && c.IsActive);

            if (coupon == null)
                return new ApplyCouponResponseDto { Success=false, Message="Invalid coupon code" };
            if (coupon.ValidTo < now)
                return new ApplyCouponResponseDto { Success=false, Message="Coupon has expired" };
            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return new ApplyCouponResponseDto { Success=false, Message="Coupon usage limit reached" };
            if (req.OrderAmount < coupon.MinOrderAmount)
                return new ApplyCouponResponseDto { Success=false, Message=$"Minimum order amount is ₹{coupon.MinOrderAmount}" };

            // Per-user limit check
            if (coupon.UsageLimitPerUser.HasValue)
            {
                var userUsage = coupon.Usages.Count(u => u.UserId == userId);
                if (userUsage >= coupon.UsageLimitPerUser.Value)
                    return new ApplyCouponResponseDto { Success=false, Message="You have already used this coupon" };
            }

            decimal discount = coupon.DiscountType == "Percent"
                ? req.OrderAmount * (coupon.DiscountValue / 100)
                : coupon.DiscountValue;

            if (coupon.MaxDiscount > 0 && discount > coupon.MaxDiscount)
                discount = coupon.MaxDiscount;

            discount = Math.Min(discount, req.OrderAmount);

            return new ApplyCouponResponseDto
            {
                Success        = true,
                Message        = $"Coupon applied! You save ₹{Math.Round(discount)}",
                DiscountAmount = Math.Round(discount, 2),
                FinalAmount    = Math.Round(req.OrderAmount - discount, 2),
                Code           = coupon.Code,
            };
        }

        public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
        {
            var coupon = new Coupon
            {
                Code          = dto.Code.ToUpper().Trim(),
                Description   = dto.Description,
                DiscountType  = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinOrderAmount= dto.MinOrderAmount,
                MaxDiscount   = dto.MaxDiscount,
                UsageLimit    = dto.UsageLimit,
                UsageLimitPerUser = dto.UsageLimitPerUser,
                AppliesTo     = dto.AppliesTo,
                TargetValue   = dto.TargetValue,
                ValidFrom     = dto.ValidFrom,
                ValidTo       = dto.ValidTo,
            };
            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();
            return Map(coupon);
        }

        public async Task<bool> ToggleAsync(int id)
        {
            var c = await _db.Coupons.FindAsync(id);
            if (c == null) return false;
            c.IsActive = !c.IsActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var c = await _db.Coupons.FindAsync(id);
            if (c == null) return false;
            _db.Coupons.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }

        private static CouponDto Map(Coupon c) => new()
        {
            Id=c.Id, Code=c.Code, Description=c.Description,
            DiscountType=c.DiscountType, DiscountValue=c.DiscountValue,
            MinOrderAmount=c.MinOrderAmount, MaxDiscount=c.MaxDiscount,
            UsageLimit=c.UsageLimit, UsedCount=c.UsedCount,
            AppliesTo=c.AppliesTo, IsActive=c.IsActive,
            ValidFrom=c.ValidFrom.ToString("MMM dd, yyyy"),
            ValidTo=c.ValidTo.ToString("MMM dd, yyyy"),
            IsExpired=c.ValidTo < DateTime.UtcNow,
            IsExhausted=c.UsageLimit>0 && c.UsedCount>=c.UsageLimit,
        };
    }
}
