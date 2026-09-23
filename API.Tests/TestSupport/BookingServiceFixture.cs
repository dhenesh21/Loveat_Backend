using Microsoft.Extensions.Logging.Abstractions;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using LovEat.API.Services;

namespace LovEat.API.Tests.TestSupport
{
    /// <summary>
    /// Wires a real BookingService — including the M179-184 Safety Suite dependencies merged in
    /// from the safety-hardening branch (SafetyPreferenceService, TrustedContactAlertService,
    /// ArrivalVerificationService, BookingTimeoutAlertService, DedicatedChefService) — against
    /// one isolated in-memory AppDbContext. Only the two services that talk to the outside world
    /// (SMS, push) are swapped for no-op fakes.
    /// </summary>
    public class BookingServiceFixture
    {
        public AppDbContext Db { get; }
        public BookingService Booking { get; }
        public SafetyPreferenceService SafetyPref { get; }
        public FakeSmsService Sms { get; } = new();
        public FakePushNotificationService Push { get; } = new();

        public BookingServiceFixture()
        {
            Db = TestDbContextFactory.Create();

            var loyalty = new LoyaltyService(Db);
            var wallet = new WalletService(Db);
            SafetyPref = new SafetyPreferenceService(Db);
            var trustedContact = new TrustedContactAlertService(Db, Sms, NullLogger<TrustedContactAlertService>.Instance);
            var arrivalVerification = new ArrivalVerificationService(Db);
            var timeoutAlert = new BookingTimeoutAlertService(Db, Push, trustedContact, NullLogger<BookingTimeoutAlertService>.Instance);
            var dedicatedChef = new DedicatedChefService(Db);

            Booking = new BookingService(Db, loyalty, wallet, SafetyPref, trustedContact, arrivalVerification, timeoutAlert, dedicatedChef);
        }

        public int SeedCustomer(string name = "Test Customer", string? gender = null)
        {
            var user = new User { FullName = name, PhoneNumber = "9000000001", Role = "Customer", IsActive = true };
            Db.Users.Add(user);
            Db.SaveChanges();
            Db.UserProfiles.Add(new UserProfile { UserId = user.Id, Gender = gender });
            Db.SaveChanges();
            return user.Id;
        }

        public int SeedChef(string name = "Test Chef", decimal hourlyRate = 500m, bool isAvailable = true)
        {
            var user = new User { FullName = name, PhoneNumber = "9000000002", Role = "Chef", IsActive = true };
            Db.Users.Add(user);
            Db.SaveChanges();
            Db.ChefProfiles.Add(new ChefProfile { UserId = user.Id, HourlyRate = hourlyRate, IsAvailable = isAvailable });
            Db.SaveChanges();
            return user.Id;
        }

        public static CreateBookingRequestDto MakeRequest(int chefId, int durationMinutes = 120, string? couponCode = null) => new()
        {
            ChefId = chefId,
            BookingType = "Instant",
            Cuisine = "South Indian",
            ScheduledAt = DateTime.UtcNow.AddHours(2),
            DurationMinutes = durationMinutes,
            GuestCount = 2,
            Address = "12 Test Street, Madurai",
            Latitude = 9.9252m,
            Longitude = 78.1198m,
            CouponCode = couponCode,
        };
    }
}
