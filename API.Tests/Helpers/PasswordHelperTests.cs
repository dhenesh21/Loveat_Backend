using LovEat.API.Helpers;
using Xunit;

namespace LovEat.API.Tests.Helpers
{
    public class PasswordHelperTests
    {
        [Fact]
        public void Hash_ProducesDifferentOutputForSamePassword_DueToRandomSalt()
        {
            var hash1 = PasswordHelper.Hash("CorrectHorseBatteryStaple");
            var hash2 = PasswordHelper.Hash("CorrectHorseBatteryStaple");

            Assert.NotEqual(hash1, hash2); // salts must differ, so raw hashes must differ too
        }

        [Fact]
        public void Verify_ReturnsTrue_ForCorrectPassword()
        {
            var stored = PasswordHelper.Hash("MySecureP@ssw0rd");
            Assert.True(PasswordHelper.Verify("MySecureP@ssw0rd", stored));
        }

        [Fact]
        public void Verify_ReturnsFalse_ForIncorrectPassword()
        {
            var stored = PasswordHelper.Hash("MySecureP@ssw0rd");
            Assert.False(PasswordHelper.Verify("WrongPassword", stored));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-a-valid-hash-format")]
        [InlineData("only.one.dot.too.many")]
        public void Verify_ReturnsFalse_ForMalformedStoredHash_InsteadOfThrowing(string? malformed)
        {
            // A corrupted/malformed stored hash must fail closed (deny login),
            // never throw an unhandled exception that could crash the request
            // or leak internal state via a 500 error.
            var result = PasswordHelper.Verify("AnyPassword", malformed);
            Assert.False(result);
        }

        [Fact]
        public void Verify_IsCaseSensitive()
        {
            var stored = PasswordHelper.Hash("CaseSensitive123");
            Assert.False(PasswordHelper.Verify("casesensitive123", stored));
        }

        [Fact]
        public void Hash_StoredFormat_ContainsExactlyOneSeparator()
        {
            var stored = PasswordHelper.Hash("AnyPassword1!");
            var parts = stored.Split('.');
            Assert.Equal(2, parts.Length); // "{salt}.{hash}" — Verify() depends on exactly this shape
        }
    }
}
