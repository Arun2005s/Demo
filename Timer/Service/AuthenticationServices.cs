using System.Net.Mail;
using System.Security.Cryptography;
using Timer.Interfaces;
using Timer.Models;

namespace Timer.Services
{
    /// <summary>
    /// Handles user registration and authentication operations with validation and secure password hashing.
    /// </summary>
    public class AuthenticationServices
    {
        private const int MinPasswordLength = 8;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServices"/> class.
        /// </summary>
        /// <param name="userRepository">The repository used to access and persist user data.</param>
        public AuthenticationServices(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
        }

        /// <summary>
        /// Validates registration details and registers a new user account with a salted password hash and daily start time.
        /// </summary>
        /// <param name="name">The display name of the new user.</param>
        /// <param name="email">The desired email address for the new account.</param>
        /// <param name="password">The plain-text password for the new account.</param>
        /// <param name="dayStartTime">The user's preferred daily work start time.</param>
        /// <param name="message">When this method returns, contains a status or validation error message.</param>
        /// <returns><see langword="true"/> if registration succeeds; otherwise, <see langword="false"/>.</returns>
        public bool SignUp(string name, string email, string password, TimeOnly dayStartTime, out string message)
        {
            name = name.Trim();
            email = email.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                message = "Name cannot be empty.";
                return false;
            }

            if (!IsValidEmail(email))
            {
                message = "Please enter a valid email address.";
                return false;
            }

            if (!ValidatePassword(password, out message))
            {
                return false;
            }

            if (this._userRepository.GetByEmail(email) != null)
            {
                message = "An account with this email already exists.";
                return false;
            }

            User user = new User
            {
                UserId = Guid.NewGuid(),
                Name = name,
                Email = email,
                Password = HashPassword(password),
                DayStartTime = dayStartTime,
            };

            this._userRepository.Add(user);

            message = "Registration successful!";
            return true;
        }

        /// <summary>
        /// Authenticates a user by verifying the provided email and password against their stored hash.
        /// </summary>
        /// <param name="email">The email address of the account to log into.</param>
        /// <param name="password">The plain-text password to verify.</param>
        /// <returns>The authenticated <see cref="User"/> if credentials are valid; otherwise, <see langword="null"/>.</returns>
        public User? Login(string email, string password)
        {
            email = email.Trim();

            User? user = this._userRepository.GetByEmail(email);

            if (user == null)
            {
                return null;
            }

            if (!VerifyPassword(password, user.Password))
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Validates whether the specified string is a properly formatted email address.
        /// </summary>
        /// <param name="email">The email string to validate.</param>
        /// <returns><see langword="true"/> if the email format is valid; otherwise, <see langword="false"/>.</returns>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Any(char.IsWhiteSpace))
            {
                return false;
            }

            return MailAddress.TryCreate(email, out MailAddress? parsed)
                   && parsed.Address == email
                   && email.Contains('.');
        }

        /// <summary>
        /// Validates that a password meets minimum length and complexity requirements.
        /// </summary>
        /// <param name="password">The plain-text password to validate.</param>
        /// <param name="errorMessage">When this method returns, contains the specific rule violated, or <see cref="string.Empty"/> if valid.</param>
        /// <returns><see langword="true"/> if the password meets all strength rules; otherwise, <see langword="false"/>.</returns>
        private static bool ValidatePassword(string password, out string errorMessage)
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

            if (!password.Any(char.IsUpper))
            {
                errorMessage = "Password must contain at least one uppercase letter (A-Z).";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                errorMessage = "Password must contain at least one lowercase letter (a-z).";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                errorMessage = "Password must contain at least one number (0-9).";
                return false;
            }

            if (password.All(char.IsLetterOrDigit))
            {
                errorMessage = "Password must contain at least one special character (e.g., !@#$%^&*).";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Generates a salted PBKDF2 hash string formatted as <c>{salt}.{hash}</c>.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>A Base64-encoded salt and hash separated by a period.</returns>
        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a plain-text password against a stored <c>{salt}.{hash}</c> string using constant-time comparison.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="storedPassword">The stored salted hash string.</param>
        /// <returns><see langword="true"/> if the password matches; otherwise, <see langword="false"/>.</returns>
        private static bool VerifyPassword(string password, string storedPassword)
        {
            string[] parts = storedPassword.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
