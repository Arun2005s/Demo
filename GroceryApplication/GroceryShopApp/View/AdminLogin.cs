
using GroceryShopApp.Model;
using GroceryShopApp.Services;

namespace GroceryShopApp.View
{
    /// <summary>
    /// Enables the admin to enter details to login.
    /// </summary>
    internal class AdminLogin
    {
        private readonly AdminServices _adminServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminLogin"/> class.
        /// </summary>
        /// <param name="adminServices">Services of admin.</param>
        public AdminLogin(AdminServices adminServices)
        {
            this._adminServices = adminServices;
        }

        /// <summary>
        /// Asks the user for admin credentials to login.
        /// </summary>
        internal void Login()
        {
            Console.Write($@"
==================================
          ADMIN LOG-IN
==================================
");

            Console.Write("Enter admin user-name: ");
            string? userName = Console.ReadLine();

            Console.Write("Enter admin password [Alpha-numeric]: ");
            string? password = Console.ReadLine();

            string validatorMessage = Validator.ValidateLoginFields(userName, password);

            if (validatorMessage != "passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            if (!this._adminServices.IsAdmin(userName!, password!))
            {
                ConsoleHelper.WriteErrorMessage("\nUser is not an admin\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            ConsoleHelper.WriteSuccessMessage("\nWelcome! Admin\n");
            Thread.Sleep(1000);
            Console.Clear();

            this.DisplayLowStockWarning();
            this.AdminMenu();
        }

        /// <summary>
        /// Displays low-stock products when admin logs in.
        /// </summary>
        private void DisplayLowStockWarning()
        {
            List<Product> products = this._adminServices.GetLowStockProducts();

            if (products.Count == 0)
            {
                ConsoleHelper.WriteSuccessMessage("\nAll products have sufficient stock.\n");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(@"
==================================
          LOW STOCK WARNING
==================================");

            foreach (Product product in products)
            {
                Console.WriteLine(
                    $"Product: {product.Name} | Category: {product.Category} | Remaining Stock: {product.Quantity}");
            }

            Console.WriteLine("==================================");

            Console.ResetColor();
        }

        /// <summary>
        /// Displays the menu options for the admin.
        /// </summary>
        internal void AdminMenu()
        {
            while (true)
            {
                Console.Write(@"
==================================
             ADMIN MENU
==================================
[P] View Products
[S] Update Product Stock
[C] Add New Category
[A] Add New Product
[U] View All Users
[R] Promote User to Admin
[O] View Customer Orders
[L] Logout

Enter Choice: ");

                ConsoleKey userChoice = Console.ReadKey().Key;
                Console.Clear();

                switch (userChoice)
                {
                    case ConsoleKey.P:
                        this.DisplayProducts();
                        break;

                    case ConsoleKey.S:
                        this.UpdateProductStock();
                        break;

                    case ConsoleKey.C:
                        this.AddCategory();
                        break;

                    case ConsoleKey.A:
                        this.AddProduct();
                        break;

                    case ConsoleKey.U:
                        this.DisplayUsers();
                        break;

                    case ConsoleKey.R:
                        this.PromoteUser();
                        break;

                    case ConsoleKey.O:
                        this.DisplayOrders();
                        break;

                    case ConsoleKey.L:
                        ConsoleHelper.WriteColored("\nLogging out...", ConsoleColor.Yellow);
                        Thread.Sleep(1000);
                        Console.Clear();
                        return;

                    default:
                        ConsoleHelper.WriteErrorMessage("\nInvalid choice.\n");
                        break;
                }
            }
        }

        /// <summary>
        /// Displays all available products.
        /// </summary>
        private void DisplayProducts()
        {
            List<Product> products = this._adminServices.GetAllProducts();

            Console.WriteLine(@"
==================================
          PRODUCT DETAILS
==================================");

            foreach (Product product in products)
            {
                Console.WriteLine($@"
Product: {product.Name}
Category: {product.Category}
Price: ₹{product.Price}
Stock: {product.Quantity}
==================================");
            }

            ConsoleHelper.CleanConsole();
        }

        /// <summary>
        /// Updates the stock of an existing product.
        /// </summary>
        private void UpdateProductStock()
        {
            this.DisplayProductsWithoutPause();

            Console.Write("\nEnter product name to update stock: ");
            string productName = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(productName))
            {
                ConsoleHelper.WriteErrorMessage("\nProduct name cannot be empty.\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            Console.Write("Enter quantity to add: ");
            string quantity = Console.ReadLine() ?? string.Empty;

            string validatorMessage = Validator.ValidateStockQuantity(quantity);

            if (validatorMessage != "passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            string result = this._adminServices.UpdateProductStock(
                productName.Trim(),
                int.Parse(quantity));

            if (result == "Product stock updated successfully.")
            {
                ConsoleHelper.WriteSuccessMessage($"\n{result}\n");
            }
            else
            {
                ConsoleHelper.WriteErrorMessage($"\n{result}\n");
            }

            ConsoleHelper.CleanConsole();
        }

        /// <summary>
        /// Adds a new category.
        /// </summary>
        private void AddCategory()
        {
            Console.WriteLine(@"
==================================
           ADD CATEGORY
==================================");

            Console.Write("Enter new category name: ");
            string categoryName = Console.ReadLine() ?? string.Empty;

            string validatorMessage = Validator.ValidateCategory(categoryName);

            if (validatorMessage != "passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            string result = this._adminServices.AddCategory(categoryName);

            if (result == "Category added successfully.")
            {
                ConsoleHelper.WriteSuccessMessage($"\n{result}\n");
            }
            else
            {
                ConsoleHelper.WriteErrorMessage($"\n{result}\n");
            }

            ConsoleHelper.CleanConsole();
        }

        /// <summary>
        /// Adds a new product to an existing category.
        /// </summary>
        private void AddProduct()
        {
            Console.WriteLine(@"
==================================
             ADD PRODUCT
==================================");

            List<string> categories = this._adminServices.GetAllCategories();

            Console.WriteLine("\nAvailable Categories:");

            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i]}");
            }

            Console.Write("\nSelect category number: ");

            if (!int.TryParse(Console.ReadLine(), out int categoryNumber) ||
                categoryNumber < 1 ||
                categoryNumber > categories.Count)
            {
                ConsoleHelper.WriteErrorMessage("\nInvalid category selection.\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            string category = categories[categoryNumber - 1];

            Console.Write("\nEnter product name: ");
            string productName = Console.ReadLine() ?? string.Empty;

            string validatorMessage = Validator.ValidateProductName(productName);

            if (validatorMessage != "passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            Console.Write("Enter product price: ");
            string price = Console.ReadLine() ?? string.Empty;

            validatorMessage = Validator.ValidateProductPrice(price);

            if (validatorMessage != "passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            Console.Write("Enter initial stock quantity: ");
            string quantity = Console.ReadLine() ?? string.Empty;

            validatorMessage = Validator.ValidateProductQuantity(quantity);

            if (validatorMessage != "passed")
            {
                ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            Console.Write("Enter image filename (optional): ");
            string imageFile = Console.ReadLine() ?? string.Empty;

            string result = this._adminServices.AddProduct(
                productName,
                category,
                decimal.Parse(price),
                int.Parse(quantity),
                imageFile);

            if (result == "Product added successfully.")
            {
                ConsoleHelper.WriteSuccessMessage($"\n{result}\n");
            }
            else
            {
                ConsoleHelper.WriteErrorMessage($"\n{result}\n");
            }

            ConsoleHelper.CleanConsole();
        }

        /// <summary>
        /// Displays all registered users.
        /// </summary>
        private void DisplayUsers()
        {
            List<User> users = this._adminServices.GetAllUsers();

            Console.WriteLine(@"
==================================
          REGISTERED USERS
==================================");

            if (users.Count == 0)
            {
                ConsoleHelper.WriteErrorMessage("\nNo users found.\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            foreach (User user in users)
            {
                Console.WriteLine($@"
Name: {user.Name}
Username: {user.UserName}
Phone: {user.Phone}
Role: {(user.IsAdmin ? "Admin" : "Customer")}
==================================");
            }

            ConsoleHelper.CleanConsole();
        }

        /// <summary>
        /// Promotes an existing customer to admin.
        /// </summary>
        private void PromoteUser()
        {
            this.DisplayUsersWithoutPause();

            Console.Write("\nEnter username to promote: ");
            string userName = Console.ReadLine() ?? string.Empty;

            string result = this._adminServices.PromoteUser(userName);

            if (result == "User promoted to admin successfully.")
            {
                ConsoleHelper.WriteSuccessMessage($"\n{result}\n");
            }
            else
            {
                ConsoleHelper.WriteErrorMessage($"\n{result}\n");
            }

            ConsoleHelper.CleanConsole();
        }

        /// <summary>
        /// Displays all registered users without pausing the console.
        /// </summary>
        private void DisplayUsersWithoutPause()
        {
            List<User> users = this._adminServices.GetAllUsers();

            Console.WriteLine(@"
==================================
          REGISTERED USERS
==================================");

            foreach (User user in users)
            {
                Console.WriteLine($@"
Username: {user.UserName}
Name: {user.Name}
Role: {(user.IsAdmin ? "Admin" : "Customer")}
==================================");
            }
        }

        /// <summary>
        /// Displays all products without pausing the console.
        /// </summary>
        private void DisplayProductsWithoutPause()
        {
            List<Product> products = this._adminServices.GetAllProducts();

            Console.WriteLine(@"
==================================
          PRODUCT STOCK
==================================");

            foreach (Product product in products)
            {
                Console.WriteLine(
                    $"Product: {product.Name} | Category: {product.Category} | Stock: {product.Quantity}");
            }
        }

        /// <summary>
        /// Displays all customer orders.
        /// </summary>
        private void DisplayOrders()
        {
            List<Order> orders = this._adminServices.GetAllOrders();

            Console.WriteLine(@"
==================================
        CUSTOMER ORDERS
==================================");

            if (orders.Count == 0)
            {
                ConsoleHelper.WriteErrorMessage("\nNo orders found.\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            foreach (Order order in orders)
            {
                Console.WriteLine($@"
==================================
Order ID: {order.OrderId}
Customer: {order.UserName}
Order Date: {order.OrderDate}
Delivery Address: {order.DeliveryAddress}
Delivery Time: {order.DeliveryTime}");

                foreach (OrderItem item in order.Items)
                {
                    Console.WriteLine($@"
Product: {item.ProductName}
Price: ₹{item.Price}
Quantity: {item.Quantity}
Subtotal: ₹{item.SubTotal}");
                }

                Console.WriteLine($@"
Total Amount: ₹{order.TotalAmount}
==================================");
            }

            ConsoleHelper.CleanConsole();
        }
    }
}
