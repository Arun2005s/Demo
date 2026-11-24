using System.Text;
using GroceryShopApp.Services;

namespace GroceryShopApp.View
{
    /// <summary>
    /// Authenticates the user by fetching the details and displays appropritate messages.
    /// </summary>
    internal class UserAuth
    {
        private readonly UserServices _userServices;
        private readonly MainMenu _mainMenu;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuth"/> class.
        /// </summary>
        /// <param name="userServices">Services of the user</param>
        /// <param name="mainMenu">Main menu of the application</param>
        public UserAuth(UserServices userServices, MainMenu mainMenu)
        {
            this._userServices = userServices;
            this._mainMenu = mainMenu;
        }

        /// <summary>
        /// Retrieved the user credentials for signing up and displays appropriate message.
        /// </summary>
        public void Register()
        {
            Console.Write($@"
==================================
             SIGN-UP
==================================
");
            Console.Write("Enter full-name: ");
            string? fullName = Console.ReadLine();

            Console.Write("Enter unique user-name [4-20 letters or digits]: ");
            string? userName = Console.ReadLine();

            Console.Write("Enter Phone number [10 digits]: ");
            string? phone = Console.ReadLine();

            Console.Write("Enter Password [4-20 character with letter and digits]: ");
            string? password = this.ReadMaskedPassword();

            string validatorMessage = Validator.ValidateSignupFields(fullName, userName, phone, password);
            if (validatorMessage != $"passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            if (this._userServices.CreateUser(fullName!, userName!, phone!, password!))
            {
                ConsoleHelper.WriteSuccessMessage("\nUser account created\n");
            }
            else
            {
                ConsoleHelper.WriteErrorMessage("\nUser account already exists\n");
            }

            ConsoleHelper.CleanConsole();
        }

        /// <summary>
        /// Retrieved the log-in credentials from user and displays appropriate message.
        /// </summary>
        internal void Login()
        {
            Console.Write($@"
==================================
             LOG-IN
==================================
");
            Console.Write("Enter unique user-name [4-20 letters or digits]: ");
            string? userName = Console.ReadLine();

            Console.Write("Enter Password [4-20 character with letter and digits]: ");
            string? password = this.ReadMaskedPassword();

            string validatorMessage = Validator.ValidateLoginFields(userName, password);
            if (validatorMessage != "passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            if (this._userServices.UserExists(userName!, password!))
            {
                ConsoleHelper.WriteColored("\nLogged In", ConsoleColor.Green);
                Thread.Sleep(1000);
                this._mainMenu.AssignCurrentUser(userName!);
                Console.Clear();
                this._mainMenu.MenuOptions();
            }
            else
            {
                ConsoleHelper.WriteColored($"User data not valid\nReturning to Main menu.", ConsoleColor.Red);
                Thread.Sleep(1300);
                Console.Clear();
            }
        }

        /// <summary>
        /// Reads password input from the console, allowing the user to toggle visibility using the Tab key.
        /// </summary>
        /// <returns>The plain-text password entered by the user.</returns>
        private string ReadMaskedPassword()
        {
            StringBuilder password = new StringBuilder();
            bool isHidden = true;
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
                    isHidden = !isHidden;
                    for (int i = 0; i < password.Length; i++)
                    {
                        Console.Write("\b \b");
                    }

                    Console.Write(isHidden ? new string('*', password.Length) : password.ToString());
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
                    Console.Write(isHidden ? '*' : keyInfo.KeyChar);
                }
            }

            return password.ToString();
        }
    }
}
