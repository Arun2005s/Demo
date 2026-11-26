using Timer.Models;

namespace Timer.Interfaces
{
    /// <summary>
    /// Defines the contract for data access operations related to <see cref="TaskItem"/> entities.
    /// </summary>
    public interface ITaskRepository
    {
        /// <summary>
        /// Adds a new task item to the repository.
        /// </summary>
        /// <param name="task">The <see cref="TaskItem"/> to add.</param>
        void Add(TaskItem task);

        /// <summary>
        /// Retrieves all task items associated with a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of <see cref="TaskItem"/> objects belonging to the specified user.</returns>
        List<TaskItem> GetByUserId(Guid userId);

        /// <summary>
        /// Retrieves a specific task item by its unique identifier.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task.</param>
        /// <returns>The matching <see cref="TaskItem"/>, or <see langword="null"/> if not found.</returns>
        TaskItem? GetById(Guid taskId);

        /// <summary>
        /// Updates an existing task item's recorded details.
        /// </summary>
        /// <param name="task">The modified <see cref="TaskItem"/> entity.</param>
        /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
        bool Update(TaskItem task);

        /// <summary>
        /// Permanently removes a task item from the repository by its unique identifier.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task to delete.</param>
        /// <returns><see langword="true"/> if the task was found and deleted; otherwise, <see langword="false"/>.</returns>
        bool Delete(Guid taskId);
    }
}
