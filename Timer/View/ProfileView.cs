using System.Text;
using Spectre.Console;
using Timer.Helpers;
using Timer.Models;
using Timer.Service;

namespace Timer.View
{
    /// <summary>
    /// Provides a simple console interface for viewing and modifying profile data.
    /// </summary>
    public class ProfileView
    {
        private readonly ProfileServices _profileServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileView"/> class.
        /// </summary>
        /// <param name="profileServices"> services provided to update profile</param>
        public ProfileView(ProfileServices profileServices)
        {
            this._profileServices = profileServices;
        }

        /// <summary>
        /// Displays the simple profile menu.
        /// </summary>
        /// <param name="currentUser"> services provided to update profile</param>
        public void ShowProfileMenu(User currentUser)
        {
            while (true)
            {
                AnsiConsole.Clear();
                NotificationHelper.Display();

                AnsiConsole.MarkupLine("[bold cyan]=== My Profile ===[/]");
                AnsiConsole.MarkupLine($"[grey]User ID:[/] {currentUser.UserId}");
                AnsiConsole.MarkupLine($"[grey]Name:[/]    {currentUser.Name}");
                AnsiConsole.MarkupLine($"[grey]Email:[/]   {currentUser.Email}");
                AnsiConsole.MarkupLine($"[grey]Start:[/]   {currentUser.DayStartTime:hh:mm tt}");
                AnsiConsole.WriteLine();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an option:")
                        .AddChoices("Edit Profile Details", "Change Password", "Back"));

                if (choice == "Back")
                {
                    break;
                }

                if (choice == "Edit Profile Details")
                {
                    this.HandleProfileUpdate(currentUser);
                }
                else if (choice == "Change Password")
                {
                    this.HandlePasswordUpdate(currentUser);
                }
            }
        }

        /// <summary>
        /// Prompts for basic profile updates.
        /// </summary>
        private void HandleProfileUpdate(User currentUser)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]--- Edit Profile ---[/]");

            if (!InputHelper.TryPromptText("Enter your name:", input => !string.IsNullOrWhiteSpace(input), "Name cannot be empty.", out string newName))
            {
                return;
            }

            if (!InputHelper.TryPromptText(
                "Enter start time (e.g., 09:00 AM):",
                input => TimeOnly.TryParse(input, out _),
                "Enter a valid time.",
                out string startTimeInput))
            {
                return;
            }

            TimeOnly newStartTime = TimeOnly.Parse(startTimeInput);

            var updatedData = new User
            {
                UserId = currentUser.UserId,
                Email = currentUser.Email,
                Name = newName,
                DayStartTime = newStartTime,
                Password = string.Empty,
            };

            if (this._profileServices.UpdateProfile(currentUser.Email, updatedData, out string error))
            {
                currentUser.Name = newName;
                currentUser.DayStartTime = newStartTime;
                AnsiConsole.MarkupLine("[green]Profile updated successfully![/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Update failed:[/] {error}");
            }

            this.WaitForKeypress();
        }

        /// <summary>
        /// Prompts for password updates using a togglable masked console reader.
        /// </summary>
        private void HandlePasswordUpdate(User currentUser)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]--- Change Password ---[/]");
            AnsiConsole.MarkupLine("[grey](Press Tab anytime to toggle visibility)[/]");

            AnsiConsole.Markup("Enter new password: ");
            string newPassword = this.ReadMaskedPassword();

            AnsiConsole.Markup("Confirm new password: ");
            string confirmPassword = this.ReadMaskedPassword();

            if (newPassword != confirmPassword)
            {
                AnsiConsole.MarkupLine("[red]Passwords do not match.[/]");
                this.WaitForKeypress();
                return;
            }

            var updatedData = new User
            {
                UserId = currentUser.UserId,
                Email = currentUser.Email,
                Name = currentUser.Name,
                DayStartTime = currentUser.DayStartTime,
                Password = newPassword,
            };

            if (this._profileServices.UpdateProfile(currentUser.Email, updatedData, out string error))
            {
                AnsiConsole.MarkupLine("[green]Password changed successfully![/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Password update failed:[/] {error}");
            }

            this.WaitForKeypress();
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

        private void WaitForKeypress()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }
}
