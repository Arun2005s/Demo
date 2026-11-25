
using System.Globalization;
using Spectre.Console;
using Timer.Models;
using Timer.Service;
using Timer.Services;

namespace Timer.Views
{
    /// <summary>
    /// Provides the console-based user interface for viewing, editing, deleting, and resuming recorded task time logs.
    /// </summary>
    public class TaskHistoryView
    {
        private const int MaxRecentLogs = 10;
        private readonly TaskHistoryServices _taskHistoryService;
        private readonly TimerServices _timerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskHistoryView"/> class.
        /// </summary>
        /// <param name="taskHistoryService">The service used to retrieve and modify user task logs.</param>
        /// <param name="timerService">The active timer service engine used to resume tasks.</param>
        public TaskHistoryView(TaskHistoryServices taskHistoryService, TimerServices timerService)
        {
            this._taskHistoryService = taskHistoryService;
            this._timerService = timerService;
        }

        /// <summary>
        /// Displays the last 10 recorded task logs and provides options to edit, delete, or resume tasks.
        /// </summary>
        /// <param name="user">The currently authenticated user.</param>
        /// <returns>True if a task was resumed; otherwise, false.</returns>
        public bool ShowLogs(User user)
        {
            while (true)
            {
                AnsiConsole.Clear();

                AnsiConsole.MarkupLine("[bold]TASK HISTORY[/]");
                AnsiConsole.Write(new Rule());

                AnsiConsole.MarkupLine(
                    $"User: {Markup.Escape(user.Name)} ({Markup.Escape(user.Email)})");

                List<TaskItem> allTasks = this._taskHistoryService
                    .GetUserTasks(user.UserId)
                    .OrderByDescending(t => t.StartDate)
                    .ThenByDescending(t => t.StartTime)
                    .ToList();

                if (allTasks.Count == 0)
                {
                    AnsiConsole.MarkupLine("\n[grey]No task logs found yet.[/]");
                    this.WaitForUser();
                    return false;
                }

                List<TaskItem> recentTasks = allTasks.Take(MaxRecentLogs).ToList();

                this.RenderTaskTable(recentTasks, allTasks.Count);

                AnsiConsole.MarkupLine(
                    "\n[grey]E: Edit Name   D: Delete Log   R: Resume Task   B: Back[/]");

                string choice = AnsiConsole.Prompt(
                    new TextPrompt<string>("Select an action:")
                        .Validate(input =>
                            new[] { "E", "D", "R", "B" }.Contains(input.Trim().ToUpperInvariant())
                                ? ValidationResult.Success()
                                : ValidationResult.Error("Enter E, D, R, or B.")));

                choice = choice.Trim().ToUpperInvariant();

                if (choice == "B")
                {
                    return false;
                }

                Console.Write($"Enter task number (1-{recentTasks.Count}): ");

                if (int.TryParse(Console.ReadLine(), out int index) &&
                    index >= 1 &&
                    index <= recentTasks.Count)
                {
                    TaskItem selectedTask = recentTasks[index - 1];

                    switch (choice)
                    {
                        case "E":
                            this.HandleEdit(selectedTask);
                            break;

                        case "D":
                            this.HandleDelete(selectedTask);
                            break;

                        case "R":
                            if (this.HandleResume(selectedTask))
                            {
                                return true;
                            }

                            break;
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid task number.[/]");
                    this.WaitForUser();
                }
            }
        }

        /// <summary>
        /// Exports the user's task history to a CSV file using a record limit or date range.
        /// </summary>
        /// <param name="user">The currently authenticated user whose records are being exported.</param>
        public void TriggerHistoryExport(User user)
        {
            AnsiConsole.Clear();

            AnsiConsole.MarkupLine("[bold]EXPORT HISTORY DATA[/]");
            AnsiConsole.Write(new Rule());

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select export mode:")
                    .AddChoices(
                        "Export recent records",
                        "Export by date range",
                        "Back"));

            string destinationPath = string.Empty;

            try
            {
                switch (choice)
                {
                    case "Export recent records":
                        string input = AnsiConsole.Prompt(
                            new TextPrompt<string>(
                                "Enter the number of recent records (leave empty for all):")
                                .DefaultValue(string.Empty)
                                .AllowEmpty());

                        int? limit = null;

                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            if (int.TryParse(input, out int parsedLimit) && parsedLimit > 0)
                            {
                                limit = parsedLimit;
                            }
                            else
                            {
                                AnsiConsole.MarkupLine(
                                    "[yellow]Invalid number. Exporting all records instead.[/]");
                            }
                        }

                        destinationPath = this._taskHistoryService.ExportTasksToCsv(
                            user.UserId,
                            user.Name,
                            limit);

                        break;

                    case "Export by date range":
                        if (!DateOnly.TryParseExact(
                            AnsiConsole.Ask<string>("Enter start date (yyyy-MM-dd):"),
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateOnly startDate))
                        {
                            AnsiConsole.MarkupLine("[red]Invalid start date.[/]");
                            this.WaitForUser();
                            return;
                        }

                        if (!DateOnly.TryParseExact(
                            AnsiConsole.Ask<string>("Enter end date (yyyy-MM-dd):"),
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateOnly endDate))
                        {
                            AnsiConsole.MarkupLine("[red]Invalid end date.[/]");
                            this.WaitForUser();
                            return;
                        }

                        if (startDate > endDate)
                        {
                            AnsiConsole.MarkupLine(
                                "[yellow]Start date cannot be after end date.[/]");

                            this.WaitForUser();
                            return;
                        }

                        destinationPath = this._taskHistoryService.ExportTasksToCsv(
                            user.UserId,
                            user.Name,
                            startDate,
                            endDate);

                        break;

                    case "Back":
                        return;
                }

                AnsiConsole.MarkupLine("\n[green]Export completed successfully.[/]");
                AnsiConsole.MarkupLine(
                    $"File location: {Markup.Escape(destinationPath)}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(
                    $"[red]Export failed: {Markup.Escape(ex.Message)}[/]");
            }

