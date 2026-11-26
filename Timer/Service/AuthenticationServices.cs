using System.Net.Mail;
using Timer.Helpers;
using Timer.Interfaces;
using Timer.Models;

namespace Timer.Services
{
    /// <summary>
    /// Handles user registration and authentication operations with validation and secure password hashing.
    /// </summary>
    public class AuthenticationServices
    {
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
                Password = PasswordHelper.Hash(password),
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

            if (!PasswordHelper.Verify(password, user.Password))
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
        /// Validates that a password meets the application's security requirements.
        /// </summary>
        /// <param name="password">The plain-text password to validate.</param>
        /// <param name="errorMessage">When this method returns false, contains the validation message.</param>
        /// <returns>True if the password is valid; otherwise, false.</returns>
        private static bool ValidatePassword(string password, out string errorMessage)
        {
            return PasswordHelper.Validate(password, out errorMessage);
        }
    }
}
