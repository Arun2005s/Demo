using Timer.Interfaces;
using Timer.Models;

namespace Timer.Service
{
    /// <summary>
    /// Services for Import and export
    /// </summary>
    public class CsvServices
    {
        private readonly ITaskRepository _taskRepository;
        private readonly TaskHistoryServices _taskHistoryServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvServices"/> class.
        /// </summary>
        /// <param name="taskRepository">The repository used to retrieve stored task items.</param>
        /// <param name="taskHistoryServices">The Task History Services.</param>
        public CsvServices(ITaskRepository taskRepository, TaskHistoryServices taskHistoryServices)
        {
            this._taskRepository = taskRepository;
            this._taskHistoryServices = taskHistoryServices;
        }

        /// <summary>
        /// Exports task records for a specific user into a CSV file,
        /// optionally limited to the most recent entries.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="userName">The name of the user used for file naming.</param>
        /// <param name="limit">The maximum number of recent records to export.</param>
        /// <returns>The absolute file path of the exported CSV file.</returns>
        public string ExportTasksToCsv(Guid userId, string userName, int? limit = null)
        {
            List<TaskItem> tasks = this._taskHistoryServices.GetUserTasks(userId);

            if (limit.HasValue && limit.Value > 0)
            {
                tasks = tasks
                    .OrderByDescending(t => t.StartDate)
                    .ThenByDescending(t => t.StartTime)
                    .Take(limit.Value)
                    .ToList();
            }

            return this.SaveTasksToCsv(tasks, userName);
        }

        /// <summary>
        /// Exports task records within the specified EndDate range.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="userName">The name of the user used for file naming.</param>
        /// <param name="filterStartDate">The starting date of the filter range.</param>
        /// <param name="filterEndDate">The ending date of the filter range.</param>
        /// <returns>The absolute file path of the exported CSV file.</returns>
        public string ExportTasksToCsv(
            Guid userId,
            string userName,
            DateOnly filterStartDate,
            DateOnly filterEndDate)
        {
            List<TaskItem> tasks = this._taskHistoryServices.GetUserTasks(userId);

            tasks = tasks
                .Where(t => t.EndDate.HasValue &&
                    t.EndDate.Value >= filterStartDate &&
                    t.EndDate.Value <= filterEndDate)
                .OrderByDescending(t => t.EndDate)
                .ThenByDescending(t => t.EndTime)
                .ToList();

            return this.SaveTasksToCsv(tasks, userName);
        }

        /// <summary>
        /// Imports task logs from a CSV file and assigns them to the specified user.
        /// Existing tasks are skipped based on TaskId.
        /// </summary>
        /// <param name="userId">The unique identifier of the importing user.</param>
        /// <param name="filePath">The path to the source CSV file.</param>
        /// <returns>The number of valid, invalid, and duplicate records.</returns>
        public TaskImportResult ImportTasksFromCsv(Guid userId, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The specified CSV file could not be found.",
                    filePath);
            }

            TaskImportResult result = new TaskImportResult();

            HashSet<Guid> existingTaskIds = this._taskRepository
                .GetByUserId(userId)
                .Select(t => t.TaskId)
                .ToHashSet();

