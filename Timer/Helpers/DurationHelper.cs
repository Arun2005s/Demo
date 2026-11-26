namespace Timer.Helpers
{
    /// <summary>
    /// Provides a shared display format for task durations.
    /// </summary>
    internal static class DurationHelper
    {
        /// <summary>
        /// Formats a duration as total hours, minutes, and seconds.
        /// </summary>
        /// <param name="duration">The duration to format.</param>
        /// <returns>The formatted duration string.</returns>
        public static string Format(TimeSpan duration)
        {
            return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }
}
