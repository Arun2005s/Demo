using System;

namespace Timer.Models
{
    /// <summary>
    /// Represents a timed task belonging to a user.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the task.
        /// </summary>
        /// <value>The unique <see cref="Guid"/> of the task.</value>
        public Guid TaskId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who owns this task.
        /// </summary>
        /// <value>The unique <see cref="Guid"/> of the associated user.</value>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the name or title of the task.
        /// </summary>
        /// <value>A <see cref="string"/> representing the task name. Defaults to <see cref="string.Empty"/>.</value>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the calendar date when the task started.
        /// </summary>
        /// <value>A <see cref="DateOnly"/> representing the start date of the task.</value>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets or sets the calendar date when the task ended.
        /// </summary>
        /// <value>A nullable <see cref="DateOnly"/> representing the end date of the task, or <see langword="null"/> if still in progress.</value>
        public DateOnly? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the task started.
        /// </summary>
        /// <value>A <see cref="DateTime"/> representing when timing began.</value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the task ended.
        /// </summary>
        /// <value>A nullable <see cref="DateTime"/> representing when timing stopped, or <see langword="null"/> if still in progress.</value>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the initial planned target duration for completing the task.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> representing the original goal duration.</value>
        public TimeSpan TargetTime { get; set; }

        /// <summary>
        /// Gets or sets the total additional time added beyond the initial <see cref="TargetTime"/>.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> representing the extended duration. Defaults to <see cref="TimeSpan.Zero"/>.</value>
        public TimeSpan ExtendedTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the actual active time worked on the task (excluding paused intervals).
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> representing the active elapsed work time.</value>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// Gets or sets the overall wall-clock duration from <see cref="StartTime"/> to <see cref="EndTime"/>.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> representing the total session duration.</value>
        public TimeSpan TotalDuration { get; set; }
    }
}
