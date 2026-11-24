using System.Security.Cryptography;
using System.Text;
using GroceryShopApp.Model;
using GroceryShopApp.Repository;

namespace GroceryShopApp.Services
{
    /// <summary>
    /// Handles the business logics for the user.
    /// </summary>
    internal class UserServices
    {
        private readonly UserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServices"/> class.
        /// </summary>
        /// <param name="userRepository">Repository of the user.</param>
        public UserServices(UserRepository userRepository)
        {
            this._userRepository = userRepository;
        }

        /// <summary>
        /// Creates a new user and passes them to repository for storage.
        /// </summary>
        /// <param name="fullName">Full name of the user</param>
        /// <param name="userName">Unique username of the sue</param>
        /// <param name="phone">Phone number of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Returns whether the new user is stored or not</returns>
        internal bool CreateUser(string fullName, string userName, string phone, string password)
        {
            if (this._userRepository.IsUserAlreadyExists(userName, phone))
            {
                return false;
            }

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string hashedPassword = Convert.ToHexString(hash).ToLowerInvariant();

            bool isAdminPresent = this._userRepository.IsAdminPresent();

            User user = new User
            {
                Name = fullName,
                UserName = userName,
                Phone = phone,
                Password = hashedPassword,
                IsAdmin = isAdminPresent ? false : true,
            };

            this._userRepository.AddUser(user);
            return true;
        }

        /// <summary>
        /// Checks whether the user data already exists in the repository or not.
        /// </summary>
        /// <param name="userName">Unqiue username of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Returns true if user already exists or vice-versa</returns>
        internal bool UserExists(string userName, string password)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string hashedPassword = Convert.ToHexString(hash).ToLowerInvariant();

            if (this._userRepository.IsRegisteredUser(userName, hashedPassword))
            {
                return true;
            }

            return false;
        }
    }
}
