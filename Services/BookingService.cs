using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// M7-M9: the core booking lifecycle (create → accept → start → complete /
    /// cancel). Pricing here is intentionally simple (hourly rate × duration,
    /// plus a flat coupon discount) — batch 18 already built a full
    /// DynamicPricingService with peak-hour/weekend/emergency surcharge
    /// rules; wiring that in is a good follow-up once this basic path is
    /// confirmed working end-to-end, rather than coupling two untested
    /// systems together in the same pass.
    /// </summary>
    public class BookingService
    {
        private readonly AppDbContext _db;
        private readonly LoyaltyService _loyalty;
        private readonly WalletService _wallet;
        private readonly SafetyPreferenceService _safetyPref;
        private readonly TrustedContactAlertService _trustedContact;
        private readonly ArrivalVerificationService _arrivalVerification;
        private readonly BookingTimeoutAlertService _timeoutAlert;
        private readonly DedicatedChefService _dedicatedChef;

        public BookingService(AppDbContext db, LoyaltyService loyalty, WalletService wallet,
            SafetyPreferenceService safetyPref, TrustedContactAlertService trustedContact, ArrivalVerificationService arrivalVerification,
            BookingTimeoutAlertService timeoutAlert, DedicatedChefService dedicatedChef)
        {
            _db = db;
            _loyalty = loyalty;
            _wallet = wallet;
            _safetyPref = safetyPref;
            _trustedContact = trustedContact;
            _arrivalVerification = arrivalVerification;
            _timeoutAlert = timeoutAlert;
            _dedicatedChef = dedicatedChef;
        }

        public async Task<(bool Success, string Message, BookingDto? Data)> CreateBookingAsync(int customerId, CreateBookingRequestDto req)
        {
            // M182: if the customer has an active subscription with a preferred chef and
            // didn't already pick a different chef, use the preferred chef.
            var preferredChefId = await _dedicatedChef.ResolvePreferredChefAsync(customerId);
            if (preferredChefId.HasValue && req.ChefId == 0)
            {
                req.ChefId = preferredChefId.Value;
            }

            var chef = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.ChefId && u.Role == "Chef" && u.IsActive);
            if (chef == null) return (false, "Chef not found or not active.", null);

            var chefProfile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == req.ChefId);
            if (chefProfile == null || !chefProfile.IsAvailable)
                return (false, "This chef isn't accepting bookings right now.", null);

            if (req.DurationMinutes <= 0) return (false, "Duration must be positive.", null);

            // M179: chef-side safety rules (e.g. female-verified-households-only) checked before
            // any booking is persisted. Does not block on customer's own preference — that's a
            // search-time filter, not a hard rule.
            var eligibility = await _safetyPref.CheckChefAcceptanceAsync(req.ChefId, customerId);
            if (!eligibility.Allowed) return (false, eligibility.Reason ?? "This chef isn't available for your booking.", null);

            var hours = (decimal)req.DurationMinutes / 60m;
            var baseAmount = Math.Round(chefProfile.HourlyRate * hours, 2);
            decimal discount = 0;

            if (!string.IsNullOrWhiteSpace(req.CouponCode))
            {
                var coupon = await _db.Coupons.FirstOrDefaultAsync(c =>
                    c.Code == req.CouponCode && c.IsActive && c.ValidFrom <= DateTime.UtcNow && c.ValidTo >= DateTime.UtcNow);

                if (coupon == null)
                    return (false, "Invalid or expired coupon code.", null);

                if (baseAmount < coupon.MinOrderAmount)
                    return (false, $"This coupon requires a minimum order of ₹{coupon.MinOrderAmount}.", null);

                if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                    return (false, "This coupon has reached its usage limit.", null);

                discount = coupon.DiscountType == "Percent"
                    ? Math.Round(baseAmount * coupon.DiscountValue / 100m, 2)
                    : coupon.DiscountValue;

                if (coupon.MaxDiscount > 0 && discount > coupon.MaxDiscount)
                    discount = coupon.MaxDiscount;

                coupon.UsedCount++;
                _db.CouponUsages.Add(new CouponUsage
                {
                    CouponId = coupon.Id,
                    UserId = customerId,
                    DiscountApplied = discount,
                });
            }

            var totalAmount = Math.Max(0, baseAmount - discount);

            var booking = new Booking
            {
                CustomerId = customerId,
                ChefId = req.ChefId,
                BookingType = req.BookingType,
                Cuisine = req.Cuisine,
                Status = "Pending",
                ScheduledAt = req.ScheduledAt,
                DurationMinutes = req.DurationMinutes,
                GuestCount = req.GuestCount,
                Address = req.Address,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Notes = req.Notes,
                BaseAmount = baseAmount,
                DiscountAmount = discount,
                TotalAmount = totalAmount,
                PaymentStatus = "Pending",
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return (true, "Booking created.", await ToDtoAsync(booking.Id));
        }

        public async Task<(bool Success, string Message)> AcceptBookingAsync(int chefUserId, int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.ChefId == chefUserId);
            if (booking == null) return (false, "Booking not found.");
            if (booking.Status != "Pending") return (false, $"Booking can't be accepted from status '{booking.Status}'.");

            booking.Status = "Accepted";
            booking.AcceptedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // M180: trusted-contact SMS (fire-and-forget-safe — never throws into this flow)
            await _trustedContact.NotifyBothPartiesAsync(booking.Id);
            // M181: generate the arrival OTP now, ready by the time the chef sets out
            await _arrivalVerification.GenerateAsync(booking.Id, booking.ChefId);
            // M182: start watching this booking for a stalled check-in
            await _timeoutAlert.CreateAsync(booking.Id);

            return (true, "Booking accepted.");
        }

        public async Task<(bool Success, string Message)> StartBookingAsync(int chefUserId, int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.ChefId == chefUserId);
            if (booking == null) return (false, "Booking not found.");
            if (booking.Status != "Accepted") return (false, $"Booking can't be started from status '{booking.Status}'.");

            booking.Status = "InProgress";
            booking.StartedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Booking started.");
        }

        public async Task<(bool Success, string Message)> CompleteBookingAsync(int chefUserId, int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.ChefId == chefUserId);
            if (booking == null) return (false, "Booking not found.");
            if (booking.Status != "InProgress") return (false, $"Booking can't be completed from status '{booking.Status}'.");

            booking.Status = "Completed";
            booking.CompletedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            var chefProfile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == chefUserId);
            if (chefProfile != null) chefProfile.TotalBookingsCompleted++;

            await _db.SaveChangesAsync();

            // Loyalty points for the customer — batch 15's LoyaltyService is already real, so use it directly.
            await _loyalty.AwardPointsAsync(booking.CustomerId, 50, "Booking completed", "Earned", booking.Id);

            return (true, "Booking marked complete.");
        }

        public async Task<(bool Success, string Message)> CancelBookingAsync(int userId, int bookingId, string? reason)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && (b.CustomerId == userId || b.ChefId == userId));
            if (booking == null) return (false, "Booking not found.");

            if (booking.Status is not ("Pending" or "Accepted"))
                return (false, $"Booking can't be cancelled from status '{booking.Status}'.");

            var wasPaid = booking.PaymentStatus == "Paid";

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = reason;
            booking.UpdatedAt = DateTime.UtcNow;

            // BUG FIX: cancelling a paid booking used to leave PaymentStatus as
            // "Paid" and the customer's money simply gone — no refund, no
            // status change, nothing. Refund the customer's wallet and mark
            // both the booking and its payment record accordingly.
            // TODO (G1): once a real payment gateway is wired, a refund for a
            // card/UPI/net-banking payment should go back through that gateway's
            // refund API instead of (or in addition to) a wallet credit —
            // crediting the wallet is the correct behavior for wallet-paid
            // bookings and a reasonable interim behavior for the rest until then.
            if (wasPaid)
            {
                var payment = await _db.Payments
                    .Where(p => p.BookingId == booking.Id && p.Status == "Success")
                    .OrderByDescending(p => p.CompletedAt)
                    .FirstOrDefaultAsync();

                if (payment != null)
                {
                    await _wallet.AdjustAsync(booking.CustomerId, payment.Amount, "Credit", "BookingCancelledRefund", booking.Id);
                    payment.Status = "Refunded";
                }

                booking.PaymentStatus = "Refunded";
            }

            await _db.SaveChangesAsync();

            return (true, wasPaid ? "Booking cancelled and your payment was refunded to your wallet." : "Booking cancelled.");
        }

        public async Task<BookingDto?> GetBookingAsync(int userId, int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && (b.CustomerId == userId || b.ChefId == userId));
            return booking == null ? null : await ToDtoAsync(booking.Id);
        }

        public async Task<List<BookingDto>> GetMyBookingsAsync(int userId, string role, string? statusFilter = null)
        {
            var query = role == "Chef"
                ? _db.Bookings.Where(b => b.ChefId == userId)
                : _db.Bookings.Where(b => b.CustomerId == userId);

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(b => b.Status == statusFilter);

            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            var results = new List<BookingDto>();
            foreach (var b in bookings)
                results.Add(await ToDtoAsync(b.Id) ?? new BookingDto { Id = b.Id });

            return results;
        }

        // ── Admin: all bookings across every customer/chef ─────────────
        // NOTE: GetMyBookingsAsync scopes to a single userId (Chef → ChefId,
        // else → CustomerId) — an Admin passed through that method would be
        // filtered on their own admin userId as if they were a customer,
        // silently returning an empty/wrong list. This is a separate,
        // unscoped query for the admin dashboard.
        public async Task<List<BookingDto>> GetAllBookingsAsync(string? statusFilter = null)
        {
            var query = _db.Bookings.AsQueryable();
            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(b => b.Status == statusFilter);

            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            var results = new List<BookingDto>();
            foreach (var b in bookings)
                results.Add(await ToDtoAsync(b.Id) ?? new BookingDto { Id = b.Id });

            return results;
        }

        private async Task<BookingDto?> ToDtoAsync(int bookingId)
        {
            var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId);
            if (b == null) return null;

            var customer = await _db.Users.FindAsync(b.CustomerId);
            var chef = await _db.Users.FindAsync(b.ChefId);

            return new BookingDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                CustomerName = customer?.FullName ?? "",
                ChefId = b.ChefId,
                ChefName = chef?.FullName ?? "",
                BookingType = b.BookingType,
                Status = b.Status,
                Cuisine = b.Cuisine,
                ScheduledAt = b.ScheduledAt,
                DurationMinutes = b.DurationMinutes,
                GuestCount = b.GuestCount,
                Address = b.Address,
                BaseAmount = b.BaseAmount,
                SurchargeAmount = b.SurchargeAmount,
                DiscountAmount = b.DiscountAmount,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus,
                Notes = b.Notes,
                CancellationReason = b.CancellationReason,
                AcceptedAt = b.AcceptedAt,
                StartedAt = b.StartedAt,
                CompletedAt = b.CompletedAt,
                CancelledAt = b.CancelledAt,
                CreatedAt = b.CreatedAt,
            };
        }
    }
}
