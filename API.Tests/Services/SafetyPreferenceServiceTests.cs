using LovEat.API.DTOs;
using LovEat.API.Tests.TestSupport;
using Xunit;

namespace LovEat.API.Tests.Services
{
    public class SafetyPreferenceServiceTests
    {
        [Fact]
        public async Task GetOrCreateAsync_CreatesDefaultRow_WhenNoneExists()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);

            var dto = await svc.GetOrCreateAsync(userId: 1);

            Assert.False(dto.AcceptOnlyFemaleVerifiedHouseholds);
            Assert.False(dto.RequireSecondPersonPresent);
            Assert.Single(db.SafetyPreferences);
        }

        [Fact]
        public async Task GetOrCreateAsync_ReturnsExistingRow_WithoutCreatingADuplicate()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);

            await svc.GetOrCreateAsync(userId: 1);
            await svc.GetOrCreateAsync(userId: 1);

            Assert.Single(db.SafetyPreferences);
        }

        [Fact]
        public async Task UpdateAsync_CreatesRow_WhenNoneExistsYet()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);

            var dto = await svc.UpdateAsync(userId: 1, new UpdateSafetyPreferenceRequestDto
            {
                AcceptOnlyFemaleVerifiedHouseholds = true,
            });

            Assert.True(dto.AcceptOnlyFemaleVerifiedHouseholds);
            Assert.Single(db.SafetyPreferences);
        }

        [Fact]
        public async Task UpdateAsync_OnlyChangesFieldsThatWereProvided()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);

            await svc.UpdateAsync(1, new UpdateSafetyPreferenceRequestDto
            {
                PreferredChefGender = "Female",
                RequireSecondPersonPresent = true,
            });

            // Second call only sets AcceptOnlyFemaleVerifiedHouseholds — the other two fields
            // must survive untouched, since UpdateAsync only applies non-null request fields.
            var dto = await svc.UpdateAsync(1, new UpdateSafetyPreferenceRequestDto
            {
                AcceptOnlyFemaleVerifiedHouseholds = true,
            });

            Assert.Equal("Female", dto.PreferredChefGender);
            Assert.True(dto.RequireSecondPersonPresent);
            Assert.True(dto.AcceptOnlyFemaleVerifiedHouseholds);
        }

        [Fact]
        public async Task CheckChefAcceptanceAsync_Allows_WhenChefHasNoPreferenceRow()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);

            var result = await svc.CheckChefAcceptanceAsync(chefUserId: 1, customerUserId: 2);

            Assert.True(result.Allowed);
        }

        [Fact]
        public async Task CheckChefAcceptanceAsync_Blocks_WhenChefRequiresFemaleHouseholdAndCustomerGenderIsNull()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);
            await svc.UpdateAsync(chefUserId(), new UpdateSafetyPreferenceRequestDto { AcceptOnlyFemaleVerifiedHouseholds = true });

            db.Users.Add(new LovEat.API.Models.User { Id = 2, FullName = "Customer", PhoneNumber = "9000000003", Role = "Customer" });
            db.UserProfiles.Add(new LovEat.API.Models.UserProfile { UserId = 2, Gender = null });
            db.SaveChanges();

            var result = await svc.CheckChefAcceptanceAsync(chefUserId: chefUserId(), customerUserId: 2);

            Assert.False(result.Allowed);
            Assert.NotNull(result.Reason);
        }

        [Fact]
        public async Task CheckChefAcceptanceAsync_Blocks_WhenChefRequiresFemaleHouseholdAndCustomerIsMale()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);
            await svc.UpdateAsync(chefUserId(), new UpdateSafetyPreferenceRequestDto { AcceptOnlyFemaleVerifiedHouseholds = true });

            db.Users.Add(new LovEat.API.Models.User { Id = 2, FullName = "Customer", PhoneNumber = "9000000003", Role = "Customer" });
            db.UserProfiles.Add(new LovEat.API.Models.UserProfile { UserId = 2, Gender = "Male" });
            db.SaveChanges();

            var result = await svc.CheckChefAcceptanceAsync(chefUserId: chefUserId(), customerUserId: 2);

            Assert.False(result.Allowed);
        }

        [Fact]
        public async Task CheckChefAcceptanceAsync_Allows_WhenChefRequiresFemaleHouseholdAndCustomerIsFemale()
        {
            var db = TestDbContextFactory.Create();
            var svc = new LovEat.API.Services.SafetyPreferenceService(db);
            await svc.UpdateAsync(chefUserId(), new UpdateSafetyPreferenceRequestDto { AcceptOnlyFemaleVerifiedHouseholds = true });

            db.Users.Add(new LovEat.API.Models.User { Id = 2, FullName = "Customer", PhoneNumber = "9000000003", Role = "Customer" });
            db.UserProfiles.Add(new LovEat.API.Models.UserProfile { UserId = 2, Gender = "Female" });
            db.SaveChanges();

            var result = await svc.CheckChefAcceptanceAsync(chefUserId: chefUserId(), customerUserId: 2);

            Assert.True(result.Allowed);
        }

        // Fixed chef id used across the female-household tests above, kept as a local
        // constant so each test stays readable without re-declaring it.
        private static int chefUserId() => 1;
    }
}
