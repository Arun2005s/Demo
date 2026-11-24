using System.Text.Json;
using GroceryShopApp.Model;

namespace GroceryShopApp.Repository
{
    /// <summary>
    /// Handles the storage and retrieval of user data to and from files.
    /// </summary>
    internal class UserRepository
    {
        private readonly List<User> _users = new List<User>();
        private readonly string _filePath;

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="filePath">Path where the user data-file is located</param>
        public UserRepository(string filePath)
        {
            this._filePath = filePath;
            this._users = this.LoadAll();
        }

        /// <summary>
        /// Adds the user data to the repository.
        /// </summary>
        /// <param name="user">Datas of user</param>
        internal void AddUser(User user)
        {
            this._users.Add(user);
            this.WriteAll();
        }

        /// <summary>
        /// Checks whether the user data already exists in repository and returns true if it exists and vice-versa.
        /// </summary>
        /// <param name="userName">Unique username of the user</param>
        /// <param name="phone">Phone number of the user</param>
        /// <returns>Returns whether the user already exits or not</returns>
        internal bool IsUserAlreadyExists(string userName, string phone)
        {
            return this._users.Any(user => user.UserName == userName || user.Phone == phone);
        }

        /// <summary>
        /// Checks whether the user has already signed up or not.
        /// </summary>
        /// <param name="userName">Unique username of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Returns true is user already signed up or vice-versa.</returns>
        internal bool IsRegisteredUser(string userName, string password)
        {
            return this._users.Any(user => user.UserName == userName && user.Password == password);
        }

        /// <summary>
        /// Checks whether the user has already signed up or not and checks if he is admin.
        /// </summary>
        /// <param name="userName">Unique username of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Returns true is user already signed up or vice-versa and also checks if he is admin or not.</returns>
        internal bool IsUserAdmin(string userName, string password)
        {
            return this._users.Any(user => user.UserName == userName && user.Password == password && user.IsAdmin == true);
        }

        /// <summary>
        /// Identifies whether the admin is already present or not.
        /// </summary>
        /// <returns>Returns true if admin is present or not.</returns>
        internal bool IsAdminPresent()
        {
            return this._users.Any(user => user.IsAdmin == true);
        }


        /// <summary>
        /// Retrieves all registered users.
        /// </summary>
        /// <returns>List of all users.</returns>
        internal List<User> GetAllUsers()
        {
            return this._users.ToList();
        }

        /// <summary>
        /// Promotes a registered user to admin.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        /// <returns>Returns true if the user is promoted.</returns>
        internal bool PromoteUser(string userName)
        {
            User? user = this._users.FirstOrDefault(item => item.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            if (user == null || user.IsAdmin)
            {
                return false;
            }

            user.IsAdmin = true;
            this.WriteAll();
            return true;
        }

        private void WriteAll()
        {
            string fileData = JsonSerializer.Serialize(this._users, this._options);
            File.WriteAllText(this._filePath, fileData);
        }

        private List<User> LoadAll()
        {
            if (!File.Exists(this._filePath))
            {
                return new List<User>();
            }

            string fileData = File.ReadAllText(this._filePath);
            return JsonSerializer.Deserialize<List<User>>(fileData, this._options) ?? new List<User>();
        }
    }
}