            using (StreamReader reader = new StreamReader(
                filePath,
                System.Text.Encoding.UTF8))
            {
                string? headerLine = reader.ReadLine();

                if (headerLine == null)
                {
                    result.InvalidCount++;
                    return result;
                }

                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    List<string> columns = new List<string>();
                    bool inQuotes = false;
                    int start = 0;

                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '"')
                        {
                            if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                            {
                                i++;
                            }
                            else
                            {
                                inQuotes = !inQuotes;
                            }
                        }
                        else if (line[i] == ',' && !inQuotes)
                        {
                            columns.Add(line.Substring(start, i - start));
                            start = i + 1;
                        }
                    }

                    columns.Add(line.Substring(start));

                    if (columns.Count != 10)
                    {
                        result.InvalidCount++;
                        continue;
                    }

                    try
                    {
                        if (!Guid.TryParse(columns[0].Trim(), out Guid importedTaskId) ||
                            importedTaskId == Guid.Empty)
                        {
                            result.InvalidCount++;
                            continue;
                        }

                        if (existingTaskIds.Contains(importedTaskId))
                        {
                            result.DuplicateCount++;
                            continue;
                        }

                        string rawTaskName = columns[1].Trim();

                        if (rawTaskName.StartsWith("\"") &&
                            rawTaskName.EndsWith("\"") &&
                            rawTaskName.Length >= 2)
                        {
                            rawTaskName = rawTaskName.Substring(1, rawTaskName.Length - 2)
                                .Replace("\"\"", "\"");
                        }

                        if (string.IsNullOrWhiteSpace(rawTaskName))
                        {
                            result.InvalidCount++;
                            continue;
                        }

                        DateOnly parsedStartDate = DateOnly.ParseExact(
                            columns[2].Trim(),
                            "yyyy-MM-dd");

                        TimeSpan parsedStartTime = TimeSpan.Parse(columns[4].Trim());

                        DateTime combinedStartTime = parsedStartDate.ToDateTime(
                            TimeOnly.FromTimeSpan(parsedStartTime));

                        DateOnly? parsedEndDate = null;
                        DateTime? combinedEndTime = null;

                        string rawEndDate = columns[3].Trim();
                        string rawEndTime = columns[5].Trim();

                        if (rawEndDate != "In Progress" && rawEndTime != "-")
                        {
                            parsedEndDate = DateOnly.ParseExact(
                                rawEndDate,
                                "yyyy-MM-dd");

                            TimeSpan parsedEndTimeValue = TimeSpan.Parse(rawEndTime);

                            combinedEndTime = parsedEndDate.Value.ToDateTime(
                                TimeOnly.FromTimeSpan(parsedEndTimeValue));
                        }
                        else if (rawEndDate != "In Progress" || rawEndTime != "-")
                        {
                            result.InvalidCount++;
                            continue;
                        }

                        TaskItem task = new TaskItem
                        {
                            TaskId = importedTaskId,
                            UserId = userId,
                            TaskName = rawTaskName,
                            StartDate = parsedStartDate,
                            EndDate = parsedEndDate,
                            StartTime = combinedStartTime,
                            EndTime = combinedEndTime,
                            TargetTime = TimeSpan.Parse(columns[6].Trim()),
                            ExtendedTime = TimeSpan.Parse(columns[7].Trim()),
                            ElapsedTime = TimeSpan.Parse(columns[8].Trim()),
                            TotalDuration = TimeSpan.Parse(columns[9].Trim()),
                        };

                        this._taskRepository.Add(task);

                        existingTaskIds.Add(importedTaskId);
                        result.ValidCount++;
                    }
                    catch
                    {
                        result.InvalidCount++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Formats and saves task records into a CSV file.
        /// </summary>
        /// <param name="tasks">The task records to export.</param>
        /// <param name="userName">The username used for file naming.</param>
        /// <returns>The absolute path of the saved CSV file.</returns>
        private string SaveTasksToCsv(List<TaskItem> tasks, string userName)
        {
            string exportsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Exports");

            Directory.CreateDirectory(exportsFolder);

            string safeName = string.Concat(
                userName.Split(Path.GetInvalidFileNameChars()));

            string fileName = $"{safeName}_task_history_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string fullPath = Path.Combine(exportsFolder, fileName);

            using (StreamWriter writer = new StreamWriter(
                fullPath,
                false,
                System.Text.Encoding.UTF8))
            {
                writer.WriteLine(
                    "TaskId,TaskName,StartDate,EndDate,StartTime,EndTime,TargetTime,ExtendedTime,ElapsedTime,TotalDuration");

                foreach (TaskItem task in tasks)
                {
                    string safeTaskName = task.TaskName ?? string.Empty;

                    if (safeTaskName.Contains(",") ||
                        safeTaskName.Contains("\"") ||
                        safeTaskName.Contains("\n") ||
                        safeTaskName.Contains("\r"))
                    {
                        safeTaskName = $"\"{safeTaskName.Replace("\"", "\"\"")}\"";
                    }

                    writer.WriteLine(
                        $"{task.TaskId}," +
                        $"{safeTaskName}," +
                        $"{task.StartDate:yyyy-MM-dd}," +
                        $"{(task.EndDate.HasValue ? task.EndDate.Value.ToString("yyyy-MM-dd") : "In Progress")}," +
                        $"{task.StartTime:HH:mm:ss}," +
                        $"{(task.EndTime.HasValue ? task.EndTime.Value.ToString("HH:mm:ss") : "-")}," +
                        $"{task.TargetTime}," +
                        $"{task.ExtendedTime}," +
                        $"{task.ElapsedTime}," +
                        $"{task.TotalDuration}");
                }
            }

            return fullPath;
        }
    }
}
