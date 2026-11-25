using System.Text.Json;
using Timer.Interfaces;
using Timer.Models;

namespace Timer.Repository
{
    /// <summary>
    /// Provides a JSON file-based implementation of <see cref="IUserRepository"/> for managing <see cref="User"/> entities.
    /// </summary>
    public class JsonUserRepository : IUserRepository
    {
        private readonly string _filePath;
        private List<User> _users;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonUserRepository"/> class,
        /// ensuring the data directory exists and loading existing users from <c>users.json</c>.
        /// </summary>
        public JsonUserRepository()
        {
            string dataFolder = Path.Combine(
                Directory.GetCurrentDirectory(), "Data");

            Directory.CreateDirectory(dataFolder);

            this._filePath = Path.Combine(dataFolder, "users.json");

            if (File.Exists(this._filePath))
            {
                string json = File.ReadAllText(this._filePath);

                this._users = JsonSerializer.Deserialize<List<User>>(json)
                              ?? new List<User>();
            }
            else
            {
                this._users = new List<User>();
                this.Save();
            }
        }

        /// <summary>
        /// Retrieves a user by their unique email address using a case-insensitive comparison.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>The matching <see cref="User"/> entity, or <see langword="null"/> if not found.</returns>
        public User? GetByEmail(string email)
        {
            return this._users.FirstOrDefault(
                u => u.Email.Equals(
                    email,
                    System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds a new user to the in-memory collection and persists the changes to the JSON file.
        /// </summary>
        /// <param name="user">The <see cref="User"/> entity to add.</param>
        public void Add(User user)
        {
            this._users.Add(user);
            this.Save();
        }

        /// <summary>
        /// Serializes the current list of users and writes it to the underlying JSON file.
        /// </summary>
        private void Save()
        {
            string json = JsonSerializer.Serialize(
                this._users,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            File.WriteAllText(this._filePath, json);
        }
    }
}
