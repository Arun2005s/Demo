using GroceryShopApp.Model;
using GroceryShopApp.Repository;

namespace GroceryShopApp.Services
{
    /// <summary>
    /// Handles the business logics for customer order history.
    /// </summary>
    internal class OrderServices
    {
        private readonly OrderRepository _orderRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderServices"/> class.
        /// </summary>
        /// <param name="orderRepository">Repository of the orders.</param>
        public OrderServices(OrderRepository orderRepository)
        {
            this._orderRepository = orderRepository;
        }

        /// <summary>
        /// Retrieves the order history of a particular user.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        /// <returns>List of customer orders.</returns>
        internal List<Order> GetOrderHistory(string userName)
        {
            return this._orderRepository.GetUserOrders(userName);
        }
    }
}
