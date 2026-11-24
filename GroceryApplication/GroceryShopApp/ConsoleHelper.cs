namespace GroceryShopApp
{
    /// <summary>
    /// Performs the console operations for smooth user experience.
    /// </summary>
    public class ConsoleHelper
    {
        /// <summary>
        /// Displays message in the console in colored format.
        /// </summary>
        /// <param name="message">Message to be displayed in the console</param>
        /// <param name="color">Color in which the message needs to be displayed</param>
        public static void WriteColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Displays the success message in green color.
        /// </summary>
        /// <param name="message">Message to be displayed</param>
        public static void WriteSuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Displays the error message in red color.
        /// </summary>
        /// <param name="message">Message to be displayed</param>
        public static void WriteErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Cleans the console for smooth user experience.
        /// </summary>
        public static void CleanConsole()
        {
            Console.Write("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
