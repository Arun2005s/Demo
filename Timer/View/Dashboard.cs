using Spectre.Console;
using Timer.Helpers;
using Timer.Models;
using Timer.Services;
using Timer.View;

namespace Timer.Views
{
    /// <summary>
    /// Provides the console-based dashboard for creating tasks, tracking time, viewing task history, and managing task records.
    /// </summary>
    public class Dashboard
    {
        private readonly TimerServices _timerService;
        private readonly TaskHistoryView _logView;
        private readonly ExportView _exportView;
        private readonly ImportView _importView;
        private readonly TaskView _taskView;
        private readonly AnalyticalDashboard _analyticalDashboard;
        private readonly ProfileView _profileView;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dashboard"/> class.
        /// </summary>
        /// <param name="timerService">The service used to manage task creation, timing, and persistence.</param>
        /// <param name="logView">The view responsible for displaying and managing task history.</param>
        /// <param name="exportView">The view responsible for displaying and managing Export history.</param>
        /// <param name="importView">The view responsible for displaying and managing Import history.</param>
        /// <param name="taskView">The view responsible for displaying and managing Task timer.</param>
        /// <param name="profileView">The view responsible for updating profile.</param>
        /// <param name="analyticalDashboard">The view responsible for displaying and managing analytical dashboard history.</param>
        public Dashboard(TimerServices timerService, TaskHistoryView logView, ExportView exportView, ImportView importView, AnalyticalDashboard analyticalDashboard, TaskView taskView, ProfileView profileView)
        {
            this._timerService = timerService;
            this._logView = logView;
            this._exportView = exportView;
            this._importView = importView;
            this._analyticalDashboard = analyticalDashboard;
            this._taskView = taskView;
            this._profileView = profileView;
        }

        /// <summary>
        /// Displays the main dashboard menu and handles navigation for the logged-in user.
        /// </summary>
        /// <param name="user">The currently authenticated user.</param>
        public void ShowDashboard(User user)
        {
            // Define the target reached event inline to print right below the menu layout cleanly without scrambling formatting
            TargetTimeReachedHandler targetReachedHandler = task =>
            {
                NotificationHelper.Queue("target", $"Target reached: {task.TaskName}");
                NotificationHelper.Display();
            };

            // Using closure to ensure background reminder checks use the exact, mutated reference loop object
            DailyWorkReminderHandler internalReminderHandler = monitoredUser => this.HandleDailyWorkReminder(monitoredUser);

            this._timerService.OnDailyWorkReminder += internalReminderHandler;
            this._timerService.OnTargetTimeReached += targetReachedHandler;
            this._timerService.StartDailyReminderMonitor(user);
            this._timerService.StartTargetTimeMonitor();

            try
            {
                while (true)
                {
                    AnsiConsole.Clear();
                    NotificationHelper.Display();

                    this.RenderDashboard(user);

                    List<string> choices = new List<string>
                    {
                        this._timerService.ActiveTask != null
                            ? "Return to Active Task"
                            : "New Task",
                        "Task History",
                        "Export Data",
                        "Import Logs",
                        "Analytical Dashboard",
                        "Profile Settings",
                        "Logout",
                    };

                    string choice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select an option")
                            .AddChoices(choices));

                    switch (choice)
                    {
                        case "Return to Active Task":
                            if (this._timerService.ActiveTask != null)
                            {
                                bool startNextImmediately = this._taskView.ExecuteTimerWorkflow(
                                    this._timerService.ActiveTask);

                                if (startNextImmediately)
                                {
                                    this._taskView.CreateAndRunTask(user);
                                }
                            }

                            break;

                        case "New Task":
                            this._taskView.CreateAndRunTask(user);
                            break;

                        case "Task History":
                            bool resumedFromHistory = this._logView.ShowLogs(user);

                            if (resumedFromHistory && this._timerService.ActiveTask != null)
                            {
                                bool startNextImmediately = this._taskView.ExecuteTimerWorkflow(
                                    this._timerService.ActiveTask);

                                if (startNextImmediately)
                                {
                                    this._taskView.CreateAndRunTask(user);
                                }
                            }

                            break;

                        case "Export Data":
                            this._exportView.TriggerHistoryExport(user);
                            break;

                        case "Import Logs":
                            this._importView.ImportData(user);
                            break;

                        case "Analytical Dashboard":
                            this._analyticalDashboard.ShowAnalyticsDashboard(user);
                            break;

                        case "Profile Settings":
                            this._profileView.ShowProfileMenu(user);
                            break;

                        case "Logout":
                            if (this._timerService.ActiveTask != null)
                            {
                                string logoutChoice = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("An active task is running. Select an option:")
                                        .AddChoices(
                                            "Save task and logout",
                                            "Logout without saving",
                                            "Return to dashboard"));

                                if (logoutChoice == "Save task and logout")
                                {
                                    this._timerService.StopAndSaveTask();
                                    return;
                                }

                                if (logoutChoice == "Logout without saving")
                                {
                                    return;
                                }

                                break;
                            }

                            return;
                    }
                }
            }
            finally
            {
                this._timerService.StopDailyReminderMonitor();
                this._timerService.StopTargetTimeMonitor();
                this._timerService.OnDailyWorkReminder -= internalReminderHandler;
                this._timerService.OnTargetTimeReached -= targetReachedHandler;
            }
        }

        /// <summary>
        /// Displays the dashboard information, including user details and the current task status.
        /// </summary>
        /// <param name="user">The currently authenticated user.</param>
        private void RenderDashboard(User user)
        {
            AnsiConsole.MarkupLine("[bold]TASK TIMER[/]");
            AnsiConsole.Write(new Rule());

            var userTable = new Table()
                .Border(TableBorder.Simple)
                .AddColumn("User")
                .AddColumn("Email")
                .AddColumn("Daily Start");

            userTable.AddRow(
                Markup.Escape(user.Name),
                Markup.Escape(user.Email),
                user.DayStartTime.ToString("hh:mm tt"));

            AnsiConsole.Write(userTable);

            TaskItem? activeTask = this._timerService.ActiveTask;

            AnsiConsole.MarkupLine("\n[bold]TASK STATUS[/]");

            if (activeTask != null)
            {
                TimeSpan currentElapsed = this._timerService.GetElapsedTime();
                TimeSpan effectiveTarget = activeTask.TargetTime + activeTask.ExtendedTime;

                string status = this._timerService.IsRunning
                    ? "[green]RUNNING[/]"
                    : "[yellow]PAUSED[/]";

                AnsiConsole.MarkupLine($"Task: {Markup.Escape(activeTask.TaskName)}");
                AnsiConsole.MarkupLine($"Status: {status}");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"Elapsed time: {currentElapsed:hh\\:mm\\:ss}");
                AnsiConsole.MarkupLine($"Target time: {activeTask.TargetTime:hh\\:mm\\:ss}");
                AnsiConsole.MarkupLine($"Extended time: {activeTask.ExtendedTime:hh\\:mm\\:ss}");
                AnsiConsole.MarkupLine($"Effective target: {effectiveTarget:hh\\:mm\\:ss}");

                if (this._timerService.IsTargetReached)
                {
                    AnsiConsole.MarkupLine("[bold red]Target time reached![/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[grey]No active task.[/]");
            }

            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Queues a daily work reminder for display on the dashboard.
        /// </summary>
        /// <param name="user">The user receiving the reminder.</param>
        private void HandleDailyWorkReminder(User user)
        {
            NotificationHelper.Queue(
                "daily",
                $"Good day, {user.Name}! It is your scheduled work start time ({user.DayStartTime:hh:mm tt}).");
            NotificationHelper.Display();
        }
    }
}
