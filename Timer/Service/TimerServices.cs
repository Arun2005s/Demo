
using System;
using System.Diagnostics;
using Timer.Interfaces;
using Timer.Models;

namespace Timer.Services
{
    /// <summary>
    /// Represents the method that handles the event when a task reaches its target duration.
    /// </summary>
    /// <param name="task">The <see cref="TaskItem"/> that reached its target duration.</param>
    public delegate void TargetTimeReachedHandler(TaskItem task);

    /// <summary>
    /// Represents the method that handles the daily work start reminder notification.
    /// </summary>
    /// <param name="user">The <see cref="User"/> whose daily work start time has arrived.</param>
    public delegate void DailyWorkReminderHandler(User user);

    /// <summary>
    /// Manages task creation, stopwatch timing operations, target and daily notifications, and persistence of completed tasks.
    /// </summary>
    public class TimerServices
    {
        private readonly ITaskRepository _taskRepository;
        private readonly Stopwatch _stopwatch;
        private TaskItem? _activeTask;
        private bool _targetNotified;
        private bool _isResumedTask;
        private TimeSpan _previouslyAccumulatedTime;

        private System.Timers.Timer? _dailyReminderTimer;
        private User? _monitoredUser;
        private DateOnly? _lastDailyNotificationDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerServices"/> class.
        /// </summary>
        /// <param name="taskRepository">The repository used to persist and retrieve task items.</param>
        public TimerServices(ITaskRepository taskRepository)
        {
            this._taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            this._stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Occurs once when the active task's elapsed time reaches or exceeds its effective target duration.
        /// </summary>
        public event TargetTimeReachedHandler? OnTargetTimeReached;

        /// <summary>
        /// Occurs once per day when the system clock reaches the logged-in user's daily work start time.
        /// </summary>
        public event DailyWorkReminderHandler? OnDailyWorkReminder;

        /// <summary>
        /// Gets a value indicating whether the stopwatch is currently running.
        /// </summary>
        public bool IsRunning => this._stopwatch.IsRunning;

        /// <summary>
        /// Gets the task currently being timed, or null if no task is active.
        /// </summary>
        public TaskItem? ActiveTask => this._activeTask;

        /// <summary>
        /// Starts monitoring the system clock for the specified user's daily work start time.
        /// </summary>
        /// <param name="user">The logged-in user to notify.</param>
        public void StartDailyReminderMonitor(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            this.StopDailyReminderMonitor();

            this._monitoredUser = user;
            this._lastDailyNotificationDate = null;

            this._dailyReminderTimer = new System.Timers.Timer(1000);
            this._dailyReminderTimer.Elapsed += (sender, e) => this.CheckDailyWorkStartTime();
            this._dailyReminderTimer.AutoReset = true;
            this._dailyReminderTimer.Start();
        }

        /// <summary>
        /// Stops monitoring for the daily work start time and releases timer resources.
        /// </summary>
        public void StopDailyReminderMonitor()
        {
            if (this._dailyReminderTimer != null)
            {
                this._dailyReminderTimer.Stop();
                this._dailyReminderTimer.Dispose();
                this._dailyReminderTimer = null;
            }

            this._monitoredUser = null;
        }

        /// <summary>
        /// Creates a new task and immediately starts the stopwatch.
        /// </summary>
        /// <param name="userId">The unique identifier of the user starting the task.</param>
        /// <param name="taskName">The name or title of the task.</param>
        /// <param name="targetTime">The initial planned target duration for the task.</param>
        /// <returns>The newly created and active task.</returns>
        public TaskItem StartNewTask(Guid userId, string taskName, TimeSpan targetTime)
        {
            if (this._activeTask != null)
            {
                throw new InvalidOperationException("An active task already exists. Stop the current task before starting a new task.");
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(taskName))
            {
                throw new ArgumentException("Task name cannot be empty.", nameof(taskName));
            }

            if (targetTime < TimeSpan.Zero)
            {
                throw new ArgumentException("Target time cannot be negative.", nameof(targetTime));
            }

            DateTime now = DateTime.Now;

            this._activeTask = new TaskItem
            {
                TaskId = Guid.NewGuid(),
                UserId = userId,
                TaskName = taskName.Trim(),
                StartDate = DateOnly.FromDateTime(now),
                EndDate = null,
                StartTime = now,
                EndTime = null,
                TargetTime = targetTime,
                ExtendedTime = TimeSpan.Zero,
                ElapsedTime = TimeSpan.Zero,
                TotalDuration = TimeSpan.Zero,
            };

            this._previouslyAccumulatedTime = TimeSpan.Zero;
            this._targetNotified = false;
            this._isResumedTask = false;
            this._stopwatch.Restart();

            return this._activeTask;
        }

        /// <summary>
        /// Loads a previously saved historical task and restarts the stopwatch tracking engine.
        /// </summary>
        /// <param name="task">The historical task record to resume tracking for.</param>
        public void ResumeExistingTask(TaskItem task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (this._activeTask != null)
            {
                throw new InvalidOperationException("An active task already exists. Stop the current task before resuming another task.");
            }

            this._activeTask = task;
            this._activeTask.EndDate = null;
            this._activeTask.EndTime = null;

            this._previouslyAccumulatedTime = task.ElapsedTime;
            this._targetNotified = false;
            this._isResumedTask = true;
            this._stopwatch.Restart();
        }

        /// <summary>
        /// Pauses the active stopwatch without resetting the accumulated duration.
        /// </summary>
        public void Pause()
        {
            if (this._stopwatch.IsRunning)
            {
                this._stopwatch.Stop();
            }
        }

        /// <summary>
        /// Resumes a paused stopwatch.
        /// </summary>
        public void Resume()
        {
            if (this._activeTask != null && !this._stopwatch.IsRunning)
            {
                this._stopwatch.Start();
            }
        }

        /// <summary>
        /// Gets the elapsed duration recorded so far and triggers the target notification if the effective target time is met.
        /// </summary>
        /// <returns>The current elapsed time.</returns>
        public TimeSpan GetElapsedTime()
        {
            TimeSpan totalElapsed = this._previouslyAccumulatedTime + this._stopwatch.Elapsed;

            if (this._activeTask != null)
            {
                TimeSpan effectiveTarget = this._activeTask.TargetTime + this._activeTask.ExtendedTime;

                if (!this._targetNotified
                    && effectiveTarget > TimeSpan.Zero
                    && totalElapsed >= effectiveTarget)
                {
                    this._targetNotified = true;
                    this.OnTargetTimeReached?.Invoke(this._activeTask);
                }
            }

            return totalElapsed;
        }

        /// <summary>
        /// Stops the stopwatch, updates task details, saves the task, and clears the active task.
        /// </summary>
        /// <returns>The completed and saved task, or null if no task was active.</returns>
        public TaskItem? StopAndSaveTask()
        {
            if (this._activeTask == null)
            {
                return null;
            }

            this._stopwatch.Stop();

            DateTime endNow = DateTime.Now;

            this._activeTask.EndDate = DateOnly.FromDateTime(endNow);
            this._activeTask.EndTime = endNow;
            this._activeTask.ElapsedTime = this._previouslyAccumulatedTime + this._stopwatch.Elapsed;
            this._activeTask.TotalDuration = endNow - this._activeTask.StartTime;

            if (this._isResumedTask)
            {
                bool isUpdated = this._taskRepository.Update(this._activeTask);

                if (!isUpdated)
                {
                    throw new InvalidOperationException("The resumed task could not be updated.");
                }
            }
            else
            {
                this._taskRepository.Add(this._activeTask);
            }

            TaskItem completedTask = this._activeTask;

            this._activeTask = null;
            this._previouslyAccumulatedTime = TimeSpan.Zero;
            this._targetNotified = false;
            this._isResumedTask = false;
            this._stopwatch.Reset();

            return completedTask;
        }

        /// <summary>
        /// Adds extra time to the task's extended duration and re-arms the target notification if necessary.
        /// </summary>
        /// <param name="additionalTime">The extra duration to add.</param>
        /// <returns>True if the active task was updated; otherwise, false.</returns>
        public bool ExtendTargetTime(TimeSpan additionalTime)
        {
            if (this._activeTask == null || additionalTime <= TimeSpan.Zero)
            {
                return false;
            }

            this._activeTask.ExtendedTime += additionalTime;

            TimeSpan totalElapsed = this._previouslyAccumulatedTime + this._stopwatch.Elapsed;
            TimeSpan effectiveTarget = this._activeTask.TargetTime + this._activeTask.ExtendedTime;

            if (totalElapsed < effectiveTarget)
            {
                this._targetNotified = false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the current time has reached the monitored user's daily work start time.
        /// Invokes the daily reminder once per day.
        /// </summary>
        private void CheckDailyWorkStartTime()
        {
            if (this._monitoredUser == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            DateOnly today = DateOnly.FromDateTime(now);
            TimeOnly currentTime = TimeOnly.FromDateTime(now);

            TimeOnly dayStartTime = new TimeOnly(
                this._monitoredUser.DayStartTime.Hour,
                this._monitoredUser.DayStartTime.Minute);

            if (this._lastDailyNotificationDate != today &&
                currentTime >= dayStartTime)
            {
                this._lastDailyNotificationDate = today;
                this.OnDailyWorkReminder?.Invoke(this._monitoredUser);
            }
        }
    }
}
