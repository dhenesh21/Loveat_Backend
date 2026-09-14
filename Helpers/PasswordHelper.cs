using System.Security.Cryptography;

namespace LovEat.API.Helpers
{
    /// <summary>
    /// PBKDF2 password hashing for admin accounts (Customer/Chef login stays
    /// OTP-only and never touches this). Uses .NET's built-in
    /// Rfc2898DeriveBytes rather than pulling in an external BCrypt package —
    /// no new dependency needed for a single-purpose admin login.
    /// </summary>
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        /// <summary>Returns "{base64 salt}.{base64 hash}" — store this whole string in User.PasswordHash.</summary>
        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            var parts = stored.Split('.');
            if (parts.Length != 2) return false;

            try
            {
                var salt = Convert.FromBase64String(parts[0]);
                var expectedHash = Convert.FromBase64String(parts[1]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
