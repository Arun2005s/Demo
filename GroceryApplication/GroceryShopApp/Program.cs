
using GroceryShopApp.Repository;
using GroceryShopApp.Services;
using GroceryShopApp.View;

namespace GroceryShopApp
{
    /// <summary>
    /// Entry point of the application.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            UserRepository userRepository = new UserRepository("Users.json");
            ProductRepository productRepository = new ProductRepository("Products.json");
            CartRepository cartRepository = new CartRepository("Carts.json");
            OrderRepository orderRepository = new OrderRepository("Orders.json");
            CategoryRepository categoryRepository = new CategoryRepository("Categories.json");

            UserServices userServices = new UserServices(userRepository);
            ProductServices productServices = new ProductServices(
                productRepository,
                categoryRepository);

            CartServices cartServices = new CartServices(
                cartRepository,
                productRepository,
                orderRepository);

            OrderServices orderServices = new OrderServices(orderRepository);

            AdminServices adminServices = new AdminServices(
                userRepository,
                productRepository,
                orderRepository,
                categoryRepository);

            ProductView productView = new ProductView(
                productServices,
                cartServices);

            CartView cartView = new CartView(cartServices);

            OrderView orderView = new OrderView(orderServices);

            MainMenu mainMenu = new MainMenu(
                productView,
                cartView,
                orderView);

            UserAuth userAuth = new UserAuth(
                userServices,
                mainMenu);

            AdminLogin adminLogin = new AdminLogin(adminServices);

            while (true)
            {
                Console.Write($@"
==================================
         GROCERY SHOP APP
==================================
[L]ogIn
[S]ignUp
[A]dmin login
[E]xit

Enter Choice: ");

                ConsoleKey userChoice = Console.ReadKey().Key;
                Console.Clear();

                switch (userChoice)
                {
                    case ConsoleKey.S:
                        userAuth.Register();
                        break;

                    case ConsoleKey.L:
                        userAuth.Login();
                        break;

                    case ConsoleKey.A:
                        adminLogin.Login();
                        break;

                    case ConsoleKey.E:
                        ConsoleHelper.WriteColored("\nExiting...", ConsoleColor.Cyan);
                        Thread.Sleep(1000);
                        Environment.Exit(0);
                        break;

                    default:
                        ConsoleHelper.WriteErrorMessage("\nInvalid choice.\n");
                        break;
                }
            }
        }
    }
}
