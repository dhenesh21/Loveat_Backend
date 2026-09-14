using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LovEat.API.Helpers
{
    /// <summary>
    /// Generates and validates JWT access tokens for both CustomerApp and ChefApp users,
    /// and for AdminWeb admin users. Reads signing config from the "Jwt" section of
    /// appsettings.json / environment variables (see Program.cs for how this is registered).
    /// </summary>
    public class JwtHelper
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiryMinutes;

        public JwtHelper(IConfiguration config)
        {
            _secret = config["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret is not configured. Set it via environment variable or user-secrets, not in appsettings.json for production.");
            _issuer = config["Jwt:Issuer"] ?? "LovEat.API";
            _audience = config["Jwt:Audience"] ?? "LovEat.Clients";
            _accessTokenExpiryMinutes = int.TryParse(config["Jwt:AccessTokenExpiryMinutes"], out var m) ? m : 60;
        }

        /// <summary>
        /// Generates a signed JWT for a given user. Role should be one of:
        /// "Customer", "Chef", "Admin" (RBAC roles like "SuperAdmin", "SupportAgent"
        /// etc. from the RBAC module can be added as additional claims once that
        /// module is wired — see API/Services/IncidentRBACService.cs).
        /// </summary>
        public string GenerateToken(int userId, string role, string? phone = null, string? email = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Role, role),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };

            if (!string.IsNullOrEmpty(phone)) claims.Add(new Claim("phone", phone));
            if (!string.IsNullOrEmpty(email)) claims.Add(new Claim(ClaimTypes.Email, email));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a long-lived opaque refresh token. Storage/rotation/revocation
        /// (e.g. in a RefreshTokens table) is part of AuthService — build that as
        /// part of Phase 3 (AuthController + AuthService).
        /// </summary>
        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Validates a token and returns the ClaimsPrincipal if valid, or null if not.
        /// Useful for manual validation outside the standard authentication middleware
        /// (e.g. validating a token passed over SignalR query string for ChatHub).
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
