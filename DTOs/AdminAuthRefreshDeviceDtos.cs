namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // Admin authentication (separate from Customer/Chef OTP login)
    // ══════════════════════════════════════════════════════════════
    public class AdminLoginRequestDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AdminAuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string PermissionsJson { get; set; } = "[]";
    }

    // ══════════════════════════════════════════════════════════════
    // Refresh token (fixes: refresh tokens were generated but never
    // persisted, so logout/session-revocation did nothing)
    // ══════════════════════════════════════════════════════════════
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = "";
    }

    public class RefreshTokenResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // Device token registration (fixes: NotificationService's push stub
    // had no device token table to look up in the first place)
    // ══════════════════════════════════════════════════════════════
    public class RegisterDeviceTokenRequestDto
    {
        public string Token { get; set; } = "";
        public string Platform { get; set; } = "Android"; // Android / iOS
    }
}