            this.WaitForUser();
        }

        /// <summary>
        /// Imports task history records from a CSV file and displays the import results.
        /// </summary>
        /// <param name="user">The user under whose account the records will be imported.</param>
        public void ImportData(User user)
        {
            AnsiConsole.Clear();

            AnsiConsole.MarkupLine("[bold]IMPORT HISTORY DATA[/]");
            AnsiConsole.Write(new Rule());

            AnsiConsole.MarkupLine(
                $"Target user: {Markup.Escape(user.Name)} ({Markup.Escape(user.Email)})");

            string importPath = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the CSV file path:")
                    .Validate(input =>
                        !string.IsNullOrWhiteSpace(input)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("File path cannot be empty.")));

            importPath = importPath.Trim('"', '\'');

            if (!File.Exists(importPath))
            {
                AnsiConsole.MarkupLine("[red]The specified file could not be found.[/]");
                this.WaitForUser();
                return;
            }

            if (!string.Equals(
                Path.GetExtension(importPath),
                ".csv",
                StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[red]Please select a CSV file.[/]");
                this.WaitForUser();
                return;
            }

            try
            {
                TaskImportResult result = this._taskHistoryService.ImportTasksFromCsv(
                    user.UserId,
                    importPath);

                AnsiConsole.MarkupLine("\n[bold]IMPORT SUMMARY[/]");
                AnsiConsole.Write(new Rule());

                var table = new Table()
                    .Border(TableBorder.Simple)
                    .AddColumn("Result")
                    .AddColumn("Count");

                table.AddRow(
                    "[green]Valid / Imported[/]",
                    result.ValidCount.ToString());

                table.AddRow(
                    "[red]Invalid[/]",
                    result.InvalidCount.ToString());

                table.AddRow(
                    "[yellow]Duplicated / Skipped[/]",
                    result.DuplicateCount.ToString());

                AnsiConsole.Write(table);

                if (result.ValidCount > 0)
                {
                    AnsiConsole.MarkupLine(
                        "\n[green]Import completed successfully.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        "\n[yellow]No new valid records were imported.[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(
                    $"[red]Import failed: {Markup.Escape(ex.Message)}[/]");
            }

            this.WaitForUser();
        }

        /// <summary>
        /// Displays an analytical dashboard containing task statistics and work duration summaries.
        /// </summary>
        /// <param name="user">The currently authenticated user.</param>
        public void ShowAnalyticsDashboard(User user)
        {
            AnsiConsole.Clear();

            AnsiConsole.MarkupLine("[bold]ANALYTICAL DASHBOARD[/]");
            AnsiConsole.Write(new Rule());

            List<TaskItem> tasks = this._taskHistoryService.GetUserTasks(user.UserId);

            if (tasks.Count == 0)
            {
                AnsiConsole.MarkupLine(
                    "\n[grey]No task records available for analysis.[/]");

                this.WaitForUser();
                return;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly weekStart = today.AddDays(
                -(((int)today.DayOfWeek + 6) % 7));

            DateOnly monthStart = new DateOnly(
                today.Year,
                today.Month,
                1);

            List<TaskItem> todayTasks = tasks
                .Where(t => t.StartDate == today)
                .ToList();

            List<TaskItem> weekTasks = tasks
                .Where(t => t.StartDate >= weekStart && t.StartDate <= today)
                .ToList();

            List<TaskItem> monthTasks = tasks
                .Where(t => t.StartDate >= monthStart && t.StartDate <= today)
                .ToList();

            int completedTasks = tasks.Count(t => t.EndDate.HasValue);
            int inProgressTasks = tasks.Count - completedTasks;

            TimeSpan totalWorked = TimeSpan.FromTicks(
                tasks.Sum(t => t.ElapsedTime.Ticks));

            TimeSpan totalExtended = TimeSpan.FromTicks(
                tasks.Sum(t => t.ExtendedTime.Ticks));

            TimeSpan totalTarget = TimeSpan.FromTicks(
                tasks.Sum(t => t.TargetTime.Ticks));

            TimeSpan todayWorked = TimeSpan.FromTicks(
                todayTasks.Sum(t => t.ElapsedTime.Ticks));

            TimeSpan weekWorked = TimeSpan.FromTicks(
                weekTasks.Sum(t => t.ElapsedTime.Ticks));

            TimeSpan monthWorked = TimeSpan.FromTicks(
                monthTasks.Sum(t => t.ElapsedTime.Ticks));

            TimeSpan averageWorked = completedTasks > 0
                ? TimeSpan.FromTicks(
                    totalWorked.Ticks / completedTasks)
                : TimeSpan.Zero;

            TaskItem? longestTask = tasks
                .OrderByDescending(t => t.ElapsedTime)
                .FirstOrDefault();

            double completionRate = tasks.Count > 0
                ? (double)completedTasks / tasks.Count * 100
                : 0;

            AnsiConsole.MarkupLine(
                $"User: [cyan]{Markup.Escape(user.Name)}[/]");

            AnsiConsole.MarkupLine(
                $"Report Date: [cyan]{today:dd-MM-yyyy}[/]\n");

            AnsiConsole.MarkupLine("[bold]TASK OVERVIEW[/]");

            var overviewTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Count").RightAligned());

            overviewTable.AddRow("Total Tasks", tasks.Count.ToString());
            overviewTable.AddRow("Completed Tasks", completedTasks.ToString());
            overviewTable.AddRow("In Progress Tasks", inProgressTasks.ToString());
            overviewTable.AddRow("Tasks Today", todayTasks.Count.ToString());
            overviewTable.AddRow("Tasks This Week", weekTasks.Count.ToString());
            overviewTable.AddRow("Tasks This Month", monthTasks.Count.ToString());
            overviewTable.AddRow("Completion Rate", $"{completionRate:F2}%");

            AnsiConsole.Write(overviewTable);

            AnsiConsole.MarkupLine("\n[bold]WORK DURATION ANALYSIS[/]");

            var durationTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Duration").RightAligned());

            durationTable.AddRow(
                "Total Time Worked",
                this.FormatDuration(totalWorked));

            durationTable.AddRow(
                "Total Extended Time",
                this.FormatDuration(totalExtended));

            durationTable.AddRow(
                "Total Target Time",
                this.FormatDuration(totalTarget));

            durationTable.AddRow(
                "Average Time Per Completed Task",
                this.FormatDuration(averageWorked));

            durationTable.AddRow(
                "Time Worked Today",
                this.FormatDuration(todayWorked));

            durationTable.AddRow(
                "Time Worked This Week",
                this.FormatDuration(weekWorked));

            durationTable.AddRow(
                "Time Worked This Month",
                this.FormatDuration(monthWorked));

            AnsiConsole.Write(durationTable);

            AnsiConsole.MarkupLine("\n[bold]TASK PERFORMANCE[/]");

            var performanceTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Metric")
                .AddColumn("Details");

            performanceTable.AddRow(
                "Longest Task",
                longestTask != null
                    ? Markup.Escape(longestTask.TaskName)
                    : "-");

            performanceTable.AddRow(
                "Longest Task Duration",
                longestTask != null
                    ? this.FormatDuration(longestTask.ElapsedTime)
                    : "00:00:00");

            AnsiConsole.Write(performanceTable);

            AnsiConsole.Write(new Rule());

            AnsiConsole.MarkupLine(
                "[grey]Week starts on Monday. Statistics are based on task start dates.[/]");

            this.WaitForUser();
        }

        /// <summary>
        /// Displays the task history in a formatted table with duration totals.
        /// </summary>
        /// <param name="recentTasks">The list of recent tasks to display.</param>
        /// <param name="totalTaskCount">The total number of tasks available for the user.</param>
        private void RenderTaskTable(List<TaskItem> recentTasks, int totalTaskCount)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Expand()
                .AddColumn(new TableColumn("#").Centered())
                .AddColumn("Task Name")
                .AddColumn(new TableColumn("Start Date").Centered())
                .AddColumn(new TableColumn("End Date").Centered())
                .AddColumn(new TableColumn("Start Time").Centered())
                .AddColumn(new TableColumn("End Time").Centered())
                .AddColumn(new TableColumn("Target").Centered())
                .AddColumn(new TableColumn("Extended").Centered())
                .AddColumn(new TableColumn("Worked").Centered())
                .AddColumn(new TableColumn("Total").Centered());

            for (int i = 0; i < recentTasks.Count; i++)
            {
                TaskItem task = recentTasks[i];

                string taskName = task.TaskName.Length > 18
                    ? task.TaskName.Substring(0, 15) + "..."
                    : task.TaskName;

                table.AddRow(
                    (i + 1).ToString(),
                    Markup.Escape(taskName),
                    task.StartDate.ToString("dd-MM-yyyy"),
                    task.EndDate?.ToString("dd-MM-yyyy") ?? "[yellow]In Progress[/]",
                    task.StartTime.ToString("hh:mm:ss tt"),
                    task.EndTime?.ToString("hh:mm:ss tt") ?? "-",
                    this.FormatDuration(task.TargetTime),
                    this.FormatDuration(task.ExtendedTime),
                    this.FormatDuration(task.ElapsedTime),
                    this.FormatDuration(task.TotalDuration));
            }

            AnsiConsole.Write(table);

            AnsiConsole.Write(new Rule());

            AnsiConsole.MarkupLine(
                $"Showing: {recentTasks.Count} of {totalTaskCount} total tasks");

            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Allows the user to edit the name of a selected task and saves the changes.
        /// </summary>
        /// <param name="task">The task whose name will be edited.</param>
        private void HandleEdit(TaskItem task)
        {
            AnsiConsole.Clear();

            AnsiConsole.MarkupLine("[bold]EDIT TASK NAME[/]");
            AnsiConsole.Write(new Rule());

            AnsiConsole.MarkupLine(
                $"Current name: {Markup.Escape(task.TaskName)}");

            string newName = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the new task name (leave empty to cancel):")
                    .DefaultValue(string.Empty)
                    .AllowEmpty());

            newName = newName.Trim();

            if (!string.IsNullOrWhiteSpace(newName))
            {
                task.TaskName = newName;

                this._taskHistoryService.UpdateTask(task);

                if (this._timerService.ActiveTask != null &&
                    this._timerService.ActiveTask.TaskId == task.TaskId)
                {
                    this._timerService.ActiveTask.TaskName = newName;
                }

                AnsiConsole.MarkupLine(
                    "[green]Task name updated successfully.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No changes made.[/]");
            }

            this.WaitForUser();
        }

        /// <summary>
        /// Confirms and deletes a selected task from the user's history.
        /// Prevents deletion of a task that is currently active.
        /// </summary>
        /// <param name="task">The task to be deleted.</param>
        private void HandleDelete(TaskItem task)
        {
            if (this._timerService.ActiveTask != null &&
                this._timerService.ActiveTask.TaskId == task.TaskId)
            {
                AnsiConsole.MarkupLine(
                    "[red]Cannot delete an active task. Stop and save it first.[/]");

                this.WaitForUser();
                return;
            }

            bool confirmDelete = AnsiConsole.Confirm(
                $"Are you sure you want to delete '{Markup.Escape(task.TaskName)}'?");

            if (confirmDelete)
            {
                this._taskHistoryService.DeleteTask(task.TaskId);

                AnsiConsole.MarkupLine(
                    "[green]Task deleted successfully.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Deletion cancelled.[/]");
            }

            this.WaitForUser();
        }

        /// <summary>
        /// Resumes a selected historical task using the active timer service.
        /// Prompts the user to save another active task before resuming.
        /// </summary>
        /// <param name="task">The historical task to resume.</param>
        /// <returns>True if the task was resumed or was already active; otherwise, false.</returns>
        private bool HandleResume(TaskItem task)
        {
            if (this._timerService.ActiveTask != null)
            {
                if (this._timerService.ActiveTask.TaskId == task.TaskId)
                {
                    AnsiConsole.MarkupLine(
                        "[yellow]This task is already active.[/]");

                    this.WaitForUser();
                    return true;
                }

                bool stopCurrentTask = AnsiConsole.Confirm(
                    $"'{Markup.Escape(this._timerService.ActiveTask.TaskName)}' is active. Stop and save it before resuming '{Markup.Escape(task.TaskName)}'?");

                if (!stopCurrentTask)
                {
                    AnsiConsole.MarkupLine("[yellow]Resume cancelled.[/]");
                    this.WaitForUser();
                    return false;
                }

                this._timerService.StopAndSaveTask();
            }

            AnsiConsole.Clear();

            AnsiConsole.MarkupLine("[bold]RESUMING HISTORICAL TASK[/]");
            AnsiConsole.Write(new Rule());

            AnsiConsole.MarkupLine(
                $"Task: {Markup.Escape(task.TaskName)}");

            AnsiConsole.MarkupLine(
                $"Previously logged time: {this.FormatDuration(task.ElapsedTime)}");

            this._timerService.ResumeExistingTask(task);

            AnsiConsole.MarkupLine(
                "\n[green]Task resumed successfully.[/]");

            this.WaitForUser();
            return true;
        }

        /// <summary>
        /// Waits for the user to press a key before continuing.
        /// </summary>
        private void WaitForUser()
        {
            AnsiConsole.MarkupLine(
                "\n[grey]Press any key to continue...[/]");

            Console.ReadKey(true);
        }

        /// <summary>
        /// Formats a <see cref="TimeSpan"/> into a standard HH:mm:ss display string.
        /// </summary>
        /// <param name="duration">The duration to format.</param>
        /// <returns>The formatted duration string.</returns>
        private string FormatDuration(TimeSpan duration)
        {
            return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }
}
