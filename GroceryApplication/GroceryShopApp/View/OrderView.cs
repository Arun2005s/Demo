using GroceryShopApp.Model;
using GroceryShopApp.Services;

namespace GroceryShopApp.View
{
    /// <summary>
    /// Performs the view level operations for customer order history.
    /// </summary>
    internal class OrderView
    {
        private readonly OrderServices _orderServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderView"/> class.
        /// </summary>
        /// <param name="orderServices">Services of the orders.</param>
        public OrderView(OrderServices orderServices)
        {
            this._orderServices = orderServices;
        }

        /// <summary>
        /// Displays the order history of the current user.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        internal void DisplayOrderHistory(string userName)
        {
            List<Order> orders = this._orderServices.GetOrderHistory(userName);

            Console.WriteLine(@"
==================================
          ORDER HISTORY
==================================");

            if (orders.Count == 0)
            {
                ConsoleHelper.WriteErrorMessage("\nNo previous orders found.\n");
                ConsoleHelper.CleanConsole();
                return;
            }

            foreach (Order order in orders)
            {
                Console.WriteLine($@"
==================================
Order ID: {order.OrderId}
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
