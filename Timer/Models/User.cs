namespace Timer.Models
{
    /// <summary>
    /// Represents a registered user in the timer application.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        /// <value>The unique <see cref="Guid"/> of the user.</value>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        /// <value>A <see cref="string"/> representing the user's name. Defaults to <see cref="string.Empty"/>.</value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's unique login email address.
        /// </summary>
        /// <value>A <see cref="string"/> representing the email address. Defaults to <see cref="string.Empty"/>.</value>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's password or password hash.
        /// </summary>
        /// <value>A <see cref="string"/> representing the password. Defaults to <see cref="string.Empty"/>.</value>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's preferred daily work start time for receiving reminder notifications.
        /// </summary>
        /// <value>A <see cref="TimeOnly"/> representing the daily start time. Defaults to <c>09:00:00</c>.</value>
        public TimeOnly DayStartTime { get; set; } = new TimeOnly(9, 0);

        /// <summary>
        /// Gets or sets the date when the user's daily reminder was last delivered.
        /// </summary>
        /// <value>The last reminder date, or null if no reminder has been delivered.</value>
        public DateOnly? LastDailyReminderDate { get; set; }
    }
}
