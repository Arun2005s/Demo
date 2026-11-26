using Spectre.Console;
using Timer.Helpers;
using Timer.Models;
using Timer.Services;

namespace Timer.View
{
    /// <summary>
    /// Creating new task and managing task
    /// </summary>
    public class TaskView
    {
        private readonly TimerServices _timerService;
        private volatile bool _targetReachedBannerShown;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskView"/> class.
        /// </summary>
        /// <param name="timerService">The service used to manage task creation, timing, and persistence.</param>
        public TaskView(TimerServices timerService)
        {
            this._timerService = timerService;
        }

        /// <summary>
        /// Prompts the user for task details and target duration, then starts the task.
        /// </summary>
        /// <param name="user">The user creating the task.</param>
        public void CreateAndRunTask(User user)
        {
            while (true)
            {
                AnsiConsole.Clear();
                NotificationHelper.Display();

                AnsiConsole.MarkupLine("[bold]CREATE NEW TASK[/]");
                AnsiConsole.Write(new Rule());

                if (!InputHelper.TryPromptText("Enter task name:", input => !string.IsNullOrWhiteSpace(input), "Task name cannot be empty.", out string taskName))
                {
                    return;
                }

                AnsiConsole.MarkupLine(
                    "\n[grey]Enter the target duration. Press Enter to use 0.[/]\n");

                if (!this.TryReadDurationUnit("Hours", 100000, out int hours) ||
                    !this.TryReadDurationUnit("Minutes", 59, out int minutes) ||
                    !this.TryReadDurationUnit("Seconds", 59, out int seconds))
                {
                    AnsiConsole.MarkupLine(
                        "[red]Invalid duration. Please enter valid non-negative numbers.[/]");

                    this.WaitForUser();
                    return;
                }

                TimeSpan targetTime =
                    TimeSpan.FromHours(hours) +
                    TimeSpan.FromMinutes(minutes) +
                    TimeSpan.FromSeconds(seconds);

                AnsiConsole.MarkupLine("\n[bold]TASK SUMMARY[/]");
                AnsiConsole.MarkupLine($"Task: {Markup.Escape(taskName)}");
                AnsiConsole.MarkupLine($"Target duration: {this.FormatDuration(targetTime)}");

                AnsiConsole.MarkupLine("\nPress Enter to start the task...");
                Console.ReadLine();

                TaskItem activeTask = this._timerService.StartNewTask(
                    user.UserId,
                    taskName,
                    targetTime);

                bool startNextTask = this.ExecuteTimerWorkflow(activeTask);

                if (!startNextTask)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Attaches the target notification event and starts the live timer workflow.
        /// </summary>
        /// <param name="task">The task being monitored.</param>
        /// <returns>True if the caller should create another task; otherwise, false.</returns>
        public bool ExecuteTimerWorkflow(TaskItem task)
        {
            TimeSpan effectiveTarget = task.TargetTime + task.ExtendedTime;

            this._targetReachedBannerShown =
                effectiveTarget > TimeSpan.Zero &&
                this._timerService.GetElapsedTime() >= effectiveTarget;

            this._timerService.OnTargetTimeReached += this.HandleTargetTimeReached;

            try
            {
                return this.RunLiveTimer(task);
            }
            finally
            {
                this._timerService.OnTargetTimeReached -= this.HandleTargetTimeReached;
            }
        }

        /// <summary>
        /// Reads and validates a non-negative integer duration value.
        /// </summary>
        /// <param name="unit">The duration unit displayed in the prompt.</param>
        /// <param name="maximum">The maximum permitted value.</param>
        /// <param name="value">The parsed duration value.</param>
        /// <returns>True if the input is valid; otherwise, false.</returns>
        private bool TryReadDurationUnit(string unit, int maximum, out int value)
        {
            string input = AnsiConsole.Prompt(
                new TextPrompt<string>($"{unit}:")
                    .DefaultValue("0")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(input))
            {
                value = 0;
                return true;
            }

            return int.TryParse(input, out value) &&
                   value >= 0 &&
                   value <= maximum;
        }

        /// <summary>
        /// Runs the live timer and handles pause, resume, extension, stop, and dashboard navigation.
        /// </summary>
        /// <param name="task">The active task being timed.</param>
        /// <returns>True if the user chooses to start another task; otherwise, false.</returns>
        private bool RunLiveTimer(TaskItem task)
        {
            while (true)
            {
                Console.Clear();
                TimerScreenAction action = TimerScreenAction.None;

                AnsiConsole.Live(this.BuildLiveTimerPanel(task))
                    .AutoClear(false)
                    .Start(context =>
                    {
                        while (action == TimerScreenAction.None)
                        {
                            TimeSpan elapsed = this._timerService.GetElapsedTime();

                            context.UpdateTarget(
                                this.BuildLiveTimerPanel(task, elapsed));

                            if (Console.KeyAvailable)
                            {
                                ConsoleKey key = Console.ReadKey(true).Key;

                                if (key == ConsoleKey.P)
                                {
                                    if (this._timerService.IsRunning)
                                    {
                                        this._timerService.Pause();
                                    }
                                    else
                                    {
                                        this._timerService.Resume();
                                    }
                                }
                                else if (key == ConsoleKey.E)
                                {
                                    action = TimerScreenAction.Extend;
                                }
                                else if (key == ConsoleKey.B)
                                {
                                    action = TimerScreenAction.Back;
                                }
                                else if (key == ConsoleKey.S)
                                {
                                    action = TimerScreenAction.Stop;
                                }
                            }

                            Thread.Sleep(200);
                        }
                    });

                if (action == TimerScreenAction.Extend)
                {
                    this.PromptExtendTargetTime(task);
                    continue;
                }

                if (action == TimerScreenAction.Back)
                {
                    return false;
                }

                if (action == TimerScreenAction.Stop)
                {
                    TaskItem? completedTask = this._timerService.StopAndSaveTask();

                    this.RenderCompletedTask(completedTask);

                    string nextChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("What would you like to do next?")
                            .AddChoices(
                                "Start Next Task",
                                "Back to Dashboard"));

                    return nextChoice == "Start Next Task";
                }
            }
        }

        /// <summary>
        /// Creates the live timer display with the current task details and elapsed duration.
        /// </summary>
        /// <param name="task">The active task.</param>
        /// <param name="elapsed">The current elapsed duration. If omitted, it is retrieved from the timer service.</param>
        /// <returns>A panel containing the current timer information.</returns>
        private Panel BuildLiveTimerPanel(TaskItem task, TimeSpan? elapsed = null)
        {
            TimeSpan currentElapsed = elapsed ?? this._timerService.GetElapsedTime();

            string status = this._timerService.IsRunning
                ? "[green]RUNNING[/]"
                : "[yellow]PAUSED[/]";

            string targetStatus = this._targetReachedBannerShown
                ? "\n[bold red]Target time reached![/]"
                : string.Empty;

            string content =
                $"[bold]Task:[/] {Markup.Escape(task.TaskName)}\n" +
                $"[bold]Status:[/] {status}\n\n" +
                $"[bold]Elapsed time:[/] [cyan]{this.FormatDuration(currentElapsed)}[/]\n" +
                $"[bold]Target time:[/] {this.FormatDuration(task.TargetTime)}\n" +
                $"[bold]Extended time:[/] {this.FormatDuration(task.ExtendedTime)}\n" +
                $"[bold]Effective target:[/] {this.FormatDuration(task.TargetTime + task.ExtendedTime)}" +
                targetStatus +
                "\n\n[grey]P: Pause/Resume   E: Extend   S: Stop & Save   B: Dashboard[/]";

            return new Panel(new Markup(content))
                .Header("TASK TIMER")
                .Border(BoxBorder.Rounded)
                .Padding(1, 0)
                .Expand();
        }

        /// <summary>
        /// Displays the details of a completed task.
        /// </summary>
        /// <param name="task">The completed task, or null if no task was saved.</param>
        private void RenderCompletedTask(TaskItem? task)
        {
            AnsiConsole.Clear();
            NotificationHelper.Display();

            if (task == null)
            {
                AnsiConsole.MarkupLine("[yellow]No active task was available to save.[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Simple)
                .AddColumn("Property")
                .AddColumn("Value");

            table.AddRow("Task", Markup.Escape(task.TaskName));
            table.AddRow("Start Date", task.StartDate.ToString());
            table.AddRow("End Date", task.EndDate?.ToString() ?? "-");
            table.AddRow("Start Time", task.StartTime.ToString("hh:mm:ss tt"));
            table.AddRow("End Time", task.EndTime?.ToString("hh:mm:ss tt") ?? "-");
            table.AddRow("Target", this.FormatDuration(task.TargetTime));
            table.AddRow("Extended", this.FormatDuration(task.ExtendedTime));
            table.AddRow("Elapsed Worked", this.FormatDuration(task.ElapsedTime));
            table.AddRow("Total Duration", this.FormatDuration(task.TotalDuration));

            AnsiConsole.MarkupLine("[bold green]TASK SAVED SUCCESSFULLY[/]");
            AnsiConsole.Write(table);
        }

        /// <summary>
        /// Prompts the user to extend the target duration of the active task.
        /// </summary>
        /// <param name="task">The active task whose target duration is being extended.</param>
        private void PromptExtendTargetTime(TaskItem task)
        {
            bool wasRunning = this._timerService.IsRunning;

            if (wasRunning)
            {
                this._timerService.Pause();
            }

            try
            {
                AnsiConsole.Clear();
                NotificationHelper.Display();

                AnsiConsole.MarkupLine("[bold]EXTEND TARGET TIME[/]");
                AnsiConsole.Write(new Rule());

                AnsiConsole.MarkupLine(
                    "[grey]The timer is paused while you enter the extension.[/]\n");

                if (!this.TryReadDurationUnit("Add Hours", 100000, out int hours) ||
                    !this.TryReadDurationUnit("Add Minutes", 59, out int minutes) ||
                    !this.TryReadDurationUnit("Add Seconds", 59, out int seconds))
                {
                    AnsiConsole.MarkupLine(
                        "[red]Invalid duration. Extension cancelled.[/]");

                    this.WaitForUser();
                    return;
                }

                TimeSpan extension =
                    TimeSpan.FromHours(hours) +
                    TimeSpan.FromMinutes(minutes) +
                    TimeSpan.FromSeconds(seconds);

                if (this._timerService.ExtendTargetTime(extension))
                {
                    TimeSpan effectiveTarget = task.TargetTime + task.ExtendedTime;

                    if (this._timerService.GetElapsedTime() < effectiveTarget)
                    {
                        this._targetReachedBannerShown = false;
                    }

                    AnsiConsole.MarkupLine(
                        $"\n[green]Target extended by {this.FormatDuration(extension)}.[/]");

                    AnsiConsole.MarkupLine(
                        $"New effective target: {this.FormatDuration(effectiveTarget)}");
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        "[yellow]No additional time was added.[/]");
                }

                this.WaitForUser();
            }
            finally
            {
                if (wasRunning && this._timerService.ActiveTask != null)
                {
                    this._timerService.Resume();
                }
            }
        }

        /// <summary>
        /// Handles the event raised when the active task reaches its target duration.
        /// </summary>
        /// <param name="task">The task whose target duration has been reached.</param>
        private void HandleTargetTimeReached(TaskItem task)
        {
            this._targetReachedBannerShown = true;
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
        /// Formats a duration into HH:mm:ss.
        /// </summary>
        /// <param name="duration">The duration to format.</param>
        /// <returns>The formatted duration.</returns>
        private string FormatDuration(TimeSpan duration)
        {
            return DurationHelper.Format(duration);
        }
    }
}
