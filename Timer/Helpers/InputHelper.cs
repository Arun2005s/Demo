using Spectre.Console;

namespace Timer.Helpers
{
    /// <summary>
    /// Provides bounded text input for views that require additional validation.
    /// </summary>
    internal static class InputHelper
    {
        private const int MaximumAttempts = 3;

        /// <summary>
        /// Reads text with at most three validation attempts.
        /// </summary>
        /// <param name="prompt">The prompt displayed to the user.</param>
        /// <param name="isValid">The validation rule for the entered value.</param>
        /// <param name="errorMessage">The message shown for invalid input.</param>
        /// <param name="value">The valid entered value, or an empty string on failure.</param>
        /// <returns>True if a valid value was entered; otherwise, false.</returns>
        public static bool TryPromptText(string prompt, Func<string, bool> isValid, string errorMessage, out string value)
        {
            for (int attempt = 1; attempt <= MaximumAttempts; attempt++)
            {
                value = AnsiConsole.Ask<string>(prompt).Trim();
                if (isValid(value))
                {
                    return true;
                }

                AnsiConsole.MarkupLine($"[red]{Markup.Escape(errorMessage)} Attempts left: {MaximumAttempts - attempt}[/]");
            }

            value = string.Empty;
            AnsiConsole.MarkupLine("[yellow]Maximum attempts reached. Returning to the previous menu.[/]");
            return false;
        }
    }
}
