using Spectre.Console;
using Timer.Helpers;
using Timer.Models;
using Timer.Service;

namespace Timer.View
{
    /// <summary>
    /// Analytical dashboard page
    /// </summary>
    public class AnalyticalDashboard
    {
        private readonly TaskHistoryServices _taskHistoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticalDashboard"/> class.
        /// </summary>
        /// <param name="taskHistoryService">The service used to retrieve and modify user task logs.</param>
        public AnalyticalDashboard(TaskHistoryServices taskHistoryService)
        {
            this._taskHistoryService = taskHistoryService;
        }

        /// <summary>
        /// Displays an analytical dashboard containing task statistics and work duration summaries using normal colors.
        /// </summary>
        /// <param name="user">The currently authenticated user.</param>
        public void ShowAnalyticsDashboard(User user)
        {
            AnsiConsole.Clear();
            NotificationHelper.Display();

            AnsiConsole.MarkupLine("[bold cyan]ANALYTICAL DASHBOARD[/]");
            AnsiConsole.Write(new Rule());

            List<TaskItem> tasks = this._taskHistoryService.GetUserTasks(user.UserId);

            if (tasks.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[grey]No task records available for analysis.[/]");
                this.WaitForUser();
                return;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
            DateOnly monthStart = new DateOnly(today.Year, today.Month, 1);

            List<TaskItem> todayTasks = tasks.Where(t => t.StartDate == today).ToList();
            List<TaskItem> weekTasks = tasks.Where(t => t.StartDate >= weekStart && t.StartDate <= today).ToList();
            List<TaskItem> monthTasks = tasks.Where(t => t.StartDate >= monthStart && t.StartDate <= today).ToList();

            int completedTasks = tasks.Count(t => t.EndDate.HasValue);
            int inProgressTasks = tasks.Count - completedTasks;

            TimeSpan totalWorked = TimeSpan.FromTicks(tasks.Sum(t => t.ElapsedTime.Ticks));
            TimeSpan totalExtended = TimeSpan.FromTicks(tasks.Sum(t => t.ExtendedTime.Ticks));
            TimeSpan totalTarget = TimeSpan.FromTicks(tasks.Sum(t => t.TargetTime.Ticks));

            TimeSpan todayWorked = TimeSpan.FromTicks(todayTasks.Sum(t => t.ElapsedTime.Ticks));
            TimeSpan weekWorked = TimeSpan.FromTicks(weekTasks.Sum(t => t.ElapsedTime.Ticks));
            TimeSpan monthWorked = TimeSpan.FromTicks(monthTasks.Sum(t => t.ElapsedTime.Ticks));

            TimeSpan averageWorked = completedTasks > 0
                ? TimeSpan.FromTicks(totalWorked.Ticks / completedTasks)
                : TimeSpan.Zero;

            TaskItem? longestTask = tasks.OrderByDescending(t => t.ElapsedTime).FirstOrDefault();

            double completionRate = tasks.Count > 0
                ? (double)completedTasks / tasks.Count * 100
                : 0;

            AnsiConsole.MarkupLine($"User: [cyan]{Markup.Escape(user.Name)}[/]");
            AnsiConsole.MarkupLine($"Report Date: [white]{today:dd-MM-yyyy}[/]\n");

            AnsiConsole.MarkupLine("[bold blue]TASK OVERVIEW[/]");

            var overviewTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Count").RightAligned());

            overviewTable.AddRow("Total Tasks", $"[white]{tasks.Count}[/]");
            overviewTable.AddRow("Completed Tasks", $"[green]{completedTasks}[/]");
            overviewTable.AddRow("In Progress Tasks", $"[yellow]{inProgressTasks}[/]");
            overviewTable.AddRow("Tasks Today", $"[cyan]{todayTasks.Count}[/]");
            overviewTable.AddRow("Tasks This Week", $"[white]{weekTasks.Count}[/]");
            overviewTable.AddRow("Tasks This Month", $"[white]{monthTasks.Count}[/]");
            overviewTable.AddRow("Completion Rate", $"[green]{completionRate:F2}%[/]");

            AnsiConsole.Write(overviewTable);
            AnsiConsole.MarkupLine("\n[bold blue]WORK DURATION ANALYSIS[/]");

            var durationTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Duration").RightAligned());

            durationTable.AddRow("Total Time Worked", $"[green]{this.FormatDuration(totalWorked)}[/]");
            durationTable.AddRow("Total Extended Time", $"[yellow]{this.FormatDuration(totalExtended)}[/]");
            durationTable.AddRow("Total Target Time", $"[cyan]{this.FormatDuration(totalTarget)}[/]");
            durationTable.AddRow("Average Time Per Completed Task", $"[white]{this.FormatDuration(averageWorked)}[/]");
            durationTable.AddRow("Time Worked Today", $"[white]{this.FormatDuration(todayWorked)}[/]");
            durationTable.AddRow("Time Worked This Week", $"[white]{this.FormatDuration(weekWorked)}[/]");
            durationTable.AddRow("Time Worked This Month", $"[white]{this.FormatDuration(monthWorked)}[/]");

            AnsiConsole.Write(durationTable);

            AnsiConsole.MarkupLine("\n[bold blue]TASK PERFORMANCE[/]");

            var performanceTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn("Metric")
                .AddColumn("Details");

            performanceTable.AddRow(
                "Longest Task",
                longestTask != null ? $"[cyan]{Markup.Escape(longestTask.TaskName)}[/]" : "[grey]-[/]");

            performanceTable.AddRow(
                "Longest Task Duration",
                longestTask != null ? $"[green]{this.FormatDuration(longestTask.ElapsedTime)}[/]" : "[grey]00:00:00[/]");

            AnsiConsole.Write(performanceTable);

            AnsiConsole.Write(new Rule());

            this.WaitForUser();
        }

        /// <summary>
        /// Waits for the user to press a key before continuing.
        /// </summary>
        private void WaitForUser()
        {
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
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
