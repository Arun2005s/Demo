namespace Timer.Models
{
    /// <summary>
    /// Defines the actions available on the live timer screen.
    /// </summary>
    internal enum TimerScreenAction
    {
        /// <summary>
        /// No action selected or default state.
        /// </summary>
        None,

        /// <summary>
        /// Extends the active timer's remaining duration.
        /// </summary>
        Extend,

        /// <summary>
        /// Navigates back from the live timer screen.
        /// </summary>
        Back,

        /// <summary>
        /// Stops and ends the active timer.
        /// </summary>
        Stop,
    }
}
