using System.Text;
using Spectre.Console;
using Timer.Helpers;
using Timer.Models;
using Timer.Services;

namespace Timer.Views
{
    /// <summary>
    /// Provides the console-based user interface for user login and registration.
    /// </summary>
    public class AuthenticationView
    {
        private readonly AuthenticationServices _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationView"/> class.
        /// </summary>
        /// <param name="authService">The service used to handle user authentication and registration.</param>
        public AuthenticationView(AuthenticationServices authService)
        {
            this._authService = authService;
        }

        /// <summary>
        /// Displays the main authentication menu and processes user choices for logging in, signing up, or exiting.
        /// </summary>
        /// <returns>The authenticated <see cref="User"/> if login succeeds; otherwise, null if the user chooses to exit.</returns>
        public User? ShowLoginPage()
        {
            while (true)
            {
                AnsiConsole.Clear();
                NotificationHelper.Display();

                AnsiConsole.MarkupLine("[bold]TASK TIMER[/]");
                AnsiConsole.Write(new Spectre.Console.Rule());
                AnsiConsole.MarkupLine("[bold]LOGIN PAGE[/]\n");

                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an option:")
                        .AddChoices("Login", "Sign Up", "Exit"));

                switch (choice)
                {
                    case "Login":
                        User? user = this.Login();

                        if (user != null)
                        {
                            return user;
                        }

                        break;

                    case "Sign Up":
                        this.SignUp();
                        break;

                    case "Exit":
                        return null;
                }
            }
        }

        /// <summary>
        /// Prompts the user for login credentials and authenticates the account.
        /// </summary>
        /// <returns>The authenticated user if the credentials are valid; otherwise, null.</returns>
        private User? Login()
        {
            AnsiConsole.Clear();
            NotificationHelper.Display();

            AnsiConsole.MarkupLine("[bold]LOGIN[/]");
            AnsiConsole.Write(new Spectre.Console.Rule());

            string email = AnsiConsole.Ask<string>("Email:");

            AnsiConsole.MarkupLine(
                "[grey]Press Tab while entering the password to show or hide it.[/]");

            AnsiConsole.MarkupLine("Password:");
            string password = this.ReadMaskedPassword();

            User? user = this._authService.Login(email, password);

            if (user == null)
            {
                AnsiConsole.MarkupLine("\n[red]Invalid email or password.[/]");
                this.WaitForUser();
                return null;
            }

            AnsiConsole.MarkupLine("\n[green]Login successful.[/]");
            NotificationHelper.Queue("login", $"Welcome, {user.Name}!");
            NotificationHelper.Display();

            return user;
        }

        /// <summary>
        /// Prompts the user for registration details and attempts to create a new account.
        /// Each field allows a maximum of three attempts.
        /// </summary>
        private void SignUp()
        {
            AnsiConsole.Clear();
            NotificationHelper.Display();

            AnsiConsole.MarkupLine("[bold]SIGN UP[/]");
            AnsiConsole.Write(new Spectre.Console.Rule());

            string name = string.Empty;
            bool validName = false;

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                name = AnsiConsole.Ask<string>("Name:").Trim();

                if (!string.IsNullOrWhiteSpace(name) &&
                    name.All(c => char.IsLetter(c) || c == ' '))
                {
                    validName = true;
                    break;
                }

                AnsiConsole.MarkupLine(
                    $"[red]Invalid name. Use only letters and spaces. Attempts left: {3 - attempt}[/]");
            }

            if (!validName)
            {
                AnsiConsole.MarkupLine("\n[red]Maximum attempts reached. Registration cancelled.[/]");
                this.WaitForUser();
                return;
            }

            string email = string.Empty;
            bool validEmail = false;

            AnsiConsole.MarkupLine(
                "[grey]Example: john@gmail.com[/]");

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                email = AnsiConsole.Ask<string>("Email:").Trim();

                if (System.Net.Mail.MailAddress.TryCreate(email, out var address) &&
                    address.Address == email)
                {
                    validEmail = true;
                    break;
                }

