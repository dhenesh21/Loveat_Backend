using LovEat.API.Data;
using LovEat.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LovEat.API.Tests.Services
{
    // A minimal fake ISmsService that never actually sends anything — lets
    // OtpService's own logic (generation, expiry, replay prevention, rate
    // limiting) be tested without a real MSG91 call.
    public class FakeSmsService : ISmsService
    {
        public bool ShouldSucceed { get; set; } = true;
        public string? LastMessageSent { get; private set; }
        public bool IsConfigured => true;

        public Task<(bool Success, string? ErrorMessage)> SendAsync(string phoneNumber, string message)
        {
            LastMessageSent = message;
            return Task.FromResult(ShouldSucceed ? (true, (string?)null) : (false, "Simulated SMS failure"));
        }
    }

    public class OtpServiceTests
    {
        private static AppDbContext NewInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task SendOtpAsync_Succeeds_AndPersistsAnUnusedOtpRow()
        {
            await using var db = NewInMemoryDb();
            var sms = new FakeSmsService();
            var otpService = new OtpService(db, sms);

            var (success, message) = await otpService.SendOtpAsync("+919876543210", "Login");

            Assert.True(success);
            var stored = await db.OtpVerifications.SingleAsync();
            Assert.Equal("+919876543210", stored.Phone);
            Assert.False(stored.IsUsed);
            Assert.Equal(6, stored.OtpCode.Length);
            Assert.Contains(stored.OtpCode, sms.LastMessageSent);
        }

        [Fact]
        public async Task SendOtpAsync_Fails_WhenSmsProviderFails_ButStillPersistsTheOtpRow()
        {
            await using var db = NewInMemoryDb();
            var sms = new FakeSmsService { ShouldSucceed = false };
            var otpService = new OtpService(db, sms);

            var (success, message) = await otpService.SendOtpAsync("+919876543210", "Login");

            Assert.False(success);
            Assert.Single(db.OtpVerifications); // row still exists even though delivery failed
        }

        [Fact]
        public async Task SendOtpAsync_RateLimits_AfterFiveRequestsInWindow()
        {
            await using var db = NewInMemoryDb();
            var otpService = new OtpService(db, new FakeSmsService());

            for (int i = 0; i < 5; i++)
                await otpService.SendOtpAsync("+919876543210", "Login");

            var (success, message) = await otpService.SendOtpAsync("+919876543210", "Login");

            Assert.False(success);
            Assert.Contains("Too many", message);
        }

        [Fact]
        public async Task VerifyOtpAsync_Succeeds_WithCorrectCode()
        {
            await using var db = NewInMemoryDb();
            var otpService = new OtpService(db, new FakeSmsService());
            await otpService.SendOtpAsync("+919876543210", "Login");
            var code = (await db.OtpVerifications.SingleAsync()).OtpCode;

            var (success, _) = await otpService.VerifyOtpAsync("+919876543210", code, "Login");

            Assert.True(success);
        }

        [Fact]
        public async Task VerifyOtpAsync_Fails_WithIncorrectCode()
        {
            await using var db = NewInMemoryDb();
            var otpService = new OtpService(db, new FakeSmsService());
            await otpService.SendOtpAsync("+919876543210", "Login");

            var (success, message) = await otpService.VerifyOtpAsync("+919876543210", "000000", "Login");

            Assert.False(success);
            Assert.Equal("Incorrect OTP.", message);
        }

        [Fact]
        public async Task VerifyOtpAsync_CannotBeReplayed_AfterSuccessfulVerification()
        {
            await using var db = NewInMemoryDb();
            var otpService = new OtpService(db, new FakeSmsService());
            await otpService.SendOtpAsync("+919876543210", "Login");
            var code = (await db.OtpVerifications.SingleAsync()).OtpCode;

            var first = await otpService.VerifyOtpAsync("+919876543210", code, "Login");
            var second = await otpService.VerifyOtpAsync("+919876543210", code, "Login");

            Assert.True(first.Success);
            Assert.False(second.Success); // the same OTP must not verify twice — replay protection
        }

        [Fact]
        public async Task VerifyOtpAsync_Fails_AfterExpiry()
        {
            await using var db = NewInMemoryDb();
            db.OtpVerifications.Add(new Models.OtpVerification
            {
                Phone = "+919876543210",
                OtpCode = "123456",
                Purpose = "Login",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1), // already expired
            });
            await db.SaveChangesAsync();
            var otpService = new OtpService(db, new FakeSmsService());

            var (success, message) = await otpService.VerifyOtpAsync("+919876543210", "123456", "Login");

            Assert.False(success);
            Assert.Contains("expired", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task VerifyOtpAsync_LocksOut_AfterFiveIncorrectAttempts()
        {
            await using var db = NewInMemoryDb();
            var otpService = new OtpService(db, new FakeSmsService());
            await otpService.SendOtpAsync("+919876543210", "Login");

            for (int i = 0; i < 5; i++)
                await otpService.VerifyOtpAsync("+919876543210", "000000", "Login");

            var (success, message) = await otpService.VerifyOtpAsync("+919876543210", "000000", "Login");

            Assert.False(success);
            Assert.Contains("Too many incorrect attempts", message);
        }
    }
}
