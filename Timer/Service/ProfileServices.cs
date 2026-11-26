using Timer.Helpers;
using Timer.Interfaces;
using Timer.Models;

namespace Timer.Service
{
    /// <summary>
    /// Provides services for managing and updating user profile data safely with validation and hashing.
    /// </summary>
    public class ProfileServices
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileServices"/> class.
        /// </summary>
        /// <param name="userRepository">The repository used to access and persist user data.</param>
        public ProfileServices(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
        }

        /// <summary>
        /// Updates the profile data of an existing user. Email address remains unchanged.
        /// If a password is provided, it is validated and securely salted/hashed.
        /// </summary>
        /// <param name="email">The email address used to identify the user.</param>
        /// <param name="updatedUser">The object containing the new profile data values.</param>
        /// <param name="errorMessage">When this method returns false, contains the validation or operational error message.</param>
        /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> or <paramref name="updatedUser"/> is null.</exception>
        public bool UpdateProfile(string email, User updatedUser, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            if (updatedUser == null)
            {
                throw new ArgumentNullException(nameof(updatedUser));
            }

            User? existingUser = this._userRepository.GetByEmail(email);
            if (existingUser == null)
            {
                errorMessage = "User profile not found.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                if (!this.ValidatePassword(updatedUser.Password, out errorMessage))
                {
                    return false;
                }

                existingUser.Password = this.HashPassword(updatedUser.Password);
            }

            existingUser.Name = updatedUser.Name;
            existingUser.DayStartTime = updatedUser.DayStartTime;

            this.CommitChanges(existingUser);

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Commits changes back to the repository data structure.
        /// </summary>
        private void CommitChanges(User user)
        {
            this._userRepository.Update(user);
        }

        /// <summary>
        /// Validates that a password meets minimum length and complexity requirements.
        /// </summary>
        /// <param name="password">The plain-text password to validate.</param>
        /// <param name="errorMessage">When this method returns, contains the specific rule violated, or <see cref="string.Empty"/> if valid.</param>
        /// <returns><see langword="true"/> if the password meets all strength rules; otherwise, <see langword="false"/>.</returns>
        private bool ValidatePassword(string password, out string errorMessage)
        {
            return PasswordHelper.Validate(password, out errorMessage);
        }

        /// <summary>
        /// Generates a salted PBKDF2 hash string formatted as <c>{salt}.{hash}</c>.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>A Base64-encoded salt and hash separated by a period.</returns>
        private string HashPassword(string password)
        {
            return PasswordHelper.Hash(password);
        }
    }
}
