using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // M179: Gender-Preference Matching & Safety Filters
    // ══════════════════════════════════════════════════════════════
    public class SafetyPreferenceService
    {
        private readonly AppDbContext _db;
        public SafetyPreferenceService(AppDbContext db) => _db = db;

        public async Task<SafetyPreferenceDto> GetOrCreateAsync(int userId)
        {
            var pref = await _db.SafetyPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (pref == null)
            {
                pref = new SafetyPreference { UserId = userId };
                _db.SafetyPreferences.Add(pref);
                await _db.SaveChangesAsync();
            }
            return ToDto(pref);
        }

        public async Task<SafetyPreferenceDto> UpdateAsync(int userId, UpdateSafetyPreferenceRequestDto req)
        {
            var pref = await _db.SafetyPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (pref == null)
            {
                pref = new SafetyPreference { UserId = userId };
                _db.SafetyPreferences.Add(pref);
            }

            if (req.PreferredChefGender != null) pref.PreferredChefGender = req.PreferredChefGender;
            if (req.AcceptOnlyFemaleVerifiedHouseholds.HasValue) pref.AcceptOnlyFemaleVerifiedHouseholds = req.AcceptOnlyFemaleVerifiedHouseholds.Value;
            if (req.RequireSecondPersonPresent.HasValue) pref.RequireSecondPersonPresent = req.RequireSecondPersonPresent.Value;
            pref.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ToDto(pref);
        }

        /// <summary>Called by BookingService.CreateBookingAsync before a booking is persisted.
        /// Checks the target chef's safety rules; does not touch customer preference (that's
        /// applied earlier, as a search filter, not a hard block).</summary>
        public async Task<SafetyEligibilityResult> CheckChefAcceptanceAsync(int chefUserId, int customerUserId)
        {
            var chefPref = await _db.SafetyPreferences.FirstOrDefaultAsync(p => p.UserId == chefUserId);
            if (chefPref == null) return new SafetyEligibilityResult { Allowed = true };

            if (chefPref.AcceptOnlyFemaleVerifiedHouseholds)
            {
                var customerProfile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == customerUserId);
                if (customerProfile?.Gender == null || !customerProfile.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                {
                    return new SafetyEligibilityResult
                    {
                        Allowed = false,
                        Reason = "This chef only accepts bookings from female-verified households. Please choose a different chef."
                    };
                }
            }
            // RequireSecondPersonPresent is a confirmation the customer must acknowledge client-side
            // at booking time (checkbox) — enforced at the UI layer, logged here for audit via Booking.Notes.
            return new SafetyEligibilityResult { Allowed = true };
        }

        private static SafetyPreferenceDto ToDto(SafetyPreference p) => new()
        {
            PreferredChefGender = p.PreferredChefGender,
            AcceptOnlyFemaleVerifiedHouseholds = p.AcceptOnlyFemaleVerifiedHouseholds,
            RequireSecondPersonPresent = p.RequireSecondPersonPresent,
            UpdatedAt = p.UpdatedAt
        };
    }

    // ══════════════════════════════════════════════════════════════
    // M180: Trusted Contact Auto-Notification
    // ══════════════════════════════════════════════════════════════
    public class TrustedContactAlertService
    {
        private readonly AppDbContext _db;
        private readonly ISmsService _sms;
        private readonly ILogger<TrustedContactAlertService> _logger;

        public TrustedContactAlertService(AppDbContext db, ISmsService sms, ILogger<TrustedContactAlertService> logger)
        {
            _db = db;
            _sms = sms;
            _logger = logger;
        }

        /// <summary>Called by BookingService when a booking's status moves to "Accepted".
        /// Fires for both the customer and the chef, independently, if each has the toggle on.
        /// Never throws — a failed SMS should never block the booking flow.</summary>
        public async Task NotifyBothPartiesAsync(int bookingId)
        {
            var booking = await _db.Bookings.Include(b => b.Customer).Include(b => b.Chef).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return;

            if (booking.Customer != null) await NotifyOneAsync(booking, booking.Customer.Id, isChef: false);
            if (booking.Chef != null) await NotifyOneAsync(booking, booking.Chef.Id, isChef: true);
        }

        private async Task NotifyOneAsync(Booking booking, int userId, bool isChef)
        {
            try
            {
                var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile == null || !profile.TrustedContactNotifyEnabled || string.IsNullOrWhiteSpace(profile.EmergencyContactPhone))
                    return;

                var user = await _db.Users.FindAsync(userId);
                var role = isChef ? "chef" : "family member";
                var message = $"LovEat Safety: {user?.FullName ?? "Your " + role} has a cooking booking starting at {booking.Address}, " +
                               $"scheduled {booking.ScheduledAt:MMM d, h:mm tt}. This is an automated safety notice.";

                var alert = new TrustedContactAlert
                {
                    UserId = userId,
                    BookingId = booking.Id,
                    ContactPhone = profile.EmergencyContactPhone!,
                    Message = message,
                    Channel = "SMS",
                    DeliveryStatus = "Pending"
                };
                _db.TrustedContactAlerts.Add(alert);
                await _db.SaveChangesAsync();

                var (success, error) = await _sms.SendAsync(profile.EmergencyContactPhone!, message);
                alert.DeliveryStatus = success ? "Sent" : "Failed";
                alert.SentAt = success ? DateTime.UtcNow : null;
                if (!success) _logger.LogWarning("TrustedContactAlert {Id} failed: {Error}", alert.Id, error);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TrustedContactAlert failed for booking {BookingId}, user {UserId}", booking.Id, userId);
            }
        }

        public async Task<List<TrustedContactAlertDto>> GetForBookingAsync(int bookingId) =>
            await _db.TrustedContactAlerts.Where(a => a.BookingId == bookingId)
                .Select(a => new TrustedContactAlertDto
                {
                    Id = a.Id, BookingId = a.BookingId, ContactPhone = a.ContactPhone,
                    Channel = a.Channel, DeliveryStatus = a.DeliveryStatus, CreatedAt = a.CreatedAt, SentAt = a.SentAt
                }).ToListAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // M181: Arrival Identity Verification
    // ══════════════════════════════════════════════════════════════
    public class ArrivalVerificationService
    {
        private readonly AppDbContext _db;
        private static readonly Random _rng = new();

        public ArrivalVerificationService(AppDbContext db) => _db = db;

        /// <summary>Called by BookingService when a booking moves to "Accepted".</summary>
        public async Task<ArrivalVerification> GenerateAsync(int bookingId, int chefId)
        {
            var existing = await _db.ArrivalVerifications.FirstOrDefaultAsync(v => v.BookingId == bookingId);
            if (existing != null) return existing;

            var otp = _rng.Next(100000, 999999).ToString();
            var verification = new ArrivalVerification
            {
                BookingId = bookingId,
                ChefId = chefId,
                ArrivalOtp = otp,
                Status = "Pending"
            };
            _db.ArrivalVerifications.Add(verification);
            await _db.SaveChangesAsync();
            return verification;
        }

        /// <summary>Only the customer on this booking may retrieve the OTP to show/tell the chef.</summary>
        public async Task<string?> GetOtpForCustomerAsync(int customerUserId, int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == customerUserId);
            if (booking == null) return null;
            var v = await _db.ArrivalVerifications.FirstOrDefaultAsync(x => x.BookingId == bookingId);
            return v?.ArrivalOtp;
        }

        /// <summary>Chef submits the OTP the customer told them. Verified → unlocks the
        /// OnTheWay→Arrived transition in BookingTracking (checked there, not enforced here).</summary>
        public async Task<(bool Success, string Message)> SubmitOtpAsync(int chefUserId, SubmitArrivalOtpRequestDto req)
        {
            var v = await _db.ArrivalVerifications.FirstOrDefaultAsync(x => x.BookingId == req.BookingId && x.ChefId == chefUserId);
            if (v == null) return (false, "No arrival verification found for this booking");
            if (v.Status == "Verified") return (true, "Already verified");

            if (v.ArrivalOtp != req.Otp.Trim())
            {
                v.FailedAttempts++;
                if (v.FailedAttempts >= 5) v.Status = "Failed";
                await _db.SaveChangesAsync();
                return (false, v.Status == "Failed" ? "Too many failed attempts — contact support" : "Incorrect code");
            }

            v.Status = "Verified";
            v.VerifiedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Arrival verified");
        }

        public async Task<bool> SkipAsync(int chefUserId, SkipArrivalVerificationRequestDto req)
        {
            var v = await _db.ArrivalVerifications.FirstOrDefaultAsync(x => x.BookingId == req.BookingId && x.ChefId == chefUserId);
            if (v == null) return false;
            v.Status = "Skipped";
            v.VerifiedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsVerifiedOrSkippedAsync(int bookingId)
        {
            var v = await _db.ArrivalVerifications.FirstOrDefaultAsync(x => x.BookingId == bookingId);
            return v != null && (v.Status == "Verified" || v.Status == "Skipped");
        }

        public async Task<ArrivalVerificationDto?> GetAsync(int bookingId)
        {
            var v = await _db.ArrivalVerifications.FirstOrDefaultAsync(x => x.BookingId == bookingId);
            if (v == null) return null;
            return new ArrivalVerificationDto
            {
                Id = v.Id, BookingId = v.BookingId, Status = v.Status,
                FailedAttempts = v.FailedAttempts, GeneratedAt = v.GeneratedAt, VerifiedAt = v.VerifiedAt
            };
        }
    }
}
