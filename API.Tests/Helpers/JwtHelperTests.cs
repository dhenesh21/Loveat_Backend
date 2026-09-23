using LovEat.API.Helpers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LovEat.API.Tests.Helpers
{
    public class JwtHelperTests
    {
        private static JwtHelper BuildHelper(string secret = "unit-test-secret-must-be-at-least-32-bytes-long!!")
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = secret,
                    ["Jwt:Issuer"] = "LovEat.Tests",
                    ["Jwt:Audience"] = "LovEat.Tests.Clients",
                    ["Jwt:AccessTokenExpiryMinutes"] = "60",
                })
                .Build();
            return new JwtHelper(config);
        }

        [Fact]
        public void Constructor_Throws_WhenSecretMissing()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            Assert.Throws<InvalidOperationException>(() => new JwtHelper(config));
        }

        [Fact]
        public void GenerateToken_ThenValidateToken_RoundTripsClaimsCorrectly()
        {
            var jwt = BuildHelper();
            var token = jwt.GenerateToken(userId: 42, role: "Chef", phone: "+919876543210", email: "chef@example.com");

            var principal = jwt.ValidateToken(token);

            Assert.NotNull(principal);
            Assert.Equal("42", principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            Assert.Equal("Chef", principal.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value);
        }

        [Fact]
        public void ValidateToken_ReturnsNull_ForTokenSignedWithDifferentSecret()
        {
            var jwtA = BuildHelper("secret-a-must-be-at-least-32-bytes-long-too!!");
            var jwtB = BuildHelper("secret-b-must-be-at-least-32-bytes-long-too!!");

            var token = jwtA.GenerateToken(1, "Customer");
            var principal = jwtB.ValidateToken(token);

            // A token signed with a different secret must never validate —
            // this is the core guarantee that prevents a forged/tampered token
            // from being accepted.
            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_ReturnsNull_ForGarbageInput()
        {
            var jwt = BuildHelper();
            Assert.Null(jwt.ValidateToken("this.is.not.a.valid.jwt"));
        }

        [Fact]
        public void GenerateRefreshToken_ProducesUniqueValuesEachCall()
        {
            var jwt = BuildHelper();
            var r1 = jwt.GenerateRefreshToken();
            var r2 = jwt.GenerateRefreshToken();

            Assert.NotEqual(r1, r2);
            Assert.True(r1.Length >= 32); // should be a real random token, not a short/guessable one
        }
    }
}
