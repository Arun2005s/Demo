using Spectre.Console;
using Spectre.Console.Rendering;

namespace Timer.Helpers
{
    /// <summary>
    /// Queues the application's supported notifications and renders them as a small stack.
    /// </summary>
    internal static class NotificationHelper
    {
        private const int MaximumNotifications = 3;
        private static readonly object SyncRoot = new object();
        private static readonly List<(string Key, string Message)> Notifications = new List<(string, string)>();

        /// <summary>
        /// Adds or refreshes a notification, keeping only the latest notification for each type.
        /// </summary>
        /// <param name="key">The notification type key.</param>
        /// <param name="message">The message to display.</param>
        public static void Queue(string key, string message)
        {
            lock (SyncRoot)
            {
                Notifications.RemoveAll(notification => notification.Key == key);
                Notifications.Add((key, message));
                if (Notifications.Count > MaximumNotifications)
                {
                    Notifications.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Renders queued notifications in a right-aligned stack and clears the queue.
        /// </summary>
        public static void Display()
        {
            List<(string Key, string Message)> pending;
            lock (SyncRoot)
            {
                pending = Notifications.ToList();
                Notifications.Clear();
            }

            if (pending.Count == 0)
            {
                return;
            }

            IRenderable[] panels = pending
                .Select(notification => new Panel(new Markup(
                    $"[{(notification.Key == "target" ? "red" : "cyan")}]{Markup.Escape(notification.Message)}[/]"))
                    .Border(BoxBorder.Rounded))
                .ToArray();
            AnsiConsole.Write(new Align(new Rows(panels), HorizontalAlignment.Right));
        }
    }
}