                AnsiConsole.MarkupLine(
                    $"[red]Invalid email. Enter a valid email address (e.g., john@gmail.com). Attempts left: {3 - attempt}[/]");
            }

            if (!validEmail)
            {
                AnsiConsole.MarkupLine("\n[red]Maximum attempts reached. Registration cancelled.[/]");
                this.WaitForUser();
                return;
            }

            AnsiConsole.MarkupLine("\n[bold]PASSWORD REQUIREMENTS[/]");
            AnsiConsole.MarkupLine("[grey]• Minimum 8 characters[/]");
            AnsiConsole.MarkupLine("[grey]• At least one uppercase letter[/]");
            AnsiConsole.MarkupLine("[grey]• At least one lowercase letter[/]");
            AnsiConsole.MarkupLine("[grey]• At least one number[/]");
            AnsiConsole.MarkupLine("[grey]• At least one special character[/]");
            AnsiConsole.MarkupLine("[grey]• No spaces allowed[/]");
            AnsiConsole.MarkupLine("[grey]Example: Timer@123[/]");
            AnsiConsole.MarkupLine(
                "\n[yellow]Tip: Press TAB while entering your password to show or hide it.[/]");

            string password = string.Empty;
            bool validPassword = false;

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                AnsiConsole.MarkupLine("\nPassword:");
                password = this.ReadMaskedPassword();

                if (this.IsValidPassword(password))
                {
                    validPassword = true;
                    break;
                }

                AnsiConsole.MarkupLine(
                    $"[red]Password does not meet the requirements. Attempts left: {3 - attempt}[/]");
            }

            if (!validPassword)
            {
                AnsiConsole.MarkupLine("\n[red]Maximum attempts reached. Registration cancelled.[/]");
                this.WaitForUser();
                return;
            }

            AnsiConsole.MarkupLine("\nConfirm Password:");
            string confirmPassword = this.ReadMaskedPassword();

            if (password != confirmPassword)
            {
                AnsiConsole.MarkupLine("\n[red]Passwords do not match. Please try again.[/]");
                this.WaitForUser();
                return;
            }

            AnsiConsole.MarkupLine("\n[bold]DAILY WORK START TIME[/]");

            if (!this.TryReadStartTime(out TimeOnly dayStartTime))
            {
                AnsiConsole.MarkupLine(
                    "\n[red]Invalid time. Use an hour between 1 and 12, a minute between 0 and 59, and AM or PM.[/]");

                this.WaitForUser();
                return;
            }

            bool success = this._authService.SignUp(
                name,
                email,
                password,
                dayStartTime,
                out string message);

            if (success)
            {
                AnsiConsole.MarkupLine($"\n[green]{Markup.Escape(message)}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"\n[red]{Markup.Escape(message)}[/]");
            }

            this.WaitForUser();
        }

        /// <summary>
        /// Checks whether the password meets the required security rules.
        /// </summary>
        /// <param name="password">The password entered by the user.</param>
        /// <returns>True if the password is valid; otherwise, false.</returns>
        private bool IsValidPassword(string password)
        {
            return password.Length >= 8 &&
                   !password.Any(char.IsWhiteSpace) &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => !char.IsLetterOrDigit(c));
        }

        /// <summary>
        /// Prompts the user for 12-hour clock components and converts them into a <see cref="TimeOnly"/> value.
        /// </summary>
        /// <param name="startTime">When this method returns, contains the parsed time if the inputs are valid.</param>
        /// <returns>True if all inputs are valid; otherwise, false.</returns>
        private bool TryReadStartTime(out TimeOnly startTime)
        {
            startTime = default;

            string hourInput = AnsiConsole.Prompt(
                new TextPrompt<string>("Hour (1-12):"));

            if (!int.TryParse(hourInput, out int hour) || hour < 1 || hour > 12)
            {
                return false;
            }

            string minuteInput = AnsiConsole.Prompt(
                new TextPrompt<string>("Minute (0-59):")
                    .DefaultValue("0")
                    .AllowEmpty());

            int minute = 0;

            if (!string.IsNullOrWhiteSpace(minuteInput) &&
                (!int.TryParse(minuteInput, out minute) || minute < 0 || minute > 59))
            {
                return false;
            }

            string period = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select AM or PM:")
                    .AddChoices("AM", "PM"));

            int hour24 = hour % 12;

            if (period == "PM")
            {
                hour24 += 12;
            }

            startTime = new TimeOnly(hour24, minute);

            return true;
        }

        /// <summary>
        /// Reads password input from the console and allows the user to toggle visibility using the Tab key.
        /// </summary>
        /// <returns>The plain-text password entered by the user.</returns>
        private string ReadMaskedPassword()
        {
            StringBuilder password = new StringBuilder();
            bool isMasked = true;

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Tab)
                {
                    isMasked = !isMasked;

                    for (int i = 0; i < password.Length; i++)
                    {
                        Console.Write("\b \b");
                    }

                    Console.Write(
                        isMasked
                            ? new string('*', password.Length)
                            : password.ToString());
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    password.Append(keyInfo.KeyChar);
                    Console.Write(isMasked ? '*' : keyInfo.KeyChar);
                }
            }

            return password.ToString();
        }

        /// <summary>
        /// Waits for the user to press a key before continuing.
        /// </summary>
        private void WaitForUser()
        {
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }
}
