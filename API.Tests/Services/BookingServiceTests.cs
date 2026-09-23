using LovEat.API.Models;
using LovEat.API.Tests.TestSupport;
using Xunit;

namespace LovEat.API.Tests.Services
{
    public class BookingServiceTests
    {
        // ── CreateBookingAsync ────────────────────────────────────────

        [Fact]
        public async Task CreateBookingAsync_Succeeds_WithValidChefAndCustomer()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef(hourlyRate: 500m);

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId, durationMinutes: 120));

            Assert.True(success, message);
            Assert.NotNull(data);
            Assert.Equal("Pending", data!.Status);
            // 500/hr * 2 hours = 1000
            Assert.Equal(1000m, data.BaseAmount);
            Assert.Equal(1000m, data.TotalAmount);
        }

        [Fact]
        public async Task CreateBookingAsync_Fails_WhenChefNotFound()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId: 99999));

            Assert.False(success);
            Assert.Null(data);
            Assert.Contains("not found", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateBookingAsync_Fails_WhenChefNotAvailable()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef(isAvailable: false);

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));

            Assert.False(success);
            Assert.Null(data);
            Assert.Contains("not accepting bookings", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateBookingAsync_Fails_WhenDurationNotPositive()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId, durationMinutes: 0));

            Assert.False(success);
            Assert.Null(data);
            Assert.Contains("Duration must be positive", message);
        }

        [Fact]
        public async Task CreateBookingAsync_AppliesPercentCoupon_WithMaxDiscountCap()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef(hourlyRate: 1000m); // 2hr => baseAmount 2000

            fx.Db.Coupons.Add(new Coupon
            {
                Code = "SAVE50",
                DiscountType = "Percent",
                DiscountValue = 50, // 50% of 2000 = 1000, but capped below
                MaxDiscount = 300,
                MinOrderAmount = 0,
                IsActive = true,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(1),
            });
            fx.Db.SaveChanges();

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId, durationMinutes: 120, couponCode: "SAVE50"));

            Assert.True(success, message);
            Assert.Equal(2000m, data!.BaseAmount);
            Assert.Equal(300m, data.DiscountAmount); // capped by MaxDiscount, not the full 50%
            Assert.Equal(1700m, data.TotalAmount);
        }

        [Fact]
        public async Task CreateBookingAsync_Fails_WhenCouponCodeInvalid()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId, couponCode: "DOESNOTEXIST"));

            Assert.False(success);
            Assert.Null(data);
            Assert.Contains("Invalid or expired coupon", message);
        }

        [Fact]
        public async Task CreateBookingAsync_Fails_WhenChefRequiresFemaleVerifiedHouseholdsAndCustomerIsNot()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer(gender: "Male");
            var chefId = fx.SeedChef();

            await fx.SafetyPref.UpdateAsync(chefId, new LovEat.API.DTOs.UpdateSafetyPreferenceRequestDto
            {
                AcceptOnlyFemaleVerifiedHouseholds = true
            });

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));

            Assert.False(success);
            Assert.Null(data);
            Assert.Contains("only accepts bookings from female-verified households", message);
        }

        [Fact]
        public async Task CreateBookingAsync_Succeeds_WhenChefRequiresFemaleVerifiedHouseholdsAndCustomerIs()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer(gender: "Female");
            var chefId = fx.SeedChef();

            await fx.SafetyPref.UpdateAsync(chefId, new LovEat.API.DTOs.UpdateSafetyPreferenceRequestDto
            {
                AcceptOnlyFemaleVerifiedHouseholds = true
            });

            var (success, message, data) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));

            Assert.True(success, message);
            Assert.NotNull(data);
        }

        // ── Lifecycle: Accept → Start → Complete ───────────────────────

        [Fact]
        public async Task FullLifecycle_AcceptStartComplete_UpdatesStatusAndAwardsLoyaltyAndIncrementsChefStats()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();
            var (_, _, created) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));
            var bookingId = created!.Id;

            var (acceptOk, acceptMsg) = await fx.Booking.AcceptBookingAsync(chefId, bookingId);
            Assert.True(acceptOk, acceptMsg);

            var afterAccept = await fx.Booking.GetBookingAsync(chefId, bookingId);
            Assert.Equal("Accepted", afterAccept!.Status);
            Assert.NotNull(afterAccept.AcceptedAt);

            // M182: accepting should have created a timeout-watch row
            Assert.True(fx.Db.BookingTimeoutAlerts.Any(a => a.BookingId == bookingId));
            // M181: accepting should have generated an arrival OTP
            Assert.True(fx.Db.ArrivalVerifications.Any(v => v.BookingId == bookingId));

            var (startOk, startMsg) = await fx.Booking.StartBookingAsync(chefId, bookingId);
            Assert.True(startOk, startMsg);
            Assert.Equal("InProgress", (await fx.Booking.GetBookingAsync(chefId, bookingId))!.Status);

            var (completeOk, completeMsg) = await fx.Booking.CompleteBookingAsync(chefId, bookingId);
            Assert.True(completeOk, completeMsg);

            var finalBooking = await fx.Booking.GetBookingAsync(chefId, bookingId);
            Assert.Equal("Completed", finalBooking!.Status);
            Assert.NotNull(finalBooking.CompletedAt);

            var chefProfile = fx.Db.ChefProfiles.First(p => p.UserId == chefId);
            Assert.Equal(1, chefProfile.TotalBookingsCompleted);

            var loyaltyAwarded = fx.Db.LoyaltyTransactions.Any(t => t.UserId == customerId && t.BookingId == bookingId);
            Assert.True(loyaltyAwarded, "Completing a booking should award the customer loyalty points.");
        }

        [Fact]
        public async Task AcceptBookingAsync_Fails_WhenCalledByADifferentChef()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();
            var otherChefId = fx.SeedChef(name: "Someone Else");
            var (_, _, created) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));

            var (success, message) = await fx.Booking.AcceptBookingAsync(otherChefId, created!.Id);

            Assert.False(success);
            Assert.Contains("not found", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task StartBookingAsync_Fails_WhenBookingIsStillPending()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();
            var (_, _, created) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));

            var (success, message) = await fx.Booking.StartBookingAsync(chefId, created!.Id);

            Assert.False(success);
            Assert.Contains("can't be started from status 'Pending'", message);
        }

        // ── CancelBookingAsync — refund-on-cancel bug fix ──────────────

        [Fact]
        public async Task CancelBookingAsync_RefundsWalletAndMarksPaymentRefunded_WhenBookingWasPaid()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef(hourlyRate: 500m);
            var (_, _, created) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId, durationMinutes: 120));
            var bookingId = created!.Id;

            // Simulate a completed payment for this booking (as PaymentService would have done).
            var booking = fx.Db.Bookings.First(b => b.Id == bookingId);
            booking.PaymentStatus = "Paid";
            fx.Db.Payments.Add(new Payment
            {
                BookingId = bookingId,
                UserId = customerId,
                Amount = booking.TotalAmount,
                Status = "Success",
                CompletedAt = DateTime.UtcNow,
            });
            fx.Db.SaveChanges();

            var walletBefore = fx.Db.Users.First(u => u.Id == customerId).WalletBalance;

            var (success, message) = await fx.Booking.CancelBookingAsync(customerId, bookingId, "Change of plans");

            Assert.True(success, message);
            Assert.Contains("refunded", message, StringComparison.OrdinalIgnoreCase);

            var walletAfter = fx.Db.Users.First(u => u.Id == customerId).WalletBalance;
            Assert.Equal(walletBefore + booking.TotalAmount, walletAfter);

            var finalBooking = fx.Db.Bookings.First(b => b.Id == bookingId);
            Assert.Equal("Cancelled", finalBooking.Status);
            Assert.Equal("Refunded", finalBooking.PaymentStatus);

            var payment = fx.Db.Payments.First(p => p.BookingId == bookingId);
            Assert.Equal("Refunded", payment.Status);
        }

        [Fact]
        public async Task CancelBookingAsync_DoesNotTouchWallet_WhenBookingWasNeverPaid()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();
            var (_, _, created) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));

            var walletBefore = fx.Db.Users.First(u => u.Id == customerId).WalletBalance;

            var (success, message) = await fx.Booking.CancelBookingAsync(customerId, created!.Id, "Changed my mind");

            Assert.True(success, message);
            Assert.DoesNotContain("refunded", message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(walletBefore, fx.Db.Users.First(u => u.Id == customerId).WalletBalance);
        }

        [Fact]
        public async Task CancelBookingAsync_Fails_WhenBookingAlreadyCompleted()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();
            var (_, _, created) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));
            var bookingId = created!.Id;

            await fx.Booking.AcceptBookingAsync(chefId, bookingId);
            await fx.Booking.StartBookingAsync(chefId, bookingId);
            await fx.Booking.CompleteBookingAsync(chefId, bookingId);

            var (success, message) = await fx.Booking.CancelBookingAsync(customerId, bookingId, "Too late");

            Assert.False(success);
            Assert.Contains("can't be cancelled from status 'Completed'", message);
        }

        // ── GetMyBookingsAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetMyBookingsAsync_FiltersByRoleAndStatus()
        {
            var fx = new BookingServiceFixture();
            var customerId = fx.SeedCustomer();
            var chefId = fx.SeedChef();
            var otherCustomerId = fx.SeedCustomer(name: "Other Customer");

            var (_, _, booking1) = await fx.Booking.CreateBookingAsync(customerId, BookingServiceFixture.MakeRequest(chefId));
            var (_, _, booking2) = await fx.Booking.CreateBookingAsync(otherCustomerId, BookingServiceFixture.MakeRequest(chefId));
            await fx.Booking.AcceptBookingAsync(chefId, booking1!.Id);

            var chefBookings = await fx.Booking.GetMyBookingsAsync(chefId, "Chef");
            Assert.Equal(2, chefBookings.Count);

            var chefAcceptedOnly = await fx.Booking.GetMyBookingsAsync(chefId, "Chef", "Accepted");
            Assert.Single(chefAcceptedOnly);
            Assert.Equal(booking1.Id, chefAcceptedOnly[0].Id);

            var customerBookings = await fx.Booking.GetMyBookingsAsync(customerId, "Customer");
            Assert.Single(customerBookings);
            Assert.Equal(booking1.Id, customerBookings[0].Id);
        }
    }
}
