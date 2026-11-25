using System.Text.Json;
using Timer.Interfaces;
using Timer.Models;

namespace Timer.Repository
{
    /// <summary>
    /// Provides a JSON file-based implementation of <see cref="ITaskRepository"/> for managing <see cref="TaskItem"/> entities.
    /// </summary>
    public class JsonTaskRepository : ITaskRepository
    {
        private readonly string _filePath;
        private List<TaskItem> _tasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTaskRepository"/> class,
        /// ensuring the data directory exists and loading existing tasks from <c>tasks.json</c>.
        /// </summary>
        public JsonTaskRepository()
        {
            string dataFolder = Path.Combine(
                Directory.GetCurrentDirectory(), "Data");

            Directory.CreateDirectory(dataFolder);

            this._filePath = Path.Combine(dataFolder, "tasks.json");

            if (File.Exists(this._filePath))
            {
                string json = File.ReadAllText(this._filePath);

                this._tasks = JsonSerializer.Deserialize<List<TaskItem>>(json)
                              ?? new List<TaskItem>();
            }
            else
            {
                this._tasks = new List<TaskItem>();
                this.Save();
            }
        }

        /// <summary>
        /// Adds a new task item or updates it if it already exists (logic for resumed tasks), then persists changes.
        /// </summary>
        /// <param name="task">The <see cref="TaskItem"/> to add or update.</param>
        public void Add(TaskItem task)
        {
            int index = this._tasks.FindIndex(t => t.TaskId == task.TaskId);
            if (index >= 0)
            {
                this._tasks[index] = task;
            }
            else
            {
                this._tasks.Add(task);
            }

            this.Save();
        }

        /// <summary>
        /// Retrieves all task items associated with a specific user, ordered by most recent start time.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of <see cref="TaskItem"/> objects belonging to the specified user.</returns>
        public List<TaskItem> GetByUserId(Guid userId)
        {
            return this._tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartTime)
                .ToList();
        }

        /// <summary>
        /// Retrieves a specific task item by its unique identifier.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task.</param>
        /// <returns>The matching <see cref="TaskItem"/>, or <see langword="null"/> if not found.</returns>
        public TaskItem? GetById(Guid taskId)
        {
            return this._tasks.FirstOrDefault(t => t.TaskId == taskId);
        }

        /// <summary>
        /// Updates an existing task item's details in the in-memory list and writes changes to the JSON file.
        /// </summary>
        /// <param name="task">The modified <see cref="TaskItem"/> entity.</param>
        /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
        public bool Update(TaskItem task)
        {
            int index = this._tasks.FindIndex(t => t.TaskId == task.TaskId);
            if (index == -1)
            {
                return false;
            }

            this._tasks[index] = task;
            this.Save();
            return true;
        }

        /// <summary>
        /// Permanently removes a task item from the JSON file by its unique identifier.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task to delete.</param>
        /// <returns><see langword="true"/> if the task was found and deleted; otherwise, <see langword="false"/>.</returns>
        public bool Delete(Guid taskId)
        {
            int index = this._tasks.FindIndex(t => t.TaskId == taskId);
            if (index == -1)
            {
                return false;
            }

            this._tasks.RemoveAt(index);
            this.Save();
            return true;
        }

        /// <summary>
        /// Serializes the current list of tasks and writes it to the underlying JSON file.
        /// </summary>
        private void Save()
        {
            string json = JsonSerializer.Serialize(
                this._tasks,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            File.WriteAllText(this._filePath, json);
        }
    }
}
