using Timer.Models;

namespace Timer.Interfaces
{
    /// <summary>
    /// Defines the contract for data access operations related to <see cref="User"/> entities.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their unique email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>The matching <see cref="User"/> entity, or <see langword="null"/> if not found.</returns>
        User? GetByEmail(string email);

        /// <summary>
        /// Adds a new user to the repository.
        /// </summary>
        /// <param name="user">The <see cref="User"/> entity to add.</param>
        void Add(User user);
    }
}
