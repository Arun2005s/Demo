using System.Security.Cryptography;

namespace Timer.Helpers
{
    /// <summary>
    /// Provides shared password validation, hashing, and verification operations.
    /// </summary>
    internal static class PasswordHelper
    {
        private const int MinPasswordLength = 8;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// Validates that a password meets the application's security requirements.
        /// </summary>
        /// <param name="password">The plain-text password to validate.</param>
        /// <param name="errorMessage">When this method returns false, contains the validation message.</param>
        /// <returns>True if the password is valid; otherwise, false.</returns>
        public static bool Validate(string password, out string errorMessage)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            {
                errorMessage = $"Password must contain at least {MinPasswordLength} characters.";
                return false;
            }

            if (password.Any(char.IsWhiteSpace))
            {
                errorMessage = "Password cannot contain spaces.";
                return false;
            }

            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit) || password.All(char.IsLetterOrDigit))
            {
                errorMessage = "Password must contain uppercase and lowercase letters, a number, and a special character.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Creates a salted PBKDF2 password hash formatted as <c>{salt}.{hash}</c>.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>The Base64-encoded salt and hash.</returns>
        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a plain-text password against a stored salted hash.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="storedPassword">The stored salt and hash.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        public static bool Verify(string password, string storedPassword)
        {
            string[] parts = storedPassword.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);
                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, expectedHash.Length);
                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
