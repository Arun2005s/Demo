using Timer.Interfaces;
using Timer.Models;

namespace Timer.Service
{
    /// <summary>
    /// Provides operations for retrieving, querying, and modifying historical task logs for users.
    /// </summary>
    public class TaskHistoryServices
    {
        private readonly ITaskRepository _taskRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskHistoryServices"/> class.
        /// </summary>
        /// <param name="taskRepository">The repository used to retrieve stored task items.</param>
        public TaskHistoryServices(ITaskRepository taskRepository)
        {
            this._taskRepository = taskRepository;
        }

        /// <summary>
        /// Retrieves all saved task logs for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of task records belonging to the user.</returns>
        public List<TaskItem> GetUserTasks(Guid userId)
        {
            return this._taskRepository.GetByUserId(userId);
        }

        /// <summary>
        /// Retrieves a single task record by its unique identifier.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task.</param>
        /// <returns>The matching task, or null if not found.</returns>
        public TaskItem? GetTaskById(Guid taskId)
        {
            return this._taskRepository.GetById(taskId);
        }

        /// <summary>
        /// Updates the recorded details of an existing historical task log.
        /// </summary>
        /// <param name="task">The modified task entity to update.</param>
        /// <returns>True if the update succeeded; otherwise, false.</returns>
        public bool UpdateTask(TaskItem task)
        {
            return this._taskRepository.Update(task);
        }

        /// <summary>
        /// Permanently removes a task log from historical records.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task log to delete.</param>
        /// <returns>True if the task was found and deleted; otherwise, false.</returns>
        public bool DeleteTask(Guid taskId)
        {
            return this._taskRepository.Delete(taskId);
        }
    }
}