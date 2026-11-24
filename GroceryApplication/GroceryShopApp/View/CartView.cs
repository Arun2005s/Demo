using GroceryShopApp.Model;
using GroceryShopApp.Services;

namespace GroceryShopApp.View
{
    /// <summary>
    /// Performs the view level operations for cart and checkout.
    /// </summary>
    internal class CartView
    {
        private readonly CartServices _cartServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartView"/> class.
        /// </summary>
        /// <param name="cartServices">Services of the cart.</param>
        public CartView(CartServices cartServices)
        {
            this._cartServices = cartServices;
        }

        /// <summary>
        /// Displays the cart products of the current user.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        internal void DisplayCart(string userName)
        {
            List<Cart> carts = this._cartServices.GetCart(userName);

            Console.WriteLine(@"
==================================
            YOUR CART
==================================");

            if (carts.Count == 0)
            {
                ConsoleHelper.WriteErrorMessage("\nYour cart is empty.\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            for (int i = 0; i < carts.Count; i++)
            {
                Cart cart = carts[i];

                Console.WriteLine($@"
{i + 1}. Product: {cart.Product.Name}
   Price: ₹{cart.Product.Price}
   Quantity: {cart.CartQuantity}
   Subtotal: ₹{cart.Product.Price * cart.CartQuantity}");
            }

            decimal totalAmount = this._cartServices.GetCartTotal(userName);

            Console.WriteLine($@"
==================================
Total Amount: ₹{totalAmount}
==================================");

            Console.Write(@"
[C]heckout
[B]ack to Main Menu

Enter Choice: ");

            ConsoleKey userChoice = Console.ReadKey().Key;
            Console.Clear();

            switch (userChoice)
            {
                case ConsoleKey.C:
                    this.Checkout(userName);
                    break;

                case ConsoleKey.B:
                    return;

                default:
                    ConsoleHelper.WriteErrorMessage("\nInvalid choice.\n");
                    ConsoleHelper.CleanConsole();
                    break;
            }
        }

        /// <summary>
        /// Handles the checkout process and retrieves delivery details.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        private void Checkout(string userName)
        {
            string deliveryAddress;

            while (true)
            {
                Console.Write("\nEnter delivery address: ");
                deliveryAddress = Console.ReadLine() ?? string.Empty;

                string validatorMessage = Validator.ValidateDeliveryAddress(deliveryAddress);

                if (validatorMessage != "passed")
                {
                    ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                    continue;
                }

                deliveryAddress = deliveryAddress.Trim();
                break;
            }

            string deliveryTime;

            while (true)
            {
                Console.Write("\nEnter delivery time (HH:mm) [08:00 - 22:00]: ");
                deliveryTime = Console.ReadLine() ?? string.Empty;

                string validatorMessage = Validator.ValidateDeliveryTime(deliveryTime);

                if (validatorMessage != "passed")
                {
                    ConsoleHelper.WriteErrorMessage($"\n{validatorMessage}\n");
                    continue;
                }

                deliveryTime = deliveryTime.Trim();
                break;
            }

            List<Cart> carts = this._cartServices.GetCart(userName);
            decimal totalAmount = this._cartServices.GetCartTotal(userName);

            Console.WriteLine(@"
==================================
           ORDER SUMMARY
==================================");

            foreach (Cart cart in carts)
            {
                Console.WriteLine($@"
Product: {cart.Product.Name}
Price: ₹{cart.Product.Price}
Quantity: {cart.CartQuantity}
Subtotal: ₹{cart.Product.Price * cart.CartQuantity}");
            }

            Console.WriteLine($@"
Delivery Address: {deliveryAddress}
Delivery Time: {deliveryTime}

==================================
Total Amount: ₹{totalAmount}
==================================");

            Console.Write("\nConfirm Order? [Y/N]: ");

            ConsoleKey confirmation = Console.ReadKey().Key;
            Console.WriteLine();

            if (confirmation != ConsoleKey.Y)
            {
                ConsoleHelper.WriteErrorMessage("\nOrder cancelled. Your cart has been retained.\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            Order? order = this._cartServices.Checkout(
                userName,
                deliveryAddress,
                deliveryTime);

            if (order == null)
            {
                ConsoleHelper.WriteErrorMessage(
                    "\nCheckout failed. Please check the available stock.\n");

                ConsoleHelper.CleanConsole();
                return;
            }

            ConsoleHelper.WriteSuccessMessage("\nOrder placed successfully!\n");

            Console.WriteLine($@"
==================================
          ORDER CONFIRMED
==================================
Order ID: {order.OrderId}
Customer: {order.UserName}
Delivery Address: {order.DeliveryAddress}
Delivery Time: {order.DeliveryTime}
Order Date: {order.OrderDate}

Total Amount: ₹{order.TotalAmount}
==================================");

            ConsoleHelper.CleanConsole();
        }
    }
}
