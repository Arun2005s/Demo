using Spectre.Console;
using Timer.Helpers;
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
        private readonly CsvServices _csvServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskHistoryView"/> class.
        /// </summary>
        /// <param name="taskHistoryService">The service used to retrieve and modify user task logs.</param>
        /// <param name="timerService">The active timer service engine used to resume tasks.</param>
        /// <param name="csvServices">The import export service.</param>
        public TaskHistoryView(TaskHistoryServices taskHistoryService, TimerServices timerService, CsvServices csvServices)
        {
            this._taskHistoryService = taskHistoryService;
            this._timerService = timerService;
            this._csvServices = csvServices;
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
                NotificationHelper.Display();

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
                AnsiConsole.WriteLine();

                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Select an action:[/]")
                        .HighlightStyle(new Style(Color.Aqua))
                        .AddChoices(new[]
                        {
                    "Edit Name",
                    "Resume Task",
                    "Delete Log",
                    "Dashboard",
                        }));

                if (choice == "Dashboard")
                {
                    return false;
                }

                if (!InputHelper.TryPromptText(
                    $"Enter task number (1-{recentTasks.Count}):",
                    input => int.TryParse(input, out int number) && number >= 1 && number <= recentTasks.Count,
                    $"Enter a number between 1 and {recentTasks.Count}.",
                    out string selectedIndex))
                {
                    return false;
                }

                int index = int.Parse(selectedIndex);

                TaskItem selectedTask = recentTasks[index - 1];

                switch (choice)
                {
                    case "Edit Name":
                        this.HandleEdit(selectedTask);
                        break;

                    case "Delete Log":
                        this.HandleDelete(selectedTask);
                        break;

                    case "Resume Task":
                        if (this.HandleResume(selectedTask))
                        {
                            return true;
                        }

                        break;
                }
            }
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
            NotificationHelper.Display();

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
            NotificationHelper.Display();

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
            return DurationHelper.Format(duration);
        }
    }
}